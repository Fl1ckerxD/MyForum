import { api, extractErrorMessage } from "./http";
import type { Post } from "../types/post";
import type { CreatePostRequest } from "../types/requests/createPostRequest";

export function getPosts() {
  return api<Post[]>("/posts");
}

export async function createPost(request: CreatePostRequest) {
  const formData = new FormData();

  formData.append("ThreadId", request.threadId.toString());
  formData.append("Content", request.content);
  formData.append("AuthorName", request.authorName || "Аноним");
  formData.append("ReplyToPostId", request.replyToPostId?.toString() || "");

  request.files?.forEach((file) => {
    formData.append("Files", file);
  });

  const response = await fetch("/api/posts", {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    const error = await extractErrorMessage(response);
    throw new Error(error);
  }

  return response.json();
}

