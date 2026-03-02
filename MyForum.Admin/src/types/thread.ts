export interface AdminThreadDto {
  id: number;
  boardShortName: string;
  title: string;
  postsCount: number;

  isLocked: boolean;
  isPinned: boolean;
  isDeleted: boolean;
  deletedAt?: string;

  createdAt: string;
  lastBumpAt: string;
}
