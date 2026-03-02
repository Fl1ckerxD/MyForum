import type { Thread } from "../thread";

export interface GetThreadResponse {
  thread: Thread;
  nextCursor: number | null;
}
