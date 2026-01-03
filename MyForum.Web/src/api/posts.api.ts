import { api } from "./http";
import type { Post } from "../types/post";

export function getPosts() {
  return api<Post[]>("/posts");
}
