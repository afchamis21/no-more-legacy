import {api} from '../api'
import type {SupportedFramework} from '../../types/SupportedFramework'

export async function ApiConversion(framework: SupportedFramework, file: File) {
    const formData = new FormData();

    formData.append('file', file)

    return api.post("api/Conversor", formData, {
        params: {
            framework,
            responseType: 'blob'
        }
    })
}