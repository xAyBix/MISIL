import * as React from "react"
import { Slot } from "@radix-ui/react-slot"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const buttonVariants = cva(
  "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-lg text-sm font-medium transition-all duration-150 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-misil-500 disabled:pointer-events-none disabled:opacity-50 [&_svg]:pointer-events-none [&_svg]:size-4 [&_svg]:shrink-0 cursor-pointer",
  {
    variants: {
      variant: {
        default:
          "bg-gradient-to-b from-misil-600 to-misil-700 text-white shadow-md shadow-misil-600/20 hover:from-misil-500 hover:to-misil-600 hover:shadow-lg hover:shadow-misil-500/30 active:from-misil-700 active:to-misil-800",
        destructive:
          "bg-gradient-to-b from-red-600 to-red-700 text-white shadow-md shadow-red-600/20 hover:from-red-500 hover:to-red-600",
        outline:
          "border border-surface-4/80 bg-surface-1/50 backdrop-blur-sm text-gray-300 hover:bg-surface-3/80 hover:text-white hover:border-surface-4",
        secondary:
          "bg-surface-3/80 backdrop-blur-sm text-gray-300 hover:bg-surface-4/80 hover:text-white",
        ghost:
          "text-gray-400 hover:text-white hover:bg-surface-3/50",
        link:
          "text-misil-500 underline-offset-4 hover:underline hover:text-misil-400",
      },
      size: {
        default: "h-9 px-4 py-2",
        sm: "h-8 rounded-lg px-3 text-xs",
        lg: "h-10 rounded-lg px-8",
        icon: "h-9 w-9",
      },
    },
    defaultVariants: { variant: "default", size: "default" },
  },
)

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, ...props }, ref) => {
    const Comp = asChild ? Slot : "button"
    return (
      <Comp className={cn(buttonVariants({ variant, size, className }))} ref={ref} {...props} />
    )
  },
)
Button.displayName = "Button"
export { Button, buttonVariants }
