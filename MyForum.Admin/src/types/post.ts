export interface AdminPostDto {
  id: number;
  threadId: number;
  isOriginal: boolean;
  author: string;
  content: string;
  isDeleted: boolean;
  deletedAt?: string;
  createdAt: string;
}
