import { useEffect, useState } from "react";
import { type TreeNode, useFileTree } from "../../hooks/UseFileTree";
import type { FileContent } from "../../types/FileContent";
import { TreeNodeView } from "./TreeNodeView";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { atomDark } from "react-syntax-highlighter/dist/esm/styles/prism";

function detectLanguage(fileName: string) {
  if (fileName.endsWith(".ts") || fileName.endsWith(".tsx"))
    return "typescript";
  if (fileName.endsWith(".js") || fileName.endsWith(".jsx"))
    return "javascript";
  if (fileName.endsWith(".java")) return "java";
  if (fileName.endsWith(".css")) return "css";
  if (fileName.endsWith(".html")) return "html";
  if (fileName.endsWith(".json")) return "json";
  if (fileName.endsWith(".properties")) return "properties";
  if (fileName.endsWith(".xml")) return "xml";
  return "text";
}

export function TreeView({ files }: { files: FileContent<string>[] }) {
  const tree = useFileTree(files);
  const [selectedFile, setSelectedFile] = useState<TreeNode | null>(null);

  useEffect(() => {
    const escHandler = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        setSelectedFile(null);
      }
    };

    window.addEventListener("keydown", escHandler);
    return () => window.removeEventListener("keydown", escHandler);
  }, []);

  return (
    <div className="flex flex-wrap gap-6 w-full mx-auto">
      <div
        className="bg-stone-900/50 backdrop-blur-sm rounded-lg p-4 border border-stone-700 
                   flex-shrink-0 w-full sm:w-1/3 min-w-[250px] 
                   max-h-[70vh] overflow-auto"
      >
        {tree.map((node, i) => (
          <TreeNodeView key={i} node={node} onFileClick={setSelectedFile} />
        ))}
      </div>

      <div
        className="flex-1 bg-stone-900/50 backdrop-blur-sm rounded-lg p-4 border border-stone-700 
                   w-full sm:w-2/3 min-w-[300px] 
                   max-h-[70vh] overflow-auto flex flex-col gap-6"
      >
        {selectedFile ? (
          <>
            <h2 className="text-green-400 font-mono font-bold mb-2">
              {selectedFile.name}
            </h2>
            <SyntaxHighlighter
              language={detectLanguage(selectedFile.name)}
              style={atomDark}
              customStyle={{
                // background: "transparent",
                padding: "1rem",
                borderRadius: "0.5rem",
                maxHeight: "70vh",
                overflow: "auto",
                fontSize: "0.875rem",
                border: "1px solid #44403b"
              }}
            >
              {selectedFile.content || ""}
            </SyntaxHighlighter>
          </>
        ) : (
          <p className="text-gray-500 italic">
            Clique em um arquivo para ver o conteúdo
          </p>
        )}
      </div>
    </div>
  );
}
