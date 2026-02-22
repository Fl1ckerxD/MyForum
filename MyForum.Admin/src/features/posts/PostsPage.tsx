import {
  useInfiniteQuery,
  useMutation,
  useQueryClient,
  type InfiniteData,
} from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import { postsApi } from "./postsApi";
import type { AdminPostDto } from "../../types/post";
import { useEffect, useState } from "react";
import { Icon } from "../../components/Icon";
import { PageHeader } from "../../components/PageHeader";
import { ActionRow } from "../../components/ActionRow";
import { LoadMoreBar } from "../../components/LoadMoreBar";
import { BanPostModal } from "./components/BanPostModal";
import "./PostsPage.css";

export const PostsPage = () => {
  const { threadId } = useParams();
  const parsedThreadId = Number(threadId);
  const isValidThreadId = Number.isInteger(parsedThreadId) && parsedThreadId > 0;

  const queryClient = useQueryClient();

  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [deletedFilter, setDeletedFilter] = useState<boolean | undefined>();

  const [banPostId, setBanPostId] = useState<number | null>(null);
  const [banBoardId, setBanBoardId] = useState<number | undefined>();
  const [banReason, setBanReason] = useState("");
  const [banExpiresAt, setBanExpiresAt] = useState("");
  const [banBoardOnly, setBanBoardOnly] = useState(true);

  useEffect(() => {
    const timeout = setTimeout(() => {
      setSearch(searchInput.trim());
    }, 380);

    return () => clearTimeout(timeout);
  }, [searchInput]);

  const { data, fetchNextPage, hasNextPage, isFetching } = useInfiniteQuery<
    AdminPostDto[],
    Error,
    InfiniteData<AdminPostDto[], number | undefined>,
    ["posts", number, string, boolean | undefined],
    number | undefined
  >({
    queryKey: ["posts", parsedThreadId, search, deletedFilter],
    enabled: isValidThreadId,
    queryFn: ({ pageParam }) =>
      postsApi.getByThread(parsedThreadId, {
        limit: 50,
        afterId: pageParam,
        search: search || undefined,
        isDeleted: deletedFilter,
      }),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => {
      if (lastPage.length === 0) return undefined;
      return lastPage[lastPage.length - 1].id;
    },
  });

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ["posts", parsedThreadId] });

  const softDeleteMutation = useMutation({ mutationFn: postsApi.softDelete, onSuccess: invalidate });
  const restoreMutation = useMutation({ mutationFn: postsApi.restore, onSuccess: invalidate });
  const hardDeleteMutation = useMutation({ mutationFn: postsApi.hardDelete, onSuccess: invalidate });

  const banMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: {
        boardId?: number;
        reason: string;
        expiresAt?: string;
      };
    }) => postsApi.ban(id, data),

    onSuccess: () => {
      setBanPostId(null);
      setBanReason("");
      setBanExpiresAt("");
      setBanBoardOnly(true);
    },
  });

  const openBanModal = (post: AdminPostDto) => {
    setBanPostId(post.id);
    setBanBoardId(post.boardId);
    setBanBoardOnly(true);
    setBanReason("");
    setBanExpiresAt("");
  };

  const closeBanModal = () => {
    setBanPostId(null);
    setBanReason("");
    setBanExpiresAt("");
    setBanBoardOnly(true);
  };

  const handleBanSubmit = async () => {
    if (!banPostId) return;

    if (!banReason.trim()) {
      alert("Причина обязательна");
      return;
    }

    try {
      await banMutation.mutateAsync({
        id: banPostId,
        data: {
          boardId: banBoardOnly ? banBoardId : undefined,
          reason: banReason,
          expiresAt: banExpiresAt ? new Date(banExpiresAt).toISOString() : undefined,
        },
      });
    } catch (err: any) {
      alert(err?.response?.data?.message ?? "Ошибка бана");
    }
  };

  if (!isValidThreadId) {
    return (
      <section className="admin-page">
        <div className="admin-card" style={{ padding: "1rem" }}>
          <h2 className="admin-page-title">
            <Icon name="warning" />
            Некорректный threadId
          </h2>
        </div>
      </section>
    );
  }

  const posts = data?.pages.flat() ?? [];

  const handleHardDelete = async (id: number) => {
    if (!confirm("Удалить этот пост?")) return;
    await hardDeleteMutation.mutateAsync(id);
  };

  return (
    <section className="admin-page posts-page">
      <PageHeader
        icon="posts"
        title={`Посты треда #${threadId}`}
        subtitle="Поиск, модерация и бан автора напрямую из поста"
      />

      <div className="admin-card posts-page__table-card">
        <div className="admin-toolbar">
          <input
            className="admin-field"
            placeholder="Поиск по содержимому"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />

          <select
            className="admin-select"
            onChange={(e) => setDeletedFilter(e.target.value === "" ? undefined : e.target.value === "true")}
          >
            <option value="">Все посты</option>
            <option value="false">Только активные</option>
            <option value="true">Только удаленные</option>
          </select>
        </div>

        {posts.length === 0 && !isFetching ? (
          <div className="admin-empty">Посты не найдены</div>
        ) : (
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Тип</th>
                  <th>Автор</th>
                  <th>Содержимое</th>
                  <th>Статус</th>
                  <th>Удален</th>
                  <th>Создан</th>
                  <th>Действия</th>
                </tr>
              </thead>

              <tbody>
                {posts.map((post) => (
                  <tr key={post.id} className={post.isDeleted ? "posts-page__deleted" : undefined}>
                    <td>{post.id}</td>
                    <td>{post.isOriginal ? "ОП" : "Ответ"}</td>
                    <td>{post.author}</td>
                    <td className="posts-page__content-cell">{post.content}</td>
                    <td>
                      {post.isDeleted ? (
                        <span className="admin-pill admin-pill-danger">Удален</span>
                      ) : (
                        <span className="admin-pill admin-pill-success">Активен</span>
                      )}
                    </td>
                    <td>{post.deletedAt ? new Date(post.deletedAt).toLocaleString() : "-"}</td>
                    <td>{new Date(post.createdAt).toLocaleString()}</td>
                    <td>
                      <ActionRow>
                        {!post.isDeleted && (
                          <button className="admin-btn admin-btn-warning" onClick={() => softDeleteMutation.mutate(post.id)}>
                            <Icon name="delete" size={14} />
                            Мягко удалить
                          </button>
                        )}

                        {post.isDeleted && (
                          <button className="admin-btn admin-btn-neutral" onClick={() => restoreMutation.mutate(post.id)}>
                            <Icon name="refresh" size={14} />
                            Восстановить
                          </button>
                        )}

                        <button className="admin-btn admin-btn-danger" onClick={() => handleHardDelete(post.id)}>
                          <Icon name="hammer" size={14} />
                          Жестко удалить
                        </button>

                        <button className="admin-btn admin-btn-primary" onClick={() => openBanModal(post)}>
                          <Icon name="bans" size={14} />
                          Бан
                        </button>
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

      <BanPostModal
        postId={banPostId}
        reason={banReason}
        expiresAt={banExpiresAt}
        boardOnly={banBoardOnly}
        isSubmitting={banMutation.isPending}
        onReasonChange={setBanReason}
        onExpiresAtChange={setBanExpiresAt}
        onBoardOnlyChange={setBanBoardOnly}
        onSubmit={handleBanSubmit}
        onClose={closeBanModal}
      />
    </section>
  );
};
