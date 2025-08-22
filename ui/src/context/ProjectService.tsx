import React, { useContext } from "react"
import type { SupportedFramework } from "../types/SupportedFramework";
import {ApiConversion} from '../api/project/ApiConversion'
interface Props {
    children: React.ReactNode;
}

interface IProjectProvider {
    upload(project: File, framework: SupportedFramework): Promise<void>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useProjectService = (): IProjectProvider => {
    const context = useContext(ProjectContextObject);

    if (!context) {
        throw new Error("Cannot use ProjectService without ProjectServiceProvider!")
    }

    return context;
}

function ProjectServiceProvider(props: Props) {

    async function upload(project: File, framework: SupportedFramework) {
        try {
            const response = await ApiConversion(framework, project)
            console.log("Vaaaaaamo", response)
        } catch {
            console.error("Deu merda")
        }
    }

    return (
        <ProjectContextObject.Provider
            value={{upload}}
        >
            {props.children}
        </ProjectContextObject.Provider>
    )
}

const ProjectContextObject = React.createContext<IProjectProvider | undefined>(undefined);
export default ProjectServiceProvider;
