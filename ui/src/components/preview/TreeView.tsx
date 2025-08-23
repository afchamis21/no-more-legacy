import { useEffect, useState } from "react";
import { ChevronRight, ChevronDown, File } from "lucide-react";
import { type TreeNode, useFileTree } from "../../hooks/UseFileTree";
import type { FileContent } from "../../types/FileContent";

function TreeNodeView({
  node,
  onFileClick,
}: {
  node: TreeNode;
  onFileClick: (file: TreeNode) => void;
}) {
  const [open, setOpen] = useState(true);

  if (node.type === "folder") {
    return (
      <div className="ml-2">
        <div
          onClick={() => setOpen(!open)}
          className="flex items-center gap-1 cursor-pointer hover:text-green-400 transition-colors"
        >
          {open ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
          <span className="font-medium">{node.name}</span>
        </div>
        {open && (
          <div className="ml-4 border-l border-stone-700 pl-2">
            {node.children?.map((child, i) => (
              <TreeNodeView key={i} node={child} onFileClick={onFileClick} />
            ))}
          </div>
        )}
      </div>
    );
  }

  return (
    <div
      className="ml-6 flex items-center gap-1 hover:text-green-400 cursor-pointer transition-colors"
      onClick={() => onFileClick(node)}
    >
      <File size={14} />
      <span>{node.name}</span>
    </div>
  );
}

export function TreeView({ files }: { files: FileContent<string>[] }) {
  const tree = useFileTree(files);
  const [selectedFile, setSelectedFile] = useState<TreeNode | null>(null);

  useEffect(() => {
    const escHandler = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        setSelectedFile(null)
      }
    };

    window.addEventListener("keydown", escHandler);

    return () => {
      window.removeEventListener("keydown", escHandler);
    };
  }, []);

  return (
    <div className="flex flex-wrap gap-6 max-w-6xl w-full mx-auto">
      <div className="bg-stone-900/50 backdrop-blur-sm rounded-lg p-4 border border-stone-700 
                      flex-shrink-0 w-full sm:w-1/3 min-w-[250px] 
                      max-h-[70vh] overflow-auto">
        {tree.map((node, i) => (
          <TreeNodeView key={i} node={node} onFileClick={setSelectedFile} />
        ))}
      </div>

      <div className="flex-1 bg-stone-900/50 backdrop-blur-sm rounded-lg p-4 border border-stone-700 
                      w-full sm:w-2/3 min-w-[300px] 
                      max-h-[70vh] overflow-auto flex flex-col justify-between gap-6">
        {selectedFile ? (
          <>
            <h2 className="text-green-400 font-mono font-bold mb-2">
              {selectedFile.name}
            </h2>
            <pre className="text-gray-200 text-sm bg-stone-950 p-4 rounded-lg 
                            overflow-auto overflow-x-auto h-full">
              {selectedFile.content}
            </pre>
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
