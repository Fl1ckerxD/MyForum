import type { Thread } from "../thread";

export interface GetThreadResponse {
  threads: Thread[];
  nextCursor: string | null;
}
