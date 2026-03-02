const API_BASE_URL =
  import.meta.env.MODE === "development"
    ? "http://localhost:80/api"
    : "/api";

export async function extractErrorMessage(response: Response): Promise<string> {
  const rawError = await response.text();

  if (!rawError) {
    return `HTTP ${response.status}`;
  }

  try {
    const parsed = JSON.parse(rawError);

    if (Array.isArray(parsed)) {
      const messages = parsed
        .map((item) =>
          typeof item?.errorMessage === "string" ? item.errorMessage : null,
        )
        .filter((message): message is string => Boolean(message?.trim()));

      if (messages.length > 0) {
        return messages.join("\n");
      }
    }

    if (typeof parsed?.message === "string" && parsed.message.trim()) {
      return parsed.message;
    }

    if (typeof parsed?.error === "string" && parsed.error.trim()) {
      return parsed.error;
    }

    if (typeof parsed?.title === "string" && parsed.title.trim()) {
      return parsed.title;
    }
  } catch {
    // Keep raw text when response is not JSON.
  }

  return rawError;
}

export async function api<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
    ...options,
  });

  if (!response.ok) {
    const error = await extractErrorMessage(response);
    throw new Error(error);
  }

  return response.json() as Promise<T>;
}

