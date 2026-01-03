import { api } from "./http";
import type { Thread } from "../types/thread";

export async function getThreadsByBoardId(boardId: number) {
  return api<Thread[]>(`/boards/${boardId}/threads`);
}
