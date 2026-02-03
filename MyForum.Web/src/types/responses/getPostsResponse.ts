import type { Post } from "../post";

export interface GetPostsResponse {
  posts: Post[];
  nextCursor: number | null;
}
