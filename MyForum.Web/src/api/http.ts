const API_BASE_URL =
  import.meta.env.MODE === "development"
    ? "https://localhost:8080/api"
    : "/api";

export async function api<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
    ...options,
  });

  if (!response.ok) {
    const error = await response.text();
    throw new Error(error);
  }

  return response.json() as Promise<T>;
}
