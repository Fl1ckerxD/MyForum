import type { Thread } from "../thread";

export interface GetThreadsResponse {
  threads: Thread[];
  nextCursor: string | null;
}
