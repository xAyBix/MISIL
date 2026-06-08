import { Wifi } from 'lucide-react'

export function StatusBar() {
  return (
    <footer className="h-6 bg-surface-1/60 backdrop-blur-md border-t border-surface-3/40 flex items-center px-3 text-[10px] text-gray-600 shrink-0">
      <div className="flex items-center gap-1.5">
        <span className="relative flex h-1.5 w-1.5">
          <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-misil-500 opacity-40" />
          <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-misil-500" />
        </span>
        <span className="text-gray-500">Connected</span>
      </div>
      <span className="ml-auto text-gray-600 font-medium">MISIL v1.0</span>
    </footer>
  )
}
