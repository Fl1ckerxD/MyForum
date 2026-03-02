import type { Post } from "../types/post";
import type { CreateThreadRequest } from "../types/requests/createThreadRequest";
import type { GetPostsResponse } from "../types/responses/getPostsResponse";
import type { GetThreadResponse } from "../types/responses/getThreadResponse";
import { api, extractErrorMessage } from "./http";

export async function getThread(
  boardShortName: string,
  threadId: number,
): Promise<GetThreadResponse> {
  const data = await api<GetThreadResponse>(
    `/threads/${boardShortName}/${threadId}`,
  );

  return {
    thread: {
      ...data.thread,
      createdAt: new Date(data.thread.createdAt),
      lastBumpAt: new Date(data.thread.lastBumpAt),
    },

    nextCursor: data.nextCursor,
  };
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
    const error = await extractErrorMessage(response);
    throw new Error(error);
  }

  return response.json();
}

export async function getThreadPosts(
  threadId: number,
  afterId: string | null,
  limit: number,
): Promise<{ items: Post[]; nextCursor: string | null }> {
  const params = new URLSearchParams();

  if (afterId) {
    params.append("afterId", afterId.toString());
  }

  params.append("limit", limit.toString());

  const data = await api<GetPostsResponse>(
    `/threads/${threadId}/posts?${params}`,
  );

  return {
    items: data.posts,
    nextCursor: data.nextCursor?.toString() || null,
  };
}

