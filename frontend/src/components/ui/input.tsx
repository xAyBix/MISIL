import * as React from "react"
import { cn } from "@/lib/utils"

const Input = React.forwardRef<HTMLInputElement, React.InputHTMLAttributes<HTMLInputElement>>(
  ({ className, type, ...props }, ref) => {
    return (
      <input
        type={type}
        className={cn(
          "flex h-9 w-full rounded-lg border border-surface-4/70 bg-surface-2/50 backdrop-blur-sm px-3 py-1 text-sm text-white shadow-sm transition-all duration-150",
          "placeholder:text-gray-600",
          "hover:border-surface-4",
          "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-misil-500/30 focus-visible:border-misil-500/50",
          "disabled:cursor-not-allowed disabled:opacity-50",
          className,
        )}
        ref={ref}
        {...props}
      />
    )
  },
)
Input.displayName = "Input"
export { Input }
