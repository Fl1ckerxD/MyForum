import { api } from "../../api/axios";
import type { Ban } from "../../types/ban";

export interface CreateBanRequest {
  ipHash: string;
  boardId?: number;
  reason: string;
  expiresAt?: string;
}

export const bansApi = {
  getAll: (params: {
    limit: number;
    beforeId?: number;
    status?: "active" | "expired" | "revoked";
    boardShortName?: string;
  }) => api.get("/admin/bans", { params }).then((r) => r.data as Ban[]),

  create: (data: CreateBanRequest) => api.post("/admin/bans", data),

  unban: (id: number) => api.delete(`/admin/bans/${id}`),
};
