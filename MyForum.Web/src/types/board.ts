import type { Thread } from "./thread";

export interface Board {
  id: number;
  name: string;
  shortName: string;
  description: string;
  threads: Thread[];
}
