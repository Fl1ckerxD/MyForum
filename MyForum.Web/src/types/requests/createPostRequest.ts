export interface CreatePostRequest {
  threadId: number;
  content: string;
  authorName?: string;
  files?: File[];
}