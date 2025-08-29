import { useState } from "react";
import type { TreeNode } from "../../hooks/UseFileTree";
import { ChevronDown, ChevronRight, File } from "lucide-react";

export function TreeNodeView({
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