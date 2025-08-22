import { useEffect, useState } from "react";
import { useProjectService } from "../context/ProjectService";
import type { SupportedFramework } from "../types/SupportedFramework";

export function ProjectForm() {
  const [projectType, setProjectType] = useState<SupportedFramework | "">("");
  const [file, setFile] = useState<File | null>(null);
  const [errors, setErrors] = useState<{ projectType?: string; file?: string }>({});

  const projectService = useProjectService();

  const SupportedFramework: { value: SupportedFramework | ""; label: string }[] = [
    { value: "", label: "Selecione uma opção" },
    { value: "Struts", label: "Struts" },
    { value: "Jsf", label: "Jsf" },
    { value: "JaxRs", label: "JaxRs" },
    { value: "AngularJs", label: "AngularJs" },
  ];

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const newErrors: { projectType?: string; file?: string } = {};

    if (!projectType) {
      newErrors.projectType = "Este campo é obrigatório.";
    }
    if (!file) {
      newErrors.file = "Por favor, selecione um arquivo .zip.";
    }

    setErrors(newErrors);

    if (file && projectType) {
      projectService.upload(file, projectType);
      console.log("Form submitted:", { projectType, file });
    }
  };

  useEffect(() => {
    setErrors((prev) => {
      const updated = { ...prev };
      if (projectType && updated.projectType) delete updated.projectType;
      if (file && updated.file) delete updated.file;
      return updated;
    });
  }, [projectType, file]);

  return (
    <form
      onSubmit={handleSubmit}
      className="flex flex-col gap-6 items-center text-gray-200 max-w-lg w-full p-6 bg-stone-900 rounded-2xl shadow-md shadow-stone-950"
    >
      <p className="text-lg font-semibold text-center">
        🚀 Faça o upload de um projeto para fazer a migração!
      </p>

      <div className="w-full group">
        <label
          htmlFor="projectType"
          className="block mb-2 text-sm font-medium text-gray-400 group-hover:text-green-500 transition-colors cursor-pointer"
        >
          Tipo de projeto
        </label>
        <select
          id="projectType"
          name="projectType"
          value={projectType}
          onChange={(e) => setProjectType(e.target.value as SupportedFramework)}
          className={`w-full px-3 py-2 rounded-lg bg-stone-800 border text-gray-200 transition-colors cursor-pointer
            focus:outline-none focus:border-green-500 focus:ring-1 focus:ring-green-500 group-hover:border-green-500
            ${errors.projectType ? "border-red-500" : "border-stone-700"}`}
        >
          {SupportedFramework.map((framework, i) => (
            <option value={framework.value} key={i}>
              {framework.label}
            </option>
          ))}
        </select>
        {errors.projectType && (
          <p className="text-red-500 text-sm mt-1">{errors.projectType}</p>
        )}
      </div>

      <div className="w-full">
        <label
          htmlFor="projectFile"
          className={`w-full flex flex-col items-center justify-center border-2 border-dashed rounded-xl cursor-pointer px-6 py-10 bg-stone-800 transition-colors
            hover:border-green-500 hover:text-green-500
            ${errors.file ? "border-red-500 text-red-500" : "border-stone-600"}
            ${file && !errors.file ? "border-green-500 text-green-500" : ""}`}
        >
          {file ? (
            <div className="text-center">
              <span className="text-sm font-semibold mb-2">Arquivo selecionado! ✅</span>
              <span className="text-gray-400 text-xs break-all">{file.name}</span>
            </div>
          ) : (
            <>
              <span className="text-sm mb-2">Arraste e solte seu arquivo aqui</span>
              <span className="text-gray-400 text-xs">ou clique para selecionar</span>
            </>
          )}

          <input
            type="file"
            id="projectFile"
            name="projectFile"
            accept=".zip"
            className="hidden"
            onChange={(e) => setFile(e.target.files ? e.target.files[0] : null)}
          />
        </label>
        {errors.file && <p className="text-red-500 text-sm mt-1">{errors.file}</p>}
      </div>

      <button
        type="submit"
        className="cursor-pointer w-full py-3 bg-green-600 hover:bg-green-500 text-white font-semibold rounded-xl shadow-md transition-colors"
      >
        Enviar Projeto
      </button>
    </form>
  );
}