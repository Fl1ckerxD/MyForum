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

export const ThreadsPage = () => {
    const queryClient = useQueryClient();

    const [search, setSearch] = useState("");
    const [board, setBoard] = useState<string | undefined>();
    const [isDeleted, setIsDeleted] = useState<boolean | undefined>();
    const [isLocked, setIsLocked] = useState<boolean | undefined>();

    const {
        data,
        fetchNextPage,
        hasNextPage,
        isFetching,
    } = useInfiniteQuery<
        AdminThreadDto[],
        Error,
        InfiniteData<AdminThreadDto[], string | undefined>,
        (string | boolean | undefined)[],
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

    const softDeleteMutation = useMutation({
        mutationFn: threadsApi.softDelete,
        onSuccess: invalidate,
    });

    const hardDeleteMutation = useMutation({
        mutationFn: threadsApi.hardDelete,
        onSuccess: invalidate,
    });

    const restoreMutation = useMutation({
        mutationFn: threadsApi.restore,
        onSuccess: invalidate,
    });

    const lockMutation = useMutation({
        mutationFn: threadsApi.lock,
        onSuccess: invalidate,
    });

    const unlockMutation = useMutation({
        mutationFn: threadsApi.unlock,
        onSuccess: invalidate,
    });

    const threads = data?.pages.flat() ?? [];

    const handleHardDelete = async (id: number) => {
        if (!confirm("Удалить этот тред?")) return;
        await hardDeleteMutation.mutateAsync(id);
    };

    return (
        <div style={{ padding: 20 }}>
            <h2>Треды</h2>

            <div style={{ marginBottom: 20 }}>
                <input
                    placeholder="Поиск по названию..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />

                <input
                    placeholder="Доска (например, b)"
                    value={board ?? ""}
                    onChange={e =>
                        setBoard(e.target.value || undefined)
                    }
                />

                <select
                    onChange={e =>
                        setIsDeleted(
                            e.target.value === ""
                                ? undefined
                                : e.target.value === "true"
                        )
                    }
                >
                    <option value="">Удалено: все</option>
                    <option value="true">Только удаленные</option>
                    <option value="false">Только активные</option>
                </select>

                <select
                    onChange={e =>
                        setIsLocked(
                            e.target.value === ""
                                ? undefined
                                : e.target.value === "true"
                        )
                    }
                >
                    <option value="">Заблокировано: все</option>
                    <option value="true">Только заблокированные</option>
                    <option value="false">Только разблокированные</option>
                </select>
            </div>

            <table width="100%" border={1} cellPadding={6}>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Доска</th>
                        <th>Название</th>
                        <th>Посты</th>
                        <th>Статус</th>
                        <th>Действия</th>
                    </tr>
                </thead>

                <tbody>
                    {threads.map(t => (
                        <tr key={t.id} style={{ opacity: t.isDeleted ? 0.5 : 1 }}>
                            <td>{t.id}</td>
                            <td>/{t.boardShortName}/</td>
                            <td>{t.title}</td>
                            <td>{t.postsCount}</td>

                            <td>
                                {t.isDeleted && "Удалено"}
                                {t.isLocked && "Заблокировано"}
                            </td>

                            <td>
                                {!t.isDeleted && (
                                    <button
                                        onClick={() =>
                                            softDeleteMutation.mutate(t.id)
                                        }
                                    >
                                        Мягкое удаление
                                    </button>
                                )}

                                {t.isDeleted && (
                                    <button
                                        onClick={() =>
                                            restoreMutation.mutate(t.id)
                                        }
                                    >
                                        Восстановить
                                    </button>
                                )}

                                <button
                                    onClick={() =>
                                        handleHardDelete(t.id)
                                    }
                                >
                                    Жесткое удаление
                                </button>

                                <button
                                    onClick={() =>
                                        t.isLocked
                                            ? unlockMutation.mutate(t.id)
                                            : lockMutation.mutate(t.id)
                                    }
                                >
                                    {t.isLocked ? "Разблокировать" : "Заблокировать"}
                                </button>

                                <Link to={`/threads/${t.id}/posts`}>
                                    Просмотреть посты
                                </Link>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            <br />

            {hasNextPage && (
                <button onClick={() => fetchNextPage()}>
                    Загрузить ещё
                </button>
            )}

            {isFetching && <p>Loading...</p>}
        </div>
    );
};
