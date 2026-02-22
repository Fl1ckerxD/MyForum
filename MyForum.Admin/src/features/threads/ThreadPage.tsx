import { useState } from "react";
import {
  type InfiniteData,
  useInfiniteQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { threadsApi } from "./threadsApi";
import type { AdminThreadDto } from "../../types/thread";
import { Link } from "react-router-dom";
import { Icon } from "../../components/Icon";
import { PageHeader } from "../../components/PageHeader";
import { ActionRow } from "../../components/ActionRow";
import { MoreMenu } from "../../components/MoreMenu";
import { LoadMoreBar } from "../../components/LoadMoreBar";
import "./ThreadPage.css";

export const ThreadsPage = () => {
  const queryClient = useQueryClient();

  const [search, setSearch] = useState("");
  const [board, setBoard] = useState<string | undefined>();
  const [isDeleted, setIsDeleted] = useState<boolean | undefined>();
  const [isLocked, setIsLocked] = useState<boolean | undefined>();

  const { data, fetchNextPage, hasNextPage, isFetching } = useInfiniteQuery<
    AdminThreadDto[],
    Error,
    InfiniteData<AdminThreadDto[], string | undefined>,
    ["threads", string, string | undefined, boolean | undefined, boolean | undefined],
    string | undefined
  >({
    queryKey: ["threads", search, board, isDeleted, isLocked],
    queryFn: ({ pageParam }) =>
      threadsApi.getAll({
        limit: 50,
        cursor: pageParam,
        search: search || undefined,
        board,
        isDeleted,
        isLocked,
      }),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => {
      if (lastPage.length === 0) return undefined;
      return lastPage[lastPage.length - 1].lastBumpAt;
    },
  });

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ["threads"] });

  const softDeleteMutation = useMutation({ mutationFn: threadsApi.softDelete, onSuccess: invalidate });
  const hardDeleteMutation = useMutation({ mutationFn: threadsApi.hardDelete, onSuccess: invalidate });
  const restoreMutation = useMutation({ mutationFn: threadsApi.restore, onSuccess: invalidate });
  const lockMutation = useMutation({ mutationFn: threadsApi.lock, onSuccess: invalidate });
  const unlockMutation = useMutation({ mutationFn: threadsApi.unlock, onSuccess: invalidate });
  const pinMutation = useMutation({ mutationFn: threadsApi.pin, onSuccess: invalidate });
  const unpinMutation = useMutation({ mutationFn: threadsApi.unpin, onSuccess: invalidate });

  const threads = data?.pages.flat() ?? [];

  const handleHardDelete = async (id: number) => {
    if (!confirm("Удалить этот тред?")) return;
    await hardDeleteMutation.mutateAsync(id);
  };

  return (
    <section className="admin-page threads-page">
      <PageHeader
        icon="threads"
        title="Треды"
        subtitle="Модерация тредов, фильтрация и быстрые действия"
      />

      <div className="admin-card threads-page__table-card">
        <div className="admin-toolbar">
          <input
            className="admin-field"
            placeholder="Поиск по названию"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />

          <input
            className="admin-field"
            placeholder="Доска, например b"
            value={board ?? ""}
            onChange={(e) => setBoard(e.target.value || undefined)}
          />

          <select
            className="admin-select"
            onChange={(e) => setIsDeleted(e.target.value === "" ? undefined : e.target.value === "true")}
          >
            <option value="">Удалено: все</option>
            <option value="true">Только удаленные</option>
            <option value="false">Только активные</option>
          </select>

          <select
            className="admin-select"
            onChange={(e) => setIsLocked(e.target.value === "" ? undefined : e.target.value === "true")}
          >
            <option value="">Блокировка: все</option>
            <option value="true">Только заблокированные</option>
            <option value="false">Только разблокированные</option>
          </select>
        </div>

        {threads.length === 0 && !isFetching ? (
          <div className="admin-empty">Треды не найдены</div>
        ) : (
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Доска</th>
                  <th>Название</th>
                  <th>Постов</th>
                  <th>Статус</th>
                  <th>Действия</th>
                </tr>
              </thead>
              <tbody>
                {threads.map((thread) => (
                  <tr key={thread.id} className={thread.isDeleted ? "threads-page__deleted" : undefined}>
                    <td>{thread.id}</td>
                    <td>/{thread.boardShortName}/</td>
                    <td>{thread.title}</td>
                    <td>{thread.postsCount}</td>

                    <td>
                      {thread.isDeleted ? (
                        <span className="admin-pill admin-pill-danger">Удален</span>
                      ) : thread.isLocked ? (
                        <span className="admin-pill admin-pill-warning">Заблокирован</span>
                      ) : (
                        <span className="admin-pill admin-pill-success">Активен</span>
                      )}
                    </td>

                    <td>
                      <ActionRow>
                        <Link className="admin-btn admin-btn-nav" to={`/threads/${thread.id}/posts`}>
                          <Icon name="posts" size={14} />
                          Посты
                        </Link>

                        <button
                          className={`admin-btn ${thread.isPinned ? "admin-btn-pin" : "admin-btn-warning"}`}
                          onClick={() => (thread.isPinned ? unpinMutation.mutate(thread.id) : pinMutation.mutate(thread.id))}
                        >
                          <Icon
                            name="pin"
                            size={14}
                            fill={thread.isPinned ? "currentColor" : "none"}
                          />
                          {thread.isPinned ? "Открепить" : "Закрепить"}
                        </button>

                        <button
                          className={`admin-btn ${thread.isLocked ? "admin-btn-success" : "admin-btn-warning"}`}
                          onClick={() => (thread.isLocked ? unlockMutation.mutate(thread.id) : lockMutation.mutate(thread.id))}
                        >
                          <Icon name={thread.isLocked ? "unlock" : "lock"} size={14} />
                          {thread.isLocked ? "Разблокировать" : "Заблокировать"}
                        </button>

                        <MoreMenu>
                          <button
                            className={`admin-btn ${thread.isDeleted ? "admin-btn-success" : "admin-btn-warning"}`}
                            onClick={() => (thread.isDeleted ? restoreMutation.mutate(thread.id) : softDeleteMutation.mutate(thread.id))}
                          >
                            <Icon name={thread.isDeleted ? "refresh" : "delete"} size={14} />
                            {thread.isDeleted ? "Восстановить" : "Мягко удалить"}
                          </button>

                          <button className="admin-btn admin-btn-danger" onClick={() => handleHardDelete(thread.id)}>
                            <Icon name="hammer" size={14} />
                            Жестко удалить
                          </button>
                        </MoreMenu>
                      </ActionRow>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <LoadMoreBar hasNextPage={hasNextPage} isFetching={isFetching} onLoadMore={() => fetchNextPage()} />
      </div>
    </section>
  );
};
