export interface AdminPostDto {
  id: number;
  threadId: number;
  boardId: number;
  isOriginal: boolean;
  author: string;
  content: string;
  isDeleted: boolean;
  deletedAt?: string;
  createdAt: string;
}
