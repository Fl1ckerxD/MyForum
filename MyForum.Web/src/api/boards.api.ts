import { api } from "./http";
import type { BoardName } from "../types/boardName";
import type { GetThreadResponse } from "../types/responses/getThreadResponse";
import type { GetBoardResponse } from "../types/responses/getBoardResponse";
import type { Thread } from "../types/thread";

export async function getBoardNames() {
  return await api<BoardName[]>("/boards");
}

export async function getBoard(
  boardShortName: string,
): Promise<GetBoardResponse> {
  const data = await api<GetBoardResponse>(`/boards/${boardShortName}`);
  return {
    board: {
      ...data.board,
      threads: data.board.threads.map((thread) => ({
        ...thread,
        createdAt: new Date(thread.createdAt),
        lastBumpAt: new Date(thread.lastBumpAt),
      })),
    },

    nextCursor: data.nextCursor,
  };
}

export async function getBoardThreads(
  boardShortName: string,
  cursor: string | null,
): Promise<{ items: Thread[]; nextCursor: string | null }> {
  const params = new URLSearchParams();

  if (cursor) {
    params.append("cursor", cursor);
  }

  const data = await api<GetThreadResponse>(
    `/boards/${boardShortName}/threads?${params}`,
  );

  return {
    items: data.threads.map((thread) => ({
      ...thread,
      createdAt: new Date(thread.createdAt),
      lastBumpAt: new Date(thread.lastBumpAt),
    })),
    nextCursor: data.nextCursor,
  };
}
