import { api } from "../../api/axios";
import type { AdminPostDto } from "../../types/post";

export interface PostsQueryParams {
  limit: number;
  afterId?: number;
  search?: string;
  isDeleted?: boolean;
}

export const postsApi = {
  getByThread: async (threadId: number, params: PostsQueryParams) => {
    const { data } = await api.get(`/admin/posts/thread/${threadId}`, {
      params,
    });
    return data as AdminPostDto[];
  },

  softDelete: (id: number) => api.delete(`/admin/posts/${id}`),

  restore: (id: number) => api.post(`/admin/posts/${id}/restore`),

  hardDelete: (id: number) => api.delete(`/admin/posts/${id}/delete`),
};
