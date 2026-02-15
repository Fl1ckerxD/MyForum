import { api } from "../../api/axios";
import type { Board } from "../../types/board";

export const boardsApi = {
  getAll: async (): Promise<Board[]> => {
    const { data } = await api.get("/admin/boards");
    return data;
  },

  create: async (payload: {
    name: string;
    shortName: string;
    description: string;
  }): Promise<Board> => {
    const { data } = await api.post("/admin/boards", payload);
    return data;
  },

  update: async (
    id: number,
    payload: {
      name: string;
      shortName: string;
      description: string;
      isHidden: boolean;
    },
  ): Promise<void> => {
    await api.put(`/admin/boards/${id}`, payload);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/admin/boards/${id}`);
  },
};
