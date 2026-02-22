import {
  useInfiniteQuery,
  useMutation,
  useQueryClient,
  type InfiniteData,
} from "@tanstack/react-query";
import { useState } from "react";
import { bansApi } from "./bansApi";
import type { Ban } from "../../types/ban";
import { Icon } from "../../components/Icon";
import { PageHeader } from "../../components/PageHeader";
import { ActionRow } from "../../components/ActionRow";
import { LoadMoreBar } from "../../components/LoadMoreBar";
import "./BansPage.css";

export const BansPage = () => {
  const queryClient = useQueryClient();

  const [statusFilter, setStatusFilter] = useState<"active" | "expired" | "revoked" | undefined>();
  const [boardShortNameFilter, setBoardShortNameFilter] = useState<string | undefined>();

  const [ipHash, setIpHash] = useState("");
  const [boardShortName, setBoardShortName] = useState<string | undefined>();
  const [reason, setReason] = useState("");
  const [expiresAt, setExpiresAt] = useState("");

  const { data, fetchNextPage, hasNextPage, isFetching } = useInfiniteQuery<
    Ban[],
    Error,
    InfiniteData<Ban[], number | undefined>,
    ["bans", string | undefined, string | undefined],
    number | undefined
  >({
    queryKey: ["bans", statusFilter, boardShortNameFilter],
    queryFn: ({ pageParam }) =>
      bansApi.getAll({
        limit: 50,
        beforeId: pageParam,
        status: statusFilter,
        boardShortName: boardShortNameFilter,
      }),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => {
      if (lastPage.length === 0) return undefined;
      return lastPage[lastPage.length - 1].id;
    },
  });

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ["bans"] });

  const createMutation = useMutation({ mutationFn: bansApi.create, onSuccess: invalidate });
  const unbanMutation = useMutation({ mutationFn: bansApi.unban, onSuccess: invalidate });

  const bans = data?.pages.flat() ?? [];

  const handleCreate = async () => {
    if (!ipHash || !reason) {
      alert("IP Hash и причина обязательны");
      return;
    }

    try {
      await createMutation.mutateAsync({
        ipHash,
        boardShortName: boardShortName,
        reason,
        expiresAt: expiresAt || undefined,
      });

      setIpHash("");
      setBoardShortName("");
      setReason("");
      setExpiresAt("");
    } catch (err: any) {
      const data = err?.response?.data;
      if (Array.isArray(data) && data.length > 0) {
        alert(data.map((e: any) => e.errorMessage ?? e.ErrorMessage).join("\n"));
      } else {
        alert(data?.message ?? "Не удалось создать бан");
      }
    }
  };

  return (
    <section className="admin-page bans-page">
      <PageHeader
        icon="bans"
        title="Баны"
        subtitle="История банов, фильтрация по статусам и ручное создание"
      />

      <div className="bans-page__grid">
        <div className="admin-card bans-page__create-card">
          <h3 className="bans-page__create-title">
            <Icon name="plus" size={16} />
            Создать бан
          </h3>

          <input className="admin-field" placeholder="IP Hash" value={ipHash} onChange={(e) => setIpHash(e.target.value)} />
          <input
            className="admin-field"
            placeholder="Короткое имя доски (опционально)"
            value={boardShortName}
            onChange={(e) => setBoardShortName(e.target.value)}
          />
          <textarea className="admin-textarea" placeholder="Причина" value={reason} onChange={(e) => setReason(e.target.value)} rows={3} />
          <input
            className="admin-field"
            type="datetime-local"
            value={expiresAt ? new Date(expiresAt).toISOString().slice(0, 16) : ""}
            onChange={(e) => setExpiresAt(e.target.value ? new Date(e.target.value).toISOString() : "")}
          />

          <button className="admin-btn admin-btn-primary" onClick={handleCreate} disabled={createMutation.isPending}>
            <Icon name="shield" size={15} />
            {createMutation.isPending ? "Создание..." : "Забанить"}
          </button>
        </div>

        <div className="admin-card bans-page__table-card">
          <div className="admin-toolbar">
            <select
              className="admin-select"
              onChange={(e) => setStatusFilter(e.target.value === "" ? undefined : (e.target.value as "active" | "expired" | "revoked"))}
            >
              <option value="">Статус: все</option>
              <option value="active">Активные</option>
              <option value="expired">Истекшие</option>
              <option value="revoked">Снятые</option>
            </select>

            <input
              className="admin-field"
              placeholder="Фильтр по короткому имени доски"
              onChange={(e) => setBoardShortNameFilter(e.target.value ? e.target.value : undefined)}
            />
          </div>

          {bans.length === 0 && !isFetching ? (
            <div className="admin-empty">Баны не найдены</div>
          ) : (
            <div className="admin-table-wrap">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>IP Hash</th>
                    <th>Доска</th>
                    <th>Причина</th>
                    <th>Статус</th>
                    <th>Срок</th>
                    <th>Забанен</th>
                    <th>Действие</th>
                  </tr>
                </thead>

                <tbody>
                  {bans.map((ban) => {
                    const rowClass = `${ban.isCurrentlyActive ? "" : "bans-page__row-muted"} ${ban.isExpired ? "bans-page__row-expired" : ""}`.trim();

                    return (
                      <tr key={ban.id} className={rowClass || undefined}>
                        <td>{ban.id}</td>
                        <td>{ban.ipAddressHash}</td>
                        <td>{ban.boardShortName ?? "Глобальный"}</td>
                        <td>{ban.reason}</td>
                        <td>
                          {ban.isCurrentlyActive ? (
                            <span className="admin-pill admin-pill-success">Активен</span>
                          ) : ban.isExpired ? (
                            <span className="admin-pill admin-pill-warning">Истек</span>
                          ) : (
                            <span className="admin-pill admin-pill-danger">Снят</span>
                          )}
                        </td>
                        <td>{ban.expiresAt ? new Date(ban.expiresAt).toLocaleString() : "Никогда"}</td>
                        <td>{new Date(ban.bannedAt).toLocaleString()}</td>
                        <td>
                          {ban.isActive && (
                            <ActionRow>
                              <button className="admin-btn admin-btn-danger" onClick={() => unbanMutation.mutate(ban.id)}>
                                <Icon name="unlock" size={14} />
                                Разбанить
                              </button>
                            </ActionRow>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}

          <LoadMoreBar hasNextPage={hasNextPage} isFetching={isFetching} onLoadMore={() => fetchNextPage()} />
        </div>
      </div>
    </section>
  );
};
