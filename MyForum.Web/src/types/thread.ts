import type { BoardSummary } from "./boardSummary";
import type { Post } from "./post";

export interface Thread {
  id: number;
  subject: string;
  isPinned: boolean;
  isLocked: boolean;
  createdAt: Date;
  lastBumpAt: Date;
  originalPost: Post;
  postCount: number;
  fileCount: number;
  board: BoardSummary;
  posts: Post[];
}
