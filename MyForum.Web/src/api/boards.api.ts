import { api } from "./http";
import type { BoardName } from "../types/boardName";
import type { Board } from "../types/board";

export async function getBoardNames() {
  return await api<BoardName[]>("/boards");
}

export async function getBoard(boardShortName: string) {
  const data = await api<Board>(`/boards/${boardShortName}`);
  return {
    ...data,
    threads: data.threads.map(thread => ({
      ...thread,
      createdAt: new Date(thread.createdAt),
    }))
  };
}