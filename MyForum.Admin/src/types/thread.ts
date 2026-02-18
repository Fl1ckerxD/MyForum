export interface AdminThreadDto {
  id: number;
  boardShortName: string;
  title: string;
  postsCount: number;

  isLocked: boolean;
  isDeleted: boolean;
  deletedAt?: string;

  createdAt: string;
  lastBumpAt: string;
}
