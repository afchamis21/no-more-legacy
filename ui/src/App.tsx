import ProjectServiceProvider from "./context/ProjectService";
import { UploadView } from "./pages/Upload";

function App() {
  return (
    <ProjectServiceProvider>
      <UploadView />
    </ProjectServiceProvider>
  );
}

export default App;
