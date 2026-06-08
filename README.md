# MISIL — Project Management Platform

> **MISIL** (pronounced "missile") — a full-stack Jira-like project management platform where teams lock onto targets until completion. Discord-style UI with a royal green theme.

---

## Table of Contents

1. [Technology Stack](#technology-stack)
2. [Architecture Overview](#architecture-overview)
3. [Backend Architecture](#backend-architecture)
4. [Frontend Architecture](#frontend-architecture)
5. [Database Schema](#database-schema)
6. [API Endpoints](#api-endpoints)
7. [Real-Time Features](#real-time-features)
8. [Authentication & Authorization](#authentication--authorization)
9. [Key Design Decisions](#key-design-decisions)
10. [Project Structure](#project-structure)
11. [Setup & Running](#setup--running)

---

## Technology Stack

### Backend
| Technology | Purpose |
|---|---|
| **.NET 10** (ASP.NET Core) | Web API framework |
| **C# 14** | Language |
| **Entity Framework Core 10** | ORM for database access |
| **PostgreSQL 16** (via Npgsql) | Relational database |
| **MediatR 14** | CQRS — command/query separation |
| **FluentValidation 12** | Request validation pipeline |
| **Mapster 10** | Object mapping (DTOs ↔ Entities) |
| **SignalR** | Real-time WebSocket communication |
| **JWT Bearer Auth** | Authentication |
| **ASP.NET Core Identity** | User management (customized) |

### Frontend
| Technology | Purpose |
|---|---|
| **React 19** | UI framework |
| **TypeScript 6** | Type-safe JavaScript |
| **Vite 8** | Build tool & dev server |
| **Tailwind CSS 4** | Utility-first CSS |
| **React Router 7** | Client-side routing |
| **SignalR (@microsoft/signalr)** | Real-time client |
| **Axios** | HTTP client |
| **Radix UI** | Headless UI primitives (Avatar, Dialog, Dropdown, Toast, etc.) |
| **Lucide React** | Icon library |
| **Recharts** | Charting (stats page) |
| **class-variance-authority** | Component variant management |
| **tailwind-merge / clsx** | Class name merging |

---

## Architecture Overview

MISIL follows **Clean Architecture** (ports & adapters) on the backend and a **feature-based component hierarchy** on the frontend.

```
┌─────────────────────────────────────────────────┐
│                   Frontend                       │
│  React 19 + TypeScript + Tailwind CSS 4          │
│  ┌───────────┐ ┌──────────────┐ ┌────────────┐  │
│  │   Pages   │ │  Components  │ │   Hooks    │  │
│  │ (Routing) │ │   (Reusable) │ │  (State)   │  │
│  └─────┬─────┘ └──────┬───────┘ └─────┬──────┘  │
│        └──────────────┼───────────────┘         │
│                       │ HTTP/WebSocket           │
│               ┌───────┴────────┐                 │
│               │   Vite Proxy   │                 │
│               └───────┬────────┘                 │
└───────────────────────┼─────────────────────────┘
                        │ /api, /hubs
┌───────────────────────┼─────────────────────────┐
│           Backend (ASP.NET Core 10)              │
│  ┌─────────────────────────────────────────────┐ │
│  │         API Layer (Misil.Api)               │ │
│  │  Controllers · Middleware · Hubs · Services  │ │
│  └────────────────────┬────────────────────────┘ │
│  ┌────────────────────┼────────────────────────┐ │
│  │    Application Layer (Misil.Application)     │ │
│  │  Commands · Queries · Handlers · Behaviors   │ │
│  │  DTOs · Validators · Interfaces · Services   │ │
│  └────────────────────┬────────────────────────┘ │
│  ┌────────────────────┼────────────────────────┐ │
│  │      Domain Layer (Misil.Domain)            │ │
│  │  Entities · Enums · Repository Interfaces   │ │
│  └────────────────────┬────────────────────────┘ │
│  ┌────────────────────┼────────────────────────┐ │
│  │  Infrastructure Layer (Misil.Infrastructure) │ │
│  │  DbContext · Repositories · EF Configs       │ │
│  │  JwtService · FileStorage · UserService      │ │
│  └────────────────────┬────────────────────────┘ │
│                       │                          │
│              ┌────────┴────────┐                 │
│              │   PostgreSQL    │                 │
│              └─────────────────┘                 │
└─────────────────────────────────────────────────┘
```

### Layering Rules

- **API → Application**: Controllers depend only on Application (MediatR interfaces, DTOs)
- **Application → Domain**: Commands/Queries depend only on Domain interfaces & entities
- **Infrastructure → Application**: Implements Application interfaces, references Domain
- **Domain**: Innermost layer — no dependencies on other layers

---

## Backend Architecture

### 1. Domain Layer (`Misil.Domain`)

The domain layer contains enterprise-wide business rules and entities. It has **zero dependencies** on other project layers.

#### Entities (22 files)

| File | Description |
|---|---|
| `Entities/User.cs` | Extends `IdentityUser<Guid>`. Adds `DisplayName`, `AvatarUrl`, `CreatedAt`. Contains navigation properties to project memberships, teams, reported/assigned issues, comments, and chat messages. |
| `Entities/Project.cs` | Core project entity with `Name`, `Description`, `Key` (unique abbreviation), `LeadUserId`, `IsArchived`. Has `AddMember()` factory method. |
| `Entities/ProjectMember.cs` | Join entity linking `User` → `Project` with a `RoleId`. |
| `Entities/ProjectInvitation.cs` | Pending invitation with `Status` (Pending/Accepted/Denied), `InvitedUserId`, `InvitedByUserId`, `RoleId`. |
| `Entities/Issue.cs` | The central work item. Fields: `Title`, `Description`, `IssueType` (enum), `Status` (enum), `Priority` (enum), `StoryPoints`, `Order`, `StartDate`, `DueDate`, `EstimatedHours`, `AssigneeId`, `AssigneeTeamId` (mutually exclusive). Has subtasks, comments, attachments, relations, dependencies, and sprint mappings. |
| `Entities/IssueComment.cs` | Comments on issues (`Content`, `CreatedAt`, `UpdatedAt`, `IsEdited`). |
| `Entities/IssueAttachment.cs` | File attachments with `FileName`, `FileUrl`, `Size`. |
| `Entities/IssueRelation.cs` | Issue-to-issue relationships (`RelatesTo`, `Blocks`, `IsBlockedBy`, `Duplicates`). |
| `Entities/IssueDependency.cs` | Gantt dependencies between issues (`FinishToStart`, `StartToStart`, etc.). |
| `Entities/IssueTag.cs` | Tagging system (`Name`, `Color`) per project. |
| `Entities/IssueTagMapping.cs` | Many-to-many join: Issues ↔ Tags. |
| `Entities/Sprint.cs` | Time-boxed iteration with `Name`, `Goal`, `StartDate`, `EndDate`, `IsActive`, `IsCompleted`. |
| `Entities/SprintIssue.cs` | Join entity: Sprint ↔ Issue. |
| `Entities/Team.cs` | Sub-group within a project. Has `AddMember()` / `RemoveMember()` methods. |
| `Entities/TeamMember.cs` | Join entity: User ↔ Team with `RoleId`. |
| `Entities/ChatChannel.cs` | Chat channel within a project, optionally scoped to a team. Supports direct messages and private channels. |
| `Entities/ChatMessage.cs` | Individual chat messages with read/edited tracking. |
| `Entities/ChatMessageRead.cs` | Composite key (UserId, MessageId) tracking read receipts. |
| `Entities/AuditLog.cs` | Activity log entry recording who did what to which entity, with old/new values. |
| `Entities/Notification.cs` | User notifications (invitations, mentions, etc.). |
| `Entities/Milestone.cs` | Project milestones with due dates. |
| `Entities/Role.cs` | Custom project roles with a `Permissions` string (comma-separated or `"all"` for owner). |

#### Enums (6 files)

| File | Values |
|---|---|
| `Enums/IssueType.cs` | Epic, Story, Task, Bug, Subtask |
| `Enums/IssueStatus.cs` | ToDo, InProgress, InReview, Done, Cancelled |
| `Enums/IssuePriority.cs` | Highest, High, Medium, Low, Lowest |
| `Enums/RelationType.cs` | RelatesTo, Blocks, IsBlockedBy, Duplicates |
| `Enums/DependencyType.cs` | FinishToStart, StartToStart, FinishToFinish, StartToFinish |
| `Enums/EntityType.cs` | Project, Issue, Sprint, Team, ChatChannel, Member, Role, Invitation |

#### Repository Interfaces (10 files)

Each defines the contract for data access. Implemented in Infrastructure.

| Interface | Key Methods |
|---|---|
| `IProjectRepository` | CRUD + GetUserProjects, AddMember, RemoveMember, UpdateMemberRole |
| `IIssueRepository` | CRUD + GetFiltered, AddComment, AddAttachment |
| `IChatRepository` | CreateChannel, GetChannels, AddMessage, GetMessages, MarkAsRead |
| `ISprintRepository` | CRUD + GetByProjectId, GetActiveSprint, AddIssueToSprint, RemoveIssueFromSprint |
| `ITeamRepository` | CRUD + GetByProjectId |
| `IRoleRepository` | CRUD + GetByName |
| `IAuditLogRepository` | Add + GetFiltered (paginated) |
| `IUserRepository` | GetById, GetByEmail, Add, Update |
| `IGanttRepository` | GetGanttData (issues + dependencies for Gantt chart) |
| `IFileStorageService` | Save, Delete, GetUrl (local disk) |

---

### 2. Application Layer (`Misil.Application`)

Orchestrates business logic through the **CQRS pattern** (Command Query Responsibility Segregation) using MediatR.

#### Modules

| Module | Files | Description |
|---|---|---|
| **Auth/** | 6 files | Register, Login, UpdateProfile commands + GetCurrentUser query |
| **Issues/** | 9 files | CRUD + Assign, UpdateStatus, Move, AddComment commands + queries |
| **Projects/** | 9 files | CRUD + AddMember, RemoveMember, UpdateRole commands + queries |
| **Chat/** | 7 files | CreateChannel, SendMessage, MarkRead commands + queries |
| **Sprints/** | 10 files | CRUD + Start, Complete, AddIssueToSprint, RemoveIssueFromSprint + queries |
| **Teams/** | 9 files | CRUD + AddMember, RemoveMember commands + queries |
| **Roles/** | 3 files | Create, Delete commands |
| **Gantt/** | 4 files | CreateDependency, DeleteDependency commands + GetGanttData query |
| **Logs/** | 1 file | GetAuditLogs query (paginated, filterable) |

#### Cross-Cutting

| File/Directory | Description |
|---|---|
| `Behaviors/ValidationBehavior.cs` | MediatR pipeline behavior — runs FluentValidation validators before handler execution |
| `Behaviors/LoggingBehavior.cs` | MediatR pipeline behavior — logs every command/query |
| `Common/DTOs/PagedResult.cs` | Generic paginated response wrapper |
| `Common/DTOs/AuditLogEntryDto.cs` | DTO for activity log entries |
| `Common/Exceptions/NotFoundException.cs` | Domain exception → HTTP 404 via middleware |
| `Common/Interfaces/ICurrentUserService.cs` | Abstraction for the current HTTP user's identity |
| `Common/Interfaces/IJwtService.cs` | Abstraction for JWT token generation |
| `Common/Services/IAuditService.cs` | Interface for audit logging |
| `Common/Services/AuditService.cs` | Implementation — creates an AuditLog entry on each action |
| `DependencyInjection.cs` | Registers MediatR, validators, pipeline behaviors, AuditService |

#### How a typical request flows:

```
HTTP Request
  → Controller (validates auth, permission check via IPermissionService)
    → MediatR.Send(Command/Query)
      → [ValidationBehavior] (FluentValidation)
        → [LoggingBehavior]
          → Handler (business logic, calls repository interface)
            → Repository (Infrastructure — EF Core)
              → PostgreSQL
  ← Response DTO ← Controller ← HTTP Response
```

---

### 3. Infrastructure Layer (`Misil.Infrastructure`)

Persistence, external services, and cross-cutting implementations.

#### Data

| File | Description |
|---|---|
| `Data/MisilDbContext.cs` | EF Core DbContext extending `IdentityDbContext<User, IdentityRole<Guid>, Guid>`. Declares all `DbSet<>` properties and configures composite keys + entity configurations. |
| `Data/Configurations/` (22 files) | EF Core Fluent API configurations per entity (table mappings, relationships, indexes, constraints). |
| `Data/Seed/SeedData.cs` | Creates 4 project roles (Owner, Admin, Member, Viewer), 4 ASP.NET Identity roles (Admin, ProjectManager, Developer, Viewer), and 2 seed users: `admin@misil.dev` / `Admin123!` (Admin role) and `user@misil.dev` / `User123!` (Developer role). |

#### Repositories (8 files)

Each implements the corresponding Domain interface using EF Core:

| Repository | Notable Implementation Details |
|---|---|
| `ProjectRepository` | `GetUserProjectsAsync` includes `.Include(p => p.Members)` for member count |
| `IssueRepository` | `GetFilteredAsync` applies dynamic filtering by status, sprint, assignee; includes comments + attachments |
| `SprintRepository` | `GetByProjectIdAsync` includes `.Include(s => s.SprintIssues).ThenInclude(si => si.Issue)` for story point computation |
| `ChatRepository` | Handles channel visibility based on team membership + private flags |
| `TeamRepository` | `GetByProjectIdAsync` includes `.Include(t => t.Members)` |
| `AuditLogRepository` | `GetFilteredAsync` supports pagination + filters (entity type, user, date range) |
| `UserRepository` | Simple CRUD wrapping UserManager |
| `RoleRepository` | CRUD for custom project roles |
| `GanttRepository` | Returns issues with start/due dates + dependencies for Gantt visualization |

#### Services (3 files)

| Service | Description |
|---|---|
| `Services/JwtService.cs` | Generates JWT tokens with sub, email, and name claims. 24-hour expiry. Uses HMAC-SHA256. |
| `Services/CurrentUserService.cs` | Reads `UserId` (from `ClaimTypes.NameIdentifier` or `JwtRegisteredClaimNames.Sub`) and `UserName` (from `ClaimTypes.Name` or `JwtRegisteredClaimNames.Name`) from the HTTP context. |
| `Services/FileStorageService.cs` | Saves files to `{BasePath}/{entityType}/{entityId}/` on local disk. Returns relative URL paths. |

#### DependencyInjection.cs

Registers:
- EF Core DbContext with PostgreSQL connection string
- ASP.NET Core Identity with `UserManager`, `RoleManager`, `SignInManager`
- JWT Bearer authentication with token validation parameters
- SignalR token passthrough via query string for WebSocket connections
- All repositories and services as scoped dependencies

---

### 4. API Layer (`Misil.Api`)

Entry point for HTTP requests. Contains controllers, SignalR hub, middleware, and permission service.

#### Controllers (12 files)

| Controller | Route | Endpoints | Permission Check |
|---|---|---|---|
| **AuthController** | `/api/auth` | POST register, POST login, GET me, PUT profile | None (public) |
| **ProjectsController** | `/api/projects` | GET all, GET by id, POST create, PUT update, DELETE — plus member management (GET, POST, PATCH, DELETE) and `/my-permissions` | `edit_project`, `invite_members`, `remove_members` |
| **IssuesController** | `/api/projects/{projectId}/issues` | GET all (filtered), GET by id, POST create, PUT update, DELETE, PATCH status, PATCH assign, POST comments | `create_issues` |
| **SprintsController** | `/api/projects/{projectId}/sprints` | GET all, GET active, GET by id, POST create, POST start, POST complete, POST add-issue, DELETE remove-issue | `manage_sprints` |
| **TeamsController** | `/api/projects/{projectId}/teams` | GET all, POST create, PUT update, DELETE, GET members, POST add-member, DELETE remove-member | `manage_teams` |
| **ChatController** | `/api/projects/{projectId}/chat` | GET channels, POST create-channel, GET messages, POST send-message, POST mark-read | `manage_channels` |
| **RolesController** | `/api/projects/{projectId}/roles` | GET all, POST create, PATCH update, DELETE | `manage_roles` |
| **GanttController** | `/api/projects/{projectId}/gantt` | GET data, POST dependency, DELETE dependency | None (read-only) |
| **StatsController** | `/api/projects/{projectId}/stats` | GET project statistics | None |
| **LogsController** | `/api/projects/{projectId}/logs` | GET audit logs (paginated, filterable) | None |
| **NotificationsController** | `/api/notifications` | GET my notifications, PATCH mark-read, POST mark-all-read | None |
| **InvitationsController** | `/api/invitations` | GET pending, POST accept, POST deny | None |

#### Hubs

| Hub | Route | Methods |
|---|---|---|
| **ProjectHub** | `/hubs/project` | `JoinProject`, `LeaveProject`, `JoinChannel`, `LeaveChannel` |

#### Middleware

| Middleware | Description |
|---|---|
| **ExceptionMiddleware** | Global exception handler — maps `NotFoundException` → 404, `UnauthorizedAccessException` → 401, `ValidationException` → 400, all others → 500. Returns JSON `{ error: string }`. |

#### Services (API Layer)

| Service | Description |
|---|---|
| **IPermissionService** / **PermissionService** | Reads the current user's project role from `ProjectMembers` + `Roles` tables and checks if the role's permission string contains the required permission (or is `"all"` for Owner). |
| **AllPermissions** (static) | Central list of all permission strings: `view_stats`, `invite_members`, `remove_members`, `manage_roles`, `edit_project`, `manage_teams`, `manage_channels`, `create_issues`, `manage_sprints` |

#### Program.cs Setup

The application entry point:
1. Registers Application and Infrastructure services
2. Configures controllers with JSON string enum serialization + cycle handling
3. Registers `PermissionService`, SignalR, and CORS (allows `localhost:5173` with credentials)
4. Adds OpenAPI in development
5. Configures middleware pipeline: ExceptionMiddleware → CORS → Auth → Authorization → Controllers → SignalR Hub
6. On startup: calls `EnsureCreatedAsync()` (creates DB if not exists), runs raw SQL for `AuditLogs.UserName` and `Issues.AssigneeTeamId` columns (idempotent via `IF NOT EXISTS`), creates `ProjectInvitations` table if missing, then seeds data.

---

## Frontend Architecture

### State Management

No external state library — the app uses **React Context** for global state:

| Context | File | State |
|---|---|---|
| **AuthContext** | `hooks/useAuth.tsx` | Current user, login/register/logout functions, `updateUser` |
| **ProjectContext** | `hooks/useProject.tsx` | Currently selected project, select/clear functions |

### Hooks

| Hook | File | Description |
|---|---|---|
| `useAuth` | `hooks/useAuth.tsx` | Authentication context consumer + provider. Stores JWT in `localStorage`, auto-loads user on mount via `GET /auth/me`. |
| `useProject` | `hooks/useProject.tsx` | Project context — tracks which project the user is currently viewing. |
| `useSignalR` | `hooks/useSignalR.ts` | Creates a single SignalR connection to `/hubs/project` with auto-reconnect. Exposes `joinProject(projectId)`, `on(event, handler)`. Used by Board, Backlog, and Sprint pages for real-time updates. |
| `usePermissions` | `hooks/usePermissions.ts` | Fetches the user's permissions for the current project via `GET /projects/{id}/my-permissions`. Exposes `can(permission)` helper. |
| `use-toast` | `hooks/use-toast.ts` | Toast notification system — reducer-based with auto-dismiss after 5 seconds. |

### Routing (App.tsx)

```
/login          → PublicRoute → LoginPage
/register       → PublicRoute → RegisterPage
/projects       → ProtectedRoute → AppShell → ProjectListPage
/board          → ProtectedRoute → AppShell → RequireProject → BoardPage
/backlog        → ProtectedRoute → AppShell → RequireProject → BacklogPage
/sprint         → ProtectedRoute → AppShell → RequireProject → SprintDetailPage
/chat           → ProtectedRoute → AppShell → RequireProject → ChatPage
/gantt          → ProtectedRoute → AppShell → RequireProject → GanttPage
/teams          → ProtectedRoute → AppShell → RequireProject → TeamsPage
/stats          → ProtectedRoute → AppShell → RequireProject → StatsPage
/members        → ProtectedRoute → AppShell → RequireProject → MembersPage
/roles          → ProtectedRoute → AppShell → RequireProject → RolesPage
/logs           → ProtectedRoute → AppShell → RequireProject → LogsPage
/settings       → ProtectedRoute → AppShell → SettingsPage
*               → Redirect to /projects
```

### Pages (12 pages)

| Page | Route | Description |
|---|---|---|
| **LoginPage** | `/login` | Glassmorphism card with animated logo, email/password inputs, show/hide toggle, error banner, spinner loading. Demo credentials hint. |
| **RegisterPage** | `/register` | Glass card with username, display name, email, password fields. Same styling as login. |
| **ProjectListPage** | `/projects` | Gradient-bannered project cards, pending invitations section with Accept/Deny, empty state CTA, create-project dialog. |
| **BoardPage** | `/board` | Kanban board with 4 columns (To Do, In Progress, In Review, Done). HTML5 drag-and-drop with optimistic UI + rollback. Search bar, create-issue dialog, issue detail slide-over. Real-time updates via SignalR. |
| **BacklogPage** | `/backlog` | Flat list of non-Done issues with type icons, priority colors, assignee badges. Create/delete with permissions. SignalR updates. |
| **SprintDetailPage** | `/sprint` | Shows active sprint with progress bar (story points), issue list, add-issue dialog. Planned and past sprint sections. Start sprint controls. SignalR updates. |
| **ChatPage** | `/chat` | Two-panel layout: channel list (left) + message thread (right). Create channels (project-wide or team-scoped). Send messages with enter key. |
| **GanttPage** | `/gantt` | Custom Gantt chart with day grid, weekend shading, "Today" marker, gradient bars per status, dependency arrows (SVG curves), issue tooltips on hover. |
| **TeamsPage** | `/teams` | Team cards with expandable member lists. Add/remove members with popover. Create team dialog. |
| **StatsPage** | `/stats` | Dashboard with Recharts — bar charts for status/type/priority distribution, numeric stat cards. |
| **MembersPage** | `/members` | Member list with avatars, email, join date, role select (or read-only display based on permission). Invite dialog with email + role. Remove member. |
| **RolesPage** | `/roles` | Role management — view existing roles, create, edit permissions, delete (except Owner). |
| **LogsPage** | `/logs` | Paginated activity feed with entity type badges, user names, timestamps, old/new value display. |
| **SettingsPage** | `/settings` | Display name edit (calls `PUT /api/auth/profile`), theme toggle (persisted to localStorage), account info. |

### Shared Components

| Component | Description |
|---|---|
| **AppShell** | Main layout wrapper — Sidebar + TopBar + `<Outlet>` + StatusBar |
| **Sidebar** | Glass sidebar (240px, collapsible). Gradient logo badge, "General" nav (Projects, Settings), "Project" context nav (Board, Backlog, Sprint, Chat, Gantt, Teams, Stats, Members, Roles, Activity Log). Active item with green border + subtle shadow. |
| **TopBar** | Glass top bar with sidebar toggle, welcome message, notification bell with unread pulse dot + dropdown, profile avatar with hover dropdown (Settings, Sign out). |
| **StatusBar** | Bottom bar with animated green "Connected" ping dot + MISIL version. |
| **CreateIssueDialog** | Modal dialog for creating issues — title, type, priority, description, assignee (member or team, mutually exclusive), sprint, story points, dates. |
| **IssueDetailPanel** | Slide-over panel from right — inline editing for title, description, type, priority. Comments section with add. Attachments list. Assignee display. |

### UI Components (6 components)

All styled with Tailwind CSS 4 glassmorphism theme:

| Component | Key Styling |
|---|---|
| **Button** | Gradient backgrounds (`from-misil-600 to-misil-700`), shadow, glow on hover, `backdrop-blur-sm`. Variants: default, destructive, outline, secondary, ghost, link. |
| **Input** | `bg-surface-2/50` with `backdrop-blur-sm`, green ring on focus, placeholder in gray-600. |
| **Badge** | `backdrop-blur-sm` with translucent background, `rounded-lg`, colored borders per variant. |
| **Avatar** | Gradient fallback background, `ring-2 ring-surface-3/50`. |
| **Toast** | `backdrop-blur-xl` with `bg-surface-1/80`, `rounded-xl`, colored borders per variant (default, destructive, success). |
| **ScrollArea** | Radix ScrollArea with custom thumb styling. |
| **Separator** | Radix Separator — thin horizontal/vertical line. |
| **Toaster** | Combines ToastProvider with viewport rendering. |

---

## Database Schema

The database uses PostgreSQL 16 with UUID primary keys throughout. The schema is managed by **EF Core migrations** (currently using `EnsureCreatedAsync()` with manual ALTER TABLE SQL for late-added columns).

### Core Tables

```
Users (IdentityUser<Guid>)
├── PK: Id (uuid)
├── UserName, Email, PasswordHash, etc.
├── DisplayName, AvatarUrl, CreatedAt
│
├──┐ ProjectMembers (join: User ↔ Project)
│  ├── PK: Id (uuid) | UK: (ProjectId, UserId)
│  ├── FK → Users(UserId), FK → Projects(ProjectId)
│  ├── RoleId → Roles(Id)
│  └── JoinedAt
│
├──┐ TeamMembers (join: User ↔ Team)
│  ├── PK: Id (uuid) | UK: (TeamId, UserId)
│  ├── FK → Users(UserId), FK → Teams(TeamId)
│  └── RoleId, JoinedAt
│
├──┐ Issues (as Assignee)
│  └── FK → Users(AssigneeId)
│
├──┐ Issues (as Reporter)
│  └── FK → Users(ReporterId)
│
├──┐ IssueComments
│  └── FK → Users(UserId)
│
├── ChatMessages
│  └── FK → Users(UserId)
│
└── Notifications
    └── FK → Users(UserId)
```

```
Projects
├── PK: Id (uuid) | UK: Key (varchar 10)
├── Name, Description, Key, LeadUserId, IsArchived
├── CreatedAt, UpdatedAt
│
├──┐ ProjectMembers
│  └── FK → Projects(ProjectId)
│
├──┐ Teams
│  ├── PK: Id (uuid)
│  ├── Name, Description, CreatedAt
│  ├── FK → Projects(ProjectId)
│  └──┐ TeamMembers
│     └── FK → Teams(TeamId)
│
├──┐ Issues
│  ├── PK: Id (uuid)
│  ├── Title, Description, IssueType (varchar 20), Status (varchar 20)
│  ├── Priority (varchar 20), StoryPoints (int), Order (int)
│  ├── StartDate, DueDate, EstimatedHours (decimal)
│  ├── AssigneeId (nullable), AssigneeTeamId (nullable, mutually exclusive)
│  ├── FK → Projects(ProjectId), FK → Issues(ParentIssueId)
│  ├── FK → Users(AssigneeId), FK → Users(ReporterId)
│  │
│  ├──┐ IssueComments
│  │  └── FK → Issues(IssueId)
│  │
│  ├──┐ IssueAttachments
│  │  └── FK → Issues(IssueId)
│  │
│  ├──┐ IssueRelations
│  │  └── FK → Issues(IssueId), FK → Issues(RelatedIssueId)
│  │
│  ├──┐ IssueDependencies
│  │  └── FK → Issues(IssueId), FK → Issues(DependsOnIssueId)
│  │
│  └──┐ IssueTagMappings (composite PK: IssueId, TagId)
│     └── FK → Issues(IssueId), FK → IssueTags(TagId)
│
├──┐ Sprints
│  ├── PK: Id (uuid)
│  ├── Name, Goal, StartDate, EndDate, IsActive, IsCompleted
│  ├── FK → Projects(ProjectId)
│  └──┐ SprintIssues
│     └── FK → Sprints(SprintId), FK → Issues(IssueId)
│
├──┐ ChatChannels
│  ├── PK: Id (uuid)
│  ├── Name, IsDirectMessage, IsPrivate
│  ├── FK → Projects(ProjectId), FK → Teams(TeamId)
│  └──┐ ChatMessages
│     ├── PK: Id (uuid)
│     ├── Content, IsEdited, IsRead
│     ├── FK → ChatChannels(ChannelId), FK → Users(UserId)
│     └──┐ ChatMessageReads (composite PK: UserId, MessageId)
│        └── FK → Users(UserId), FK → ChatMessages(MessageId)
│
├──┐ AuditLogs
│  └── FK → Projects(ProjectId), FK → Users(UserId)
│
├──┐ Milestones
│  └── FK → Projects(ProjectId)
│
├──┐ IssueTags
│  └── FK → Projects(ProjectId) | UK: (ProjectId, Name)
│
└──┐ ProjectInvitations
   └── FK → Projects(ProjectId), FK → Users(InvitedUserId), FK → Users(InvitedByUserId)
```

### ASP.NET Identity Tables
`AspNetRoles`, `AspNetRoleClaims`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserRoles`, `AspNetUserTokens` — standard Identity tables for user management.

### Custom Tables
`Roles` (project-level roles with permissions string), `Notifications` (per-user notification inbox).

---

## API Endpoints

### Authentication (`/api/auth`)
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login, returns JWT |
| GET | `/api/auth/me` | Yes | Get current user |
| PUT | `/api/auth/profile` | Yes | Update display name / avatar |

### Projects (`/api/projects`)
| Method | Path | Permission | Description |
|---|---|---|---|
| GET | `/api/projects` | — | List user's projects |
| GET | `/api/projects/{id}` | — | Get project details + members |
| POST | `/api/projects` | — | Create project (creator becomes Owner) |
| PUT | `/api/projects/{id}` | `edit_project` | Update project |
| DELETE | `/api/projects/{id}` | `edit_project` | Delete project |
| GET | `/api/projects/{id}/members` | — | List project members |
| POST | `/api/projects/{id}/members` | `invite_members` | Invite user (creates invitation) |
| PATCH | `/api/projects/{id}/members/{userId}/role` | `invite_members` | Change member role |
| DELETE | `/api/projects/{id}/members/{userId}` | `remove_members` | Remove member |
| GET | `/api/projects/{id}/my-permissions` | — | Get current user's permissions |

### Issues (`/api/projects/{projectId}/issues`)
| Method | Path | Permission | Description |
|---|---|---|---|
| GET | `/?status=&sprintId=&assigneeId=&page=&pageSize=` | — | List issues (filterable) |
| GET | `/{id}` | — | Get issue detail (with comments & attachments) |
| POST | `/` | `create_issues` | Create issue |
| PUT | `/{id}` | `create_issues` | Update issue |
| DELETE | `/{id}` | `create_issues` | Delete issue |
| PATCH | `/{id}/status` | — | Change status (drag-and-drop) |
| PATCH | `/{id}/assign` | `create_issues` | Change assignee |
| POST | `/{id}/comments` | — | Add comment |

### Sprints (`/api/projects/{projectId}/sprints`)
| Method | Path | Permission | Description |
|---|---|---|---|
| GET | `/` | — | List all sprints |
| GET | `/active` | — | Get active sprint |
| GET | `/{id}` | — | Get sprint by ID |
| POST | `/` | `manage_sprints` | Create sprint |
| POST | `/{id}/start` | `manage_sprints` | Start sprint |
| POST | `/{id}/complete` | `manage_sprints` | Complete sprint |
| POST | `/{sprintId}/issues/{issueId}` | `manage_sprints` | Add issue to sprint |
| DELETE | `/{sprintId}/issues/{issueId}` | `manage_sprints` | Remove issue from sprint |

### Teams (`/api/projects/{projectId}/teams`)
| Method | Path | Permission | Description |
|---|---|---|---|
| GET | `/` | — | List teams |
| POST | `/` | `manage_teams` | Create team |
| PUT | `/{id}` | `manage_teams` | Update team |
| DELETE | `/{id}` | `manage_teams` | Delete team |
| GET | `/{teamId}/members` | — | List team members |
| POST | `/{teamId}/members/{userId}` | `manage_teams` | Add member |
| DELETE | `/{teamId}/members/{userId}` | `manage_teams` | Remove member |

### Chat (`/api/projects/{projectId}/chat`)
| Method | Path | Permission | Description |
|---|---|---|---|
| GET | `/channels` | — | List channels |
| POST | `/channels` | `manage_channels` | Create channel |
| GET | `/channels/{channelId}/messages` | — | Get messages |
| POST | `/channels/{channelId}/messages` | — | Send message |
| POST | `/messages/{messageId}/read` | — | Mark message read |

### Roles (`/api/projects/{projectId}/roles`)
| Method | Path | Permission | Description |
|---|---|---|---|
| GET | `/` | — | List roles |
| POST | `/` | `manage_roles` | Create role |
| PATCH | `/{roleId}` | `manage_roles` | Update role |
| DELETE | `/{roleId}` | `manage_roles` | Delete role |

### Gantt (`/api/projects/{projectId}/gantt`)
| Method | Path | Description |
|---|---|---|
| GET | `/` | Get Gantt chart data (issues + dependencies) |
| POST | `/dependencies` | Create dependency |
| DELETE | `/dependencies/{id}` | Delete dependency |

### Other
| Method | Path | Description |
|---|---|---|
| GET | `/api/projects/{projectId}/stats` | Project statistics (counts by status/type/priority) |
| GET | `/api/projects/{projectId}/logs` | Paginated audit log |
| GET | `/api/notifications` | User notifications |
| PATCH | `/api/notifications/{id}/read` | Mark notification read |
| POST | `/api/notifications/read-all` | Mark all notifications read |
| GET | `/api/invitations/pending` | Get pending invitations |
| POST | `/api/invitations/{id}/accept` | Accept invitation |
| POST | `/api/invitations/{id}/deny` | Deny invitation |

---

## Real-Time Features (SignalR)

MISIL uses a **single SignalR hub** for all real-time communication.

### Hub: `ProjectHub` at `/hubs/project`

**Client-to-Server methods:**
- `JoinProject(projectId)` — join the SignalR group for a project
- `LeaveProject(projectId)` — leave the group
- `JoinChannel(channelId)` — join a chat channel group
- `LeaveChannel(channelId)` — leave a chat channel group

**Server-to-Client events** (pushed from controllers):
| Event | Payload | Triggered By |
|---|---|---|
| `IssueCreated` | `IssueDto` | IssuesController.Create |
| `IssueUpdated` | `IssueDto` | IssuesController.Update |
| `IssueDeleted` | `Guid (id)` | IssuesController.Delete |
| `IssueStatusChanged` | `(Guid id, IssueStatus status)` | IssuesController.UpdateStatus |
| `IssueAssigned` | `(Guid id, Guid? assigneeId, Guid? assigneeTeamId)` | IssuesController.Assign |
| `ChannelCreated` | `ChannelDto` | ChatController.CreateChannel |
| `MessageSent` | `MessageDto` | ChatController.SendMessage |

**Frontend usage:**
- `useSignalR` hook creates a single connection per user session with auto-reconnect
- Pages join the project group on mount, subscribe to events, and reload data on notification
- The connection is established when the user logs in and torn down on logout

---

## Authentication & Authorization

### Authentication (JWT)
- JWT tokens generated by `JwtService` with claims: `sub` (user ID), `email`, `name` (username)
- 24-hour expiry, HMAC-SHA256 signing
- SignalR passes token via `access_token` query string parameter (handled in `JwtBearerEvents.OnMessageReceived`)
- Frontend stores token in `localStorage`, attaches via Axios interceptor

### Authorization (Permission-based)
- **Project-level roles**: Owner, Admin, Member, Viewer (custom `Roles` table, not ASP.NET Identity roles)
- **Permissions**: stored as comma-separated string in `Role.Permissions`; `"all"` grants everything (Owner)
- `PermissionService` checks: find user's project membership → get role → check permission string
- Permissions enforced on controllers via `IPermissionService.HasPermissionAsync()`
- Frontend hides UI controls based on user's permissions (fetched via `/my-permissions`)
- ASP.NET Identity roles (Admin, ProjectManager, Developer, Viewer) used only for app-level role management

### Permission Matrix

| Permission | Owner | Admin | Member | Viewer |
|---|---|---|---|---|
| all | ✅ | — | — | — |
| view_stats | ✅ | ✅ | ✅ | — |
| invite_members | ✅ | ✅ | — | — |
| remove_members | ✅ | ✅ | — | — |
| manage_roles | ✅ | ✅ | — | — |
| edit_project | ✅ | ✅ | — | — |
| manage_teams | ✅ | ✅ | — | — |
| manage_channels | ✅ | ✅ | — | — |
| create_issues | ✅ | ✅ | — | — |
| manage_sprints | ✅ | ✅ | — | — |

---

## Key Design Decisions

### 1. Clean Architecture with CQRS
- Separates read and write operations for clarity and testability
- MediatR pipeline behaviors enable cross-cutting concerns (validation, logging) without touching handlers
- Application layer has zero dependency on ASP.NET Core or EF Core

### 2. ASP.NET Identity with Custom Roles
- Uses `IdentityUser<Guid>` for user management (password hashing, email uniqueness)
- **Does not use** Identity's role-based authorization for project-level permissions
- Instead, a custom `Roles` table with string-based permissions provides flexible project-level access control

### 3. Single SignalR Hub
- All real-time features share one hub at `/hubs/project`
- Group-based routing by project ID (`project_{projectId}`) and channel ID (`channel_{channelId}`)
- Events pushed from controllers (not command handlers) to keep the Application layer free of SignalR dependency

### 4. EnsureCreatedAsync() over Migrations
- Uses `EnsureCreatedAsync()` for schema creation with raw SQL for late-added columns
- This avoids the complexity of EF Core migration management during rapid development
- Raw SQL uses `IF NOT EXISTS` pattern for idempotency

### 5. Mutual Exclusivity for Assignee
- An issue can be assigned to either a **member** (`AssigneeId`) or a **team** (`AssigneeTeamId`), not both
- Enforced at both frontend (clearing one when the other is selected) and backend (`InvalidOperationException`)

### 6. Glassmorphism UI Theme
- Dark base (`#080808`) with royal green accents (`misil-500`)
- `backdrop-blur-xl` + semi-transparent backgrounds for glass effect
- Gradient orbs, ambient backgrounds, animated logo, custom keyframe animations (float, glow, shimmer)
- All UI components have variants with backdrop blur and subtle borders

### 7. Vite Proxy for Development
- Frontend dev server proxies `/api` and `/hubs` to `http://localhost:5000` (the backend)
- No CORS issues in development; in production, backend serves static files or a reverse proxy handles routing

---

## Project Structure

```
MISIL/
├── init.sql                              # Full DB schema dump (EF Core migration SQL)
├── README.md                             # This file
│
├── backend/
│   └── src/
│       ├── Misil.sln                     # Solution file
│       │
│       ├── Misil.Domain/                 # Innermost layer — enterprise logic
│       │   ├── Misil.Domain.csproj
│       │   ├── Entities/
│       │   │   ├── AuditLog.cs           # Activity log entry
│       │   │   ├── ChatChannel.cs        # Chat channel (project/team scoped)
│       │   │   ├── ChatMessage.cs        # Chat message
│       │   │   ├── ChatMessageRead.cs    # Read receipt (composite key)
│       │   │   ├── Issue.cs             # Main work item (45+ fields)
│       │   │   ├── IssueAttachment.cs   # File attachment on an issue
│       │   │   ├── IssueComment.cs      # Comment on an issue
│       │   │   ├── IssueDependency.cs   # Gantt dependency link
│       │   │   ├── IssueRelation.cs     # Issue relationship
│       │   │   ├── IssueTag.cs          # Tag definition
│       │   │   ├── IssueTagMapping.cs   # Many-to-many issue → tag
│       │   │   ├── Milestone.cs         # Project milestone
│       │   │   ├── Notification.cs      # User notification
│       │   │   ├── Project.cs           # Project aggregate root
│       │   │   ├── ProjectInvitation.cs # Pending invitation
│       │   │   ├── ProjectMember.cs     # User → project membership
│       │   │   ├── Role.cs              # Custom project role with permissions
│       │   │   ├── Sprint.cs            # Time-boxed iteration
│       │   │   ├── SprintIssue.cs       # Sprint ↔ issue assignment
│       │   │   ├── Team.cs              # Sub-team within a project
│       │   │   ├── TeamMember.cs        # User → team membership
│       │   │   └── User.cs              # IdentityUser<Guid> extension
│       │   ├── Enums/
│       │   │   ├── DependencyType.cs    # FinishToStart, etc.
│       │   │   ├── EntityType.cs        # Log entity types
│       │   │   ├── IssuePriority.cs     # Highest → Lowest
│       │   │   ├── IssueStatus.cs       # ToDo → Cancelled
│       │   │   ├── IssueType.cs         # Epic → Subtask
│       │   │   └── RelationType.cs      # RelatesTo, etc.
│       │   └── Interfaces/
│       │       ├── IAuditLogRepository.cs
│       │       ├── IChatRepository.cs
│       │       ├── IFIleStorageService.cs
│       │       ├── IGanttRepository.cs
│       │       ├── IIssueRepository.cs
│       │       ├── IProjectRepository.cs
│       │       ├── IRoleRepository.cs
│       │       ├── ISprintRepository.cs
│       │       ├── ITeamRepository.cs
│       │       └── IUserRepository.cs
│       │
│       ├── Misil.Application/           # Use cases / business logic
│       │   ├── Misil.Application.csproj
│       │   ├── DependencyInjection.cs   # Registers MediatR, validators, behaviors
│       │   ├── Auth/
│       │   │   ├── AuthResponse.cs      # Response DTO (token + user)
│       │   │   ├── Commands/
│       │   │   │   ├── Register.cs      # RegisterCommand + Handler
│       │   │   │   ├── Login.cs         # LoginCommand + Handler
│       │   │   │   └── UpdateProfile.cs # UpdateProfileCommand + Handler
│       │   │   ├── Queries/
│       │   │   │   └── GetCurrentUser.cs
│       │   │   └── Validators/
│       │   │       └── RegisterValidator.cs
│       │   ├── Issues/
│       │   │   ├── IssueDto.cs          # IssueDto, IssueDetailDto, CommentDto, AttachmentDto
│       │   │   ├── Commands/
│       │   │   │   ├── CreateIssue.cs
│       │   │   │   ├── UpdateIssue.cs
│       │   │   │   ├── DeleteIssue.cs
│       │   │   │   ├── AssignIssue.cs
│       │   │   │   ├── UpdateIssueStatus.cs
│       │   │   │   ├── MoveIssue.cs
│       │   │   │   └── AddComment.cs
│       │   │   └── Queries/
│       │   │       ├── GetIssues.cs
│       │   │       └── GetIssueById.cs
│       │   ├── Projects/
│       │   │   ├── ProjectDto.cs
│       │   │   ├── Commands/
│       │   │   │   ├── CreateProject.cs
│       │   │   │   ├── UpdateProject.cs
│       │   │   │   ├── DeleteProject.cs
│       │   │   │   ├── AddProjectMember.cs
│       │   │   │   ├── RemoveProjectMember.cs
│       │   │   │   └── UpdateProjectMemberRole.cs
│       │   │   └── Queries/
│       │   │       ├── GetProjects.cs
│       │   │       ├── GetProjectById.cs
│       │   │       └── GetProjectMembers.cs
│       │   ├── Chat/
│       │   │   ├── ChatDto.cs           # ChannelDto, MessageDto
│       │   │   ├── Commands/
│       │   │   │   ├── CreateChannel.cs
│       │   │   │   ├── SendMessage.cs
│       │   │   │   └── MarkMessageRead.cs
│       │   │   └── Queries/
│       │   │       ├── GetChannels.cs
│       │   │       └── GetMessages.cs
│       │   ├── Sprints/
│       │   │   ├── SprintDto.cs         # Includes FromSprint factory with story point computation
│       │   │   ├── Commands/
│       │   │   │   ├── CreateSprint.cs
│       │   │   │   ├── StartSprint.cs
│       │   │   │   ├── CompleteSprint.cs
│       │   │   │   ├── AddIssueToSprint.cs
│       │   │   │   └── RemoveIssueFromSprint.cs
│       │   │   └── Queries/
│       │   │       ├── GetSprints.cs
│       │   │       ├── GetSprintById.cs
│       │   │       └── GetActiveSprint.cs
│       │   ├── Teams/
│       │   │   ├── TeamDto.cs           # TeamDto, TeamMemberDto
│       │   │   ├── Commands/
│       │   │   │   ├── CreateTeam.cs
│       │   │   │   ├── UpdateTeam.cs
│       │   │   │   ├── DeleteTeam.cs
│       │   │   │   ├── AddTeamMember.cs
│       │   │   │   └── RemoveTeamMember.cs
│       │   │   └── Queries/
│       │   │       ├── GetTeams.cs
│       │   │       ├── GetTeamById.cs
│       │   │       └── GetTeamMembers.cs
│       │   ├── Roles/
│       │   │   └── Commands/
│       │   │       ├── CreateRole.cs
│       │   │       └── DeleteRole.cs
│       │   ├── Gantt/
│       │   │   ├── GanttDto.cs          # GanttDataDto, GanttIssueDto, GanttDependencyDto
│       │   │   ├── Commands/
│       │   │   │   ├── CreateDependency.cs
│       │   │   │   └── DeleteDependency.cs
│       │   │   └── Queries/
│       │   │       └── GetGanttData.cs
│       │   ├── Logs/
│       │   │   └── Queries/
│       │   │       └── GetAuditLogs.cs
│       │   ├── Behaviors/
│       │   │   ├── ValidationBehavior.cs  # FluentValidation pipeline
│       │   │   └── LoggingBehavior.cs     # Request/response logging
│       │   └── Common/
│       │       ├── DTOs/
│       │       │   ├── PagedResult.cs
│       │       │   └── AuditLogEntryDto.cs
│       │       ├── Exceptions/
│       │       │   └── NotFoundException.cs
│       │       ├── Interfaces/
│       │       │   ├── ICurrentUserService.cs
│       │       │   └── IJwtService.cs
│       │       └── Services/
│       │           ├── IAuditService.cs
│       │           └── AuditService.cs
│       │
│       ├── Misil.Infrastructure/         # Persistence & external services
│       │   ├── Misil.Infrastructure.csproj
│       │   ├── DependencyInjection.cs   # Registers EF, Identity, JWT, repos, services
│       │   ├── Data/
│       │   │   ├── MisilDbContext.cs     # EF Core DbContext (extends IdentityDbContext)
│       │   │   ├── Configurations/      # 22 Fluent API entity configurations
│       │   │   │   ├── AuditLogConfiguration.cs
│       │   │   │   ├── ChatChannelConfiguration.cs
│       │   │   │   ├── ChatMessageConfiguration.cs
│       │   │   │   ├── ChatMessageReadConfiguration.cs
│       │   │   │   ├── IssueAttachmentConfiguration.cs
│       │   │   │   ├── IssueCommentConfiguration.cs
│       │   │   │   ├── IssueConfiguration.cs
│       │   │   │   ├── IssueDependencyConfiguration.cs
│       │   │   │   ├── IssueRelationConfiguration.cs
│       │   │   │   ├── IssueTagConfiguration.cs
│       │   │   │   ├── IssueTagMappingConfiguration.cs
│       │   │   │   ├── MilestoneConfiguration.cs
│       │   │   │   ├── NotificationConfiguration.cs
│       │   │   │   ├── ProjectConfiguration.cs
│       │   │   │   ├── ProjectInvitationConfiguration.cs
│       │   │   │   ├── ProjectMemberConfiguration.cs
│       │   │   │   ├── RoleConfiguration.cs
│       │   │   │   ├── SprintConfiguration.cs
│       │   │   │   ├── SprintIssueConfiguration.cs
│       │   │   │   ├── TeamConfiguration.cs
│       │   │   │   ├── TeamMemberConfiguration.cs
│       │   │   │   └── UserConfiguration.cs
│       │   │   └── Seed/
│       │   │       └── SeedData.cs      # Seed users + roles
│       │   ├── Repositories/
│       │   │   ├── AuditLogRepository.cs
│       │   │   ├── ChatRepository.cs
│       │   │   ├── GanttRepository.cs
│       │   │   ├── IssueRepository.cs
│       │   │   ├── ProjectRepository.cs
│       │   │   ├── RoleRepository.cs
│       │   │   ├── SprintRepository.cs
│       │   │   ├── TeamRepository.cs
│       │   │   └── UserRepository.cs
│       │   ├── Services/
│       │   │   ├── CurrentUserService.cs  # Reads user from HTTP context
│       │   │   ├── FileStorageService.cs  # Local disk file storage
│       │   │   └── JwtService.cs          # JWT generation
│       │   └── Migrations/               # EF Core migrations (auto-generated)
│       │
│       └── Misil.Api/                   # Host / entry point
│           ├── Misil.Api.csproj
│           ├── Program.cs               # App startup, DI, middleware, DB init
│           ├── Permissions.cs           # AllPermissions static list
│           ├── appsettings.json
│           ├── appsettings.Development.json
│           ├── Properties/
│           │   └── launchSettings.json
│           ├── Controllers/
│           │   ├── AuthController.cs
│           │   ├── ProjectsController.cs
│           │   ├── IssuesController.cs
│           │   ├── SprintsController.cs
│           │   ├── TeamsController.cs
│           │   ├── ChatController.cs
│           │   ├── RolesController.cs
│           │   ├── GanttController.cs
│           │   ├── StatsController.cs
│           │   ├── LogsController.cs
│           │   ├── NotificationsController.cs
│           │   └── InvitationsController.cs
│           ├── Hubs/
│           │   └── ProjectHub.cs         # SignalR hub
│           ├── Middleware/
│           │   └── ExceptionMiddleware.cs # Global exception handler
│           └── Services/
│               ├── IPermissionService.cs  # Permission check interface
│               └── PermissionService.cs   # Permission check implementation
│
└── frontend/
    ├── index.html                         # HTML entry point
    ├── package.json                       # Dependencies & scripts
    ├── vite.config.ts                     # Vite config (dev proxy, aliases)
    ├── tsconfig.json                      # TypeScript config
    ├── tsconfig.app.json                  # App-specific TS config
    ├── tsconfig.node.json                 # Node-specific TS config
    └── src/
        ├── main.tsx                       # React entry point
        ├── App.tsx                        # Router + providers
        ├── index.css                      # Global styles (Tailwind + glassmorphism)
        ├── vite-env.d.ts                  # Vite type declarations
        ├── types/
        │   └── index.ts                   # All TypeScript interfaces & enums
        ├── lib/
        │   ├── api.ts                     # Axios instance with auth interceptor
        │   └── utils.ts                   # cn() helper (clsx + tailwind-merge)
        ├── hooks/
        │   ├── useAuth.tsx                # Auth context (login, register, logout, updateUser)
        │   ├── useProject.tsx             # Project context (select, clear)
        │   ├── useSignalR.ts             # SignalR connection hook
        │   ├── use-toast.ts              # Toast notification system
        │   └── usePermissions.ts         # Permission checking hook
        ├── store/
        │   └── projectStore.ts           # (reserved for future state management)
        ├── components/
        │   ├── layout/
        │   │   ├── AppShell.tsx           # Main layout (sidebar + topbar + content)
        │   │   ├── Sidebar.tsx            # Navigation sidebar
        │   │   ├── TopBar.tsx             # Top bar with user menu + notifications
        │   │   └── StatusBar.tsx          # Connection status bar
        │   ├── ui/
        │   │   ├── button.tsx             # Glass-styled button with variants
        │   │   ├── input.tsx              # Glass-styled input
        │   │   ├── badge.tsx              # Glass-styled badge
        │   │   ├── avatar.tsx             # Glass-styled avatar
        │   │   ├── toast.tsx              # Toast primitives (Radix-based)
        │   │   ├── toaster.tsx            # Toast container
        │   │   ├── scroll-area.tsx        # Scroll area (Radix)
        │   │   └── separator.tsx          # Separator (Radix)
        │   └── shared/
        │       ├── CreateIssueDialog.tsx   # Issue creation modal
        │       └── IssueDetailPanel.tsx    # Issue detail slide-over
        └── pages/
            ├── auth/
            │   ├── LoginPage.tsx           # Login with glass card + ambient orbs
            │   └── RegisterPage.tsx        # Register with glass card + ambient orbs
            ├── projects/
            │   └── ProjectListPage.tsx     # Project cards with invitations
            ├── board/
            │   └── BoardPage.tsx           # Kanban board with DnD + SignalR
            ├── backlog/
            │   └── BacklogPage.tsx         # Backlog list + SignalR
            ├── sprint/
            │   └── SprintDetailPage.tsx    # Sprint detail + progress + SignalR
            ├── chat/
            │   └── ChatPage.tsx            # Channel + message panel
            ├── gantt/
            │   └── GanttPage.tsx           # Custom Gantt chart visualization
            ├── teams/
            │   └── TeamsPage.tsx           # Team management with members
            ├── stats/
            │   └── StatsPage.tsx           # Recharts dashboard
            ├── members/
            │   └── MembersPage.tsx         # Member management + invites
            ├── roles/
            │   └── RolesPage.tsx           # Role management
            ├── logs/
            │   └── LogsPage.tsx            # Activity log viewer
            └── settings/
                └── SettingsPage.tsx        # Profile + theme settings
```

---

## Setup & Running

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- PostgreSQL 16

### Backend Setup

```bash
cd backend/src/Misil.Api

# Update the connection string in appsettings.json:
# "Host=localhost;Port=5432;Database=misil;Username=postgres;Password=your_password"

dotnet restore
dotnet run
```

The API starts on `http://localhost:5000`. OpenAPI docs at `http://localhost:5000/openapi/v1.json`.

### Frontend Setup

```bash
cd frontend
npm install
npm run dev
```

The dev server starts on `http://localhost:5173`, proxying `/api` and `/hubs` to the backend.

### Seed Users

| Email | Password | Role |
|---|---|---|
| `admin@misil.dev` | `Admin123!` | Super Admin (+ Owner of any projects they create) |
| `user@misil.dev` | `User123!` | Demo user (Developer role) |

### Environment Variables

All configuration lives in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=misil;...;Password=683683"
  },
  "Jwt": {
    "Key": "MISIL-SuperSecret-Key-That-Is-At-Least-32-Characters-Long!!",
    "Issuer": "Misil",
    "Audience": "Misil",
    "ExpiresInHours": 24
  },
  "FileStorage": {
    "BasePath": "uploads"
  }
}
```

---

## Key Architectural Patterns

### CQRS (Command Query Responsibility Segregation)
- Every action is either a **Command** (mutates state) or a **Query** (reads state)
- Commands and Queries are separate records with their own handlers
- MediatR routes requests to the correct handler

### Repository Pattern
- Domain defines interfaces (`IProjectRepository`, etc.)
- Infrastructure implements them with EF Core
- Application layer depends only on interfaces (Dependency Inversion)

### Pipeline Behaviors (Decorator Pattern)
- `ValidationBehavior<TRequest, TResponse>` — validates commands before execution
- `LoggingBehavior<TRequest, TResponse>` — logs every request/response
- Both are MediatR pipeline behaviors, applied globally via DI registration

### Audit Logging
- Every create/update/delete/status-change/assign action is logged to `AuditLogs`
- `AuditService` captures: who did it, what entity, what action, old/new values
- Logs are viewable in the Activity Log page with filtering and pagination

### Permission-Based Authorization
- Project-level permissions are checked per-action via `IPermissionService`
- Permissions are stored as strings on the `Role` entity (e.g., `"create_issues,manage_sprints"`)
- The Owner role has `"all"` permission, bypassing individual checks
- Frontend hides UI elements when the user lacks the required permission
