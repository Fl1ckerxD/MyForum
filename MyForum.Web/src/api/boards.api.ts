import { api } from "./http";
import type { BoardName } from "../types/boardName";
import type { Board } from "../types/board";
import type { PagedThread } from "../types/pagedThread";

export async function getBoardNames() {
  return await api<BoardName[]>("/boards");
}

export async function getBoard(boardShortName: string) {
  const data = await api<Board>(`/boards/${boardShortName}`);
  return {
    ...data,
    threads: data.threads.map((thread) => ({
      ...thread,
      createdAt: new Date(thread.createdAt),
    })),
  };
}

export async function getBoardThreads(boardShortName: string, cursor?: string) {
  const params = new URLSearchParams();

  if (cursor) {
    params.append("cursor", cursor);
  }

  const data = await api<PagedThread>(
    `/boards/${boardShortName}/threads?${params}`,
  );

  return {
    ...data,
    threads: data.threads.map((thread) => ({
      ...thread,
      createdAt: new Date(thread.createdAt),
    })),
  };
}
