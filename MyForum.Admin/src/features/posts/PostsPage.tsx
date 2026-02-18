import { useInfiniteQuery, useMutation, useQueryClient, type InfiniteData } from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import { postsApi } from "./postsApi";
import type { AdminPostDto } from "../../types/post";
import { useEffect, useState } from "react";

export const PostsPage = () => {
    const { threadId } = useParams();
    const parsedThreadId = Number(threadId);
    const isValidThreadId = Number.isInteger(parsedThreadId) && parsedThreadId > 0;

    const queryClient = useQueryClient();

    const [searchInput, setSearchInput] = useState("");
    const [search, setSearch] = useState("");
    const [deletedFilter, setDeletedFilter] = useState<boolean | undefined>();

    useEffect(() => {
        const timeout = setTimeout(() => {
            setSearch(searchInput.trim());
        }, 400);

        return () => clearTimeout(timeout);
    }, [searchInput]);

    const {
        data,
        fetchNextPage,
        hasNextPage,
        isFetching,
    } = useInfiniteQuery<
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
        queryClient.invalidateQueries({
            queryKey: ["posts", parsedThreadId],
        });

    const softDeleteMutation = useMutation({
        mutationFn: postsApi.softDelete,
        onSuccess: invalidate,
    });

    const restoreMutation = useMutation({
        mutationFn: postsApi.restore,
        onSuccess: invalidate,
    });

    const hardDeleteMutation = useMutation({
        mutationFn: postsApi.hardDelete,
        onSuccess: invalidate,
    });

    if (!isValidThreadId) {
        return (
            <div style={{ padding: 20 }}>
                <h2>Некорректный threadId</h2>
            </div>
        );
    }

    const posts = data?.pages.flat() ?? [];

    const handleHardDelete = async (id: number) => {
        if (!confirm("Delete this post?")) return;
        await hardDeleteMutation.mutateAsync(id);
    };

    return (
        <div style={{ padding: 20 }}>
            <h2>Посты от треда #{threadId}</h2>

            <div style={{ marginBottom: 20 }}>
                <input
                    placeholder="Поиск по содержимому..."
                    value={searchInput}
                    onChange={(e) => setSearchInput(e.target.value)}
                />

                <select
                    onChange={(e) =>
                        setDeletedFilter(
                            e.target.value === ""
                                ? undefined
                                : e.target.value === "true"
                        )
                    }
                >
                    <option value="">Все</option>
                    <option value="false">Только активные</option>
                    <option value="true">Только удаленные</option>
                </select>
            </div>

            <table width="100%" border={1} cellPadding={6}>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Тип</th>
                        <th>Автор</th>
                        <th>Содержание</th>
                        <th>Статус</th>
                        <th>Удалено в</th>
                        <th>Создано в</th>
                        <th>Действия</th>
                    </tr>
                </thead>

                <tbody>
                    {posts.map((p) => (
                        <tr key={p.id} style={{ opacity: p.isDeleted ? 0.5 : 1 }}>
                            <td>{p.id}</td>
                            <td>{p.isOriginal ? "ОП" : "Ответ"}</td>
                            <td>{p.author}</td>
                            <td style={{ maxWidth: 400 }}>{p.content}</td>
                            <td>{p.isDeleted ? "Удалено" : "Активно"}</td>
                            <td>{p.deletedAt ? new Date(p.deletedAt).toLocaleString() : "-"}</td>
                            <td>{new Date(p.createdAt).toLocaleString()}</td>
                            <td>
                                {!p.isDeleted && (
                                    <button onClick={() => softDeleteMutation.mutate(p.id)}>
                                        Мягкое удаление
                                    </button>
                                )}

                                {p.isDeleted && (
                                    <button onClick={() => restoreMutation.mutate(p.id)}>
                                        Восстановить
                                    </button>
                                )}

                                <button onClick={() => handleHardDelete(p.id)}>
                                    Жесткое удаление
                                </button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            <br />

            {hasNextPage && (
                <button onClick={() => fetchNextPage()}>
                    Загрузить еще
                </button>
            )}

            {isFetching && <p>Loading...</p>}
        </div>
    );
};
