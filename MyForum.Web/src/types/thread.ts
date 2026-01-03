import type { Post } from "./post";

export interface Thread {
  id: number;
  subject: string;
  createdAt: Date;
  originalPost: Post;
  postCount: number;
  fileCount: number;
  posts: Post[];
}
