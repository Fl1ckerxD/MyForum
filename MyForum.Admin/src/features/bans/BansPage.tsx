import {
    useInfiniteQuery,
    useMutation,
    useQueryClient,
    type InfiniteData,
} from "@tanstack/react-query";
import { useState } from "react";
import { bansApi } from "./bansApi";
import type { Ban } from "../../types/ban";

export const BansPage = () => {
    const queryClient = useQueryClient();

    const [statusFilter, setStatusFilter] = useState<"active" | "expired" | "revoked" | undefined>();
    const [boardShortNameFilter, setBoardShortNameFilter] = useState<string | undefined>();

    const {
        data,
        fetchNextPage,
        hasNextPage,
        isFetching,
    } = useInfiniteQuery<
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

    const createMutation = useMutation({
        mutationFn: bansApi.create,
        onSuccess: invalidate,
    });

    const unbanMutation = useMutation({
        mutationFn: bansApi.unban,
        onSuccess: invalidate,
    });

    const bans = data?.pages.flat() ?? [];

    const [ipHash, setIpHash] = useState("");
    const [boardId, setBoardId] = useState<number | undefined>();
    const [reason, setReason] = useState("");
    const [expiresAt, setExpiresAt] = useState("");

    const handleCreate = async () => {
        if (!ipHash || !reason) {
            alert("IP Hash и причина обязательны");
            return;
        }

        await createMutation.mutateAsync({
            ipHash,
            boardId,
            reason,
            expiresAt: expiresAt || undefined,
        });

        setIpHash("");
        setBoardId(undefined);
        setReason("");
        setExpiresAt("");
    };

    return (
        <div style={{ padding: 20 }}>
            <h2>Баны</h2>

            <div style={{ marginBottom: 20 }}>
                <select
                    onChange={(e) =>
                        setStatusFilter(
                            e.target.value === ""
                                ? undefined
                                : (e.target.value as any)
                        )
                    }
                >
                    <option value="">Все</option>
                    <option value="active">Активные</option>
                    <option value="expired">Истёкшие</option>
                    <option value="revoked">Снятые</option>
                </select>

                <input
                    placeholder="Короткое имя доски"
                    onChange={(e) =>
                        setBoardShortNameFilter(
                            e.target.value
                                ? e.target.value
                                : undefined
                        )
                    }
                />
            </div>

            <div
                style={{
                    border: "1px solid #ccc",
                    padding: 10,
                    marginBottom: 20,
                }}
            >
                <h4>Создать бан</h4>

                <input
                    placeholder="IP Hash"
                    value={ipHash}
                    onChange={(e) => setIpHash(e.target.value)}
                />

                <input
                    placeholder="BoardId (опционально)"
                    type="number"
                    onChange={(e) =>
                        setBoardId(
                            e.target.value
                                ? Number(e.target.value)
                                : undefined
                        )
                    }
                />

                <input
                    placeholder="Причина"
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                />

                <input
                    type="datetime-local"
                    onChange={(e) =>
                        setExpiresAt(
                            e.target.value
                                ? new Date(e.target.value).toISOString()
                                : ""
                        )
                    }
                />

                <button onClick={handleCreate}>
                    Забанить
                </button>
            </div>

            <table width="100%" border={1} cellPadding={6}>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>IP Hash</th>
                        <th>Доска</th>
                        <th>Причина</th>
                        <th>Статус</th>
                        <th>Срок действия</th>
                        <th>Забанен</th>
                        <th>Действие</th>
                    </tr>
                </thead>

                <tbody>
                    {bans.map((b) => (
                        <tr
                            key={b.id}
                            style={{
                                opacity: b.isCurrentlyActive
                                    ? 1
                                    : 0.5,
                                backgroundColor: b.isExpired
                                    ? "#fff3cd"
                                    : undefined,
                            }}
                        >
                            <td>{b.id}</td>
                            <td>{b.ipAddressHash}</td>
                            <td>{b.boardShortName ?? "Глобальный"}</td>
                            <td>{b.reason}</td>
                            <td>
                                {b.isCurrentlyActive && "Активен"}
                                {b.isExpired && "Истекший"}
                                {!b.isActive && "Отменено"}
                            </td>
                            <td>
                                {b.expiresAt
                                    ? new Date(b.expiresAt).toLocaleString()
                                    : "Никогда"}
                            </td>
                            <td>
                                {new Date(b.bannedAt).toLocaleString()}
                            </td>
                            <td>
                                {b.isActive && (
                                    <button
                                        onClick={() =>
                                            unbanMutation.mutate(b.id)
                                        }
                                    >
                                        Разбанить
                                    </button>
                                )}
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
