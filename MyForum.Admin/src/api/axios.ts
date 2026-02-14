import axios from "axios";

export const api = axios.create({
  baseURL:
    import.meta.env.MODE === "development" ? "http://localhost:80/api" : "/api",
  withCredentials: true, // если cookie auth
});
