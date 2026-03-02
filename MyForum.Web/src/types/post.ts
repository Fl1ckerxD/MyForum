import type { File } from "./file";

export interface Post {
  id: number;
  authorName: string;
  content: string;
  createdAt: Date;
  files: File[];
  replyToPostId?: number;
}
