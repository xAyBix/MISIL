import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const badgeVariants = cva(
  "inline-flex items-center rounded-lg px-2.5 py-0.5 text-[11px] font-medium transition-colors duration-150",
  {
    variants: {
      variant: {
        default:
          "bg-misil-500/10 backdrop-blur-sm text-misil-400 border border-misil-500/20",
        secondary:
          "bg-surface-3/50 backdrop-blur-sm text-gray-300 border border-surface-4/50",
        destructive:
          "bg-red-500/10 backdrop-blur-sm text-red-400 border border-red-500/20",
        outline:
          "text-gray-500 border border-surface-4/50",
      },
    },
    defaultVariants: { variant: "default" },
  },
)

export interface BadgeProps extends React.HTMLAttributes<HTMLDivElement>, VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return <div className={cn(badgeVariants({ variant }), className)} {...props} />
}

export { Badge, badgeVariants }
