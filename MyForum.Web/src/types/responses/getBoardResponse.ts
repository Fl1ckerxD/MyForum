import type { Board } from "../board";

export interface GetBoardResponse {
  board: Board;
  nextCursor: string | null;
}
