import { api } from "./http";
import type { BoardName } from "../types/boardName";

export async function getBoardNames() {
  return await api<BoardName[]>("/boards");
}
