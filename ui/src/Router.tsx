import { Route, Routes } from "react-router-dom";
import { UploadView } from "./pages/Upload";
import { Layout } from "./Layout";
import { PreviewView } from "./pages/Preview";

export function Router() {
    return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<UploadView />} />
        <Route path="preview" element={<PreviewView />} />
      </Route>
    </Routes>
    )
}