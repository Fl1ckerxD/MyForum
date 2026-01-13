interface CreateThreadRequest {
  boardId: number;
  boardShortName: string;
  subject: string;
  content: string;
  authorName?: string;
  files?: File[];
}

export async function createThread(request: CreateThreadRequest) {
  const formData = new FormData();

  formData.append("BoardId", request.boardId.toString());
  formData.append("BoardShortName", request.boardShortName);
  formData.append("Subject", request.subject);

  formData.append("OriginalPost.Content", request.content);
  formData.append("OriginalPost.AuthorName", request.authorName || "Аноним");

  request.files?.forEach((file) => {
    formData.append("OriginalPost.Files", file);
  });

  const response = await fetch("/api/threads", {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    const error = await response.text();
    throw new Error(error);
  }

  return response.json();
}
