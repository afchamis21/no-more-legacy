import { Axios } from "axios";

const API_URL = import.meta.env.VITE_API_BASE_URL

export const api = new Axios({
    baseURL: API_URL
})