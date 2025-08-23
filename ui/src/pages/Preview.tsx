import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useProjectService } from "../context/ProjectService";
import { TreeView } from "../components/preview/TreeView";

export function PreviewView() {
  const { conversionResult, previewFiles } = useProjectService();
  const [showPreview, setShowPreview] = useState(true);
  const navigate = useNavigate();

  // Redirect if no conversion result
  useEffect(() => {
    if (!conversionResult) {
      navigate("/");
    }
  }, [conversionResult, navigate]);

  if (!conversionResult) return null; // safeguard while redirecting

  // Download handler
  const handleDownload = () => {
    const url = URL.createObjectURL(conversionResult.content);
    const a = document.createElement("a");
    a.href = url;
    a.download = conversionResult.name;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="flex flex-col items-center justify-center w-full h-full text-gray-200 p-6">
      <h1 className="text-2xl font-bold text-green-500 mb-6">
        ✅ Seu projeto está pronto
      </h1>

      <div className="flex gap-3 mb-6">
        <button
          onClick={handleDownload}
          className="px-5 py-2 bg-stone-800 hover:bg-stone-700 rounded-lg shadow-sm font-medium text-sm transition-colors border border-stone-700"
        >
          ⬇️ Baixar ZIP
        </button>

        <button
          onClick={() => setShowPreview(!showPreview)}
          className="px-5 py-2 bg-stone-800 hover:bg-stone-700 rounded-lg shadow-sm font-medium text-sm transition-colors border border-stone-700"
        >
          {showPreview ? "👁️ Ocultar Arquivos" : "👁️ Ver Arquivos"}
        </button>

        <button
          onClick={() => navigate("/")}
          className="px-5 py-2 bg-stone-800 hover:bg-stone-700 rounded-lg shadow-sm font-medium text-sm transition-colors border border-stone-700"
        >
          ⬅️ Voltar
        </button>
      </div>

      {showPreview && <TreeView files={previewFiles} />}
    </div>
  );
}
