import { Toaster } from "react-hot-toast";
import ProjectServiceProvider from "./context/ProjectService";
import { Router } from "./Router";

function App() {
  return (
    <>
      <Toaster
        position="top-right"
        reverseOrder={false}
        toastOptions={{
          duration: 5000,
        }}
      />
      <ProjectServiceProvider>
        <Router />
      </ProjectServiceProvider>
    </>
  );
}

export default App;