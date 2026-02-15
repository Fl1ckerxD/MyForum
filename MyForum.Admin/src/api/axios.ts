import axios from "axios";

export const api = axios.create({
  baseURL:
    import.meta.env.MODE === "development"
      ? "http://localhost:80/api"
      : "http://localhost/api",
  withCredentials: true,
});
