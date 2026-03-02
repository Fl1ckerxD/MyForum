export interface CreateThreadRequest {
  boardId: number;
  boardShortName: string;
  subject: string;
  content: string;
  authorName?: string;
  files?: File[];
}