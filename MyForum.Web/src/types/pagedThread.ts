import type { Thread } from "./thread";

export interface PagedThread {
  threads: Thread[];
  nextCursor: string | null;
}
