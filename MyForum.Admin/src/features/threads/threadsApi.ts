import { api } from "../../api/axios";
import type { AdminThreadDto } from "../../types/thread";

export interface ThreadsQueryParams {
  limit: number;
  cursor?: string;
  search?: string;
  board?: string;
  isDeleted?: boolean;
  isLocked?: boolean;
}

export const threadsApi = {
  getAll: async (params: ThreadsQueryParams) => {
    const { data } = await api.get("/admin/threads", {
      params,
    });
    return data as AdminThreadDto[];
  },

  hardDelete: (id: number) => api.delete(`/admin/threads/${id}/delete`),

  softDelete: (id: number) => api.delete(`/admin/threads/${id}`),

  restore: (id: number) => api.post(`/admin/threads/${id}/restore`),

  lock: (id: number) => api.post(`/admin/threads/${id}/lock`),

  unlock: (id: number) => api.post(`/admin/threads/${id}/unlock`),

  pin: (id: number) => api.post(`/admin/threads/${id}/pin`),

  unpin: (id: number) => api.post(`/admin/threads/${id}/unpin`),
};
