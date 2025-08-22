import { Header } from "../components/Header";
import { ProjectForm } from "../components/ProjectForm";

export function UploadView() {    
    return (
    <div className="relative w-screen h-screen flex flex-col bg-gradient-to-br from-stone-950 via-stone-900 to-stone-950 overflow-hidden">
      <div className="absolute inset-0 bg-[linear-gradient(to_right,rgba(255,255,255,0.04)_1px,transparent_1px),linear-gradient(to_bottom,rgba(255,255,255,0.04)_1px,transparent_1px)] bg-[size:40px_40px] pointer-events-none"></div>

      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[500px] h-[500px] rounded-full bg-green-500/10 blur-3xl pointer-events-none"></div>

      <div className="relative flex flex-col h-full">
        <Header />
        <main className="flex-1 flex items-center justify-center">
          <ProjectForm />
        </main>
      </div>
    </div>
    )
}