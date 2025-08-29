import React, { useContext, useState } from "react";
import type { SupportedFramework } from "../types/SupportedFramework";
import { ApiConversion } from "../api/project/ApiConversion";
import { FullScreenLoader } from "../components/shared/FullScreenLoader";
import toast from "react-hot-toast";
import JSZip from "jszip";
import { useNavigate } from "react-router-dom";
import type { FileContent } from "../types/FileContent";
interface Props {
  children: React.ReactNode;
}

interface IProjectProvider {
  convert(project: File, framework: SupportedFramework): Promise<void>;
  conversionResult: FileContent<Blob> | null;
  previewFiles: FileContent<string>[];
}

// eslint-disable-next-line react-refresh/only-export-components
export const useProjectService = (): IProjectProvider => {
  const context = useContext(ProjectContextObject);

  if (!context) {
    throw new Error(
      "Cannot use ProjectService without ProjectServiceProvider!"
    );
  }

  return context;
};

function ProjectServiceProvider(props: Props) {
  const [isLoading, setIsLoading] = useState(false);
  const [conversionResult, setConversionResult] =
    useState<FileContent<Blob> | null>(null);
  const [previewFiles, setPreviewFiles] = useState<FileContent<string>[]>([]);

  const navigate = useNavigate();

  const loadingMessages = [
    "Preparando o café dos agentes...",
    "Alinhando os modelos de linguagem...",
    "Ensinando novos truques à inteligência artificial...",
    "Descompactando algoritmos complexos...",
    "Isso pode demorar um pouco, os agentes estão trabalhando duro!",
    "Compilando criatividade...",
    "Quase lá, os neurônios artificiais estão a todo vapor!",
  ];

  async function convert(project: File, framework: SupportedFramework) {
    setIsLoading(true);
    try {
      const response = await ApiConversion(framework, project);
      const header = response.headers["content-disposition"];
      const fileName =
        header?.split("filename=")[1]?.replaceAll('"', "") || "download.zip";

      const res: FileContent<Blob> = { content: response.data, name: fileName };

      console.log("Parseando ZIP...");
      const zip = await JSZip.loadAsync(res.content);
      const files: FileContent<string>[] = [];

      await Promise.all(
        Object.keys(zip.files).map(async (relativePath) => {
          const file = zip.files[relativePath];
          if (!file.dir) {
            const content = await file.async("string");
            files.push({ name: relativePath, content });
            console.log("Nome: ", relativePath)
          }
        })
      );
      setConversionResult(res);
      setPreviewFiles(files);
      toast.success("Projeto convertido com sucesso!");
      navigate("/preview");
    } catch (error) {
      console.error("Erro na conversão:", error);
      toast.error("Ocorreu um erro ao converter o projeto...");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <>
      <ProjectContextObject.Provider
        value={{ convert, conversionResult, previewFiles }}
      >
        {props.children}
      </ProjectContextObject.Provider>
      {isLoading && (
        <FullScreenLoader
          interval={5_000}
          messages={loadingMessages}
          random={true}
        ></FullScreenLoader>
      )}
    </>
  );
}

const ProjectContextObject = React.createContext<IProjectProvider | undefined>(
  undefined
);
export default ProjectServiceProvider;
