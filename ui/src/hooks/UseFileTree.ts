import { useMemo } from "react";
import type { FileContent } from "../types/FileContent";

export type TreeNode = {
  name: string;
  type: "folder" | "file";
  children?: TreeNode[];
  content?: string;
};

type InternalNode = {
  name: string;
  type: "folder" | "file";
  children?: Record<string, InternalNode>;
  content?: string;
};

function buildTree(files: FileContent<string>[]): TreeNode[] {
  const root: Record<string, InternalNode> = {};

  for (const file of files) {
    const parts = file.name.split("/").filter(Boolean);
    let current = root;

    parts.forEach((part, idx) => {
      const isFile = idx === parts.length - 1;

      if (!current[part]) {
        current[part] = isFile
          ? { name: part, type: "file", content: file.content }
          : { name: part, type: "folder", children: {} };
      }

      if (!isFile) {
        current = current[part].children!;
      }
    });
  }

  const toArray = (obj: Record<string, InternalNode>): TreeNode[] =>
    Object.values(obj).map((n) =>
      n.type === "folder"
        ? { name: n.name, type: "folder", children: toArray(n.children!) }
        : { name: n.name, type: "file", content: n.content }
    );

  return toArray(root);
}

export function useFileTree(files: FileContent<string>[]) {
  return useMemo(() => buildTree(files), [files]);
}