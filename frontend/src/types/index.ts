export enum IssueType { Epic = "Epic", Story = "Story", Task = "Task", Bug = "Bug", Subtask = "Subtask" }
export enum IssueStatus { ToDo = "ToDo", InProgress = "InProgress", InReview = "InReview", Done = "Done", Cancelled = "Cancelled" }
export enum IssuePriority { Highest = "Highest", High = "High", Medium = "Medium", Low = "Low", Lowest = "Lowest" }
export enum RelationType { RelatesTo = "RelatesTo", Blocks = "Blocks", IsBlockedBy = "IsBlockedBy", Duplicates = "Duplicates" }
export enum DependencyType { FinishToStart = "FinishToStart", StartToStart = "StartToStart", FinishToFinish = "FinishToFinish", StartToFinish = "StartToFinish" }
export enum EntityType { Project = "Project", Issue = "Issue", Sprint = "Sprint", Team = "Team", ChatChannel = "ChatChannel", Member = "Member", Role = "Role", Invitation = "Invitation" }

export interface UserDto { id: string; username: string; email: string; displayName?: string; avatarUrl?: string; }
export interface AuthResponse { token: string; refreshToken: string; user: UserDto; }
export interface RegisterRequest { email: string; password: string; username: string; displayName: string; }
export interface LoginRequest { email: string; password: string; }

export interface ProjectDto { id: string; name: string; description?: string; key: string; createdAt: string; memberCount: number; }
export interface ProjectDetailDto { id: string; name: string; description?: string; key: string; leadUserId: string; createdAt: string; members: ProjectMemberDto[]; }
export interface ProjectMemberDto { userId: string; userName: string; email: string; roleId: string; displayName?: string; avatarUrl?: string; joinedAt: string; }

export interface IssueDto {
  id: string;
  title: string;
  issueType: IssueType;
  status: IssueStatus;
  priority: IssuePriority;
  projectId: string;
  assigneeId?: string;
  assigneeTeamId?: string;
  reporterId: string;
  parentIssueId?: string;
  order: number;
  storyPoints?: number;
  startDate?: string;
  dueDate?: string;
  createdAt: string;
  updatedAt: string;
}
export interface IssueDetailDto extends IssueDto { description?: string; comments: CommentDto[]; attachments: AttachmentDto[]; }
export interface CommentDto { id: string; issueId: string; userId: string; content: string; createdAt: string; }
export interface AttachmentDto { id: string; issueId: string; fileName: string; fileUrl: string; uploadedBy: string; uploadedAt: string; }

export interface SprintDto { id: string; name: string; goal?: string; projectId: string; isActive: boolean; isCompleted: boolean; startDate?: string; endDate?: string; issueCount: number; totalStoryPoints: number; completedStoryPoints: number; createdAt: string; }

export interface ChannelDto { id: string; projectId: string; name: string; teamId?: string; isPrivate: boolean; createdAt: string; }
export interface MessageDto { id: string; channelId: string; userId: string; content: string; createdAt: string; isRead: boolean; }

export interface TeamDto { id: string; name: string; description?: string; projectId: string; memberCount: number; createdAt: string; }
export interface TeamMemberDto { teamId: string; userId: string; }

export interface GanttDataDto { issues: GanttIssueDto[]; dependencies: GanttDependencyDto[]; }
export interface GanttIssueDto { id: string; title: string; issueType: IssueType; status: IssueStatus; order: number; startDate?: string; dueDate?: string; parentIssueId?: string; }
export interface GanttDependencyDto { id: string; issueId: string; dependsOnIssueId: string; dependencyType: DependencyType; }

export interface AuditLogEntryDto { id: string; entityType: EntityType; entityId: string; userId: string; userName: string; action: string; oldValue?: string; newValue?: string; createdAt: string; }

export interface PagedResult<T> { items: T[]; total: number; page: number; pageSize: number; }

export interface Role { id: string; name: string; description?: string; permissions: string; }
export interface ProjectInvitation {
  id: string
  projectId: string
  invitedUserId: string
  invitedByUserId: string
  roleId: string
  status: string
  createdAt: string
  respondedAt: string | null
  project: ProjectDto
  invitedByUser: { userName: string; email: string }
}
