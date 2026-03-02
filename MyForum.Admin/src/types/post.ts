import type { File } from "./file";

export interface AdminPostDto {
  id: number;
  threadId: number;
  boardId: number;
  isOriginal: boolean;
  author: string;
  content: string;
  files: File[];
  isDeleted: boolean;
  deletedAt?: string;
  createdAt: string;
}
