import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { Board } from "../../types/board";
import { boardsApi } from "./boardsApi";

type HiddenFilter = "" | "true" | "false";

export const BoardsPage = () => {
    const queryClient = useQueryClient();

    const [name, setName] = useState("");
    const [shortName, setShortName] = useState("");
    const [description, setDescription] = useState("");
    const [editingId, setEditingId] = useState<number | null>(null);
    const [searchName, setSearchName] = useState("");
    const [hiddenFilter, setHiddenFilter] = useState<HiddenFilter>("");

    const { data: boards = [], isLoading } = useQuery<Board[]>({
        queryKey: ["boards"],
        queryFn: boardsApi.getAll,
    });

    const invalidateBoards = () =>
        queryClient.invalidateQueries({ queryKey: ["boards"] });

    const createMutation = useMutation({
        mutationFn: boardsApi.create,
        onSuccess: invalidateBoards,
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, payload }: { id: number; payload: { name: string; shortName: string; description: string } }) =>
            boardsApi.update(id, payload),
        onSuccess: invalidateBoards,
    });

    const deleteMutation = useMutation({
        mutationFn: boardsApi.delete,
        onSuccess: invalidateBoards,
    });

    const visibilityMutation = useMutation({
        mutationFn: ({ id, isHidden }: { id: number; isHidden: boolean }) =>
            boardsApi.patchVisibility(id, isHidden),
        onSuccess: invalidateBoards,
    });

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (editingId) {
            await updateMutation.mutateAsync({
                id: editingId,
                payload: { name, shortName, description },
            });
        } else {
            await createMutation.mutateAsync({ name, shortName, description });
        }

        setName("");
        setShortName("");
        setDescription("");
        setEditingId(null);
    };

    const handleEdit = (board: Board) => {
        setName(board.name);
        setShortName(board.shortName);
        setDescription(board.description);
        setEditingId(board.id);
    };

    const handleDelete = async (id: number) => {
        if (!confirm("Удалить эту доску?")) return;
        await deleteMutation.mutateAsync(id);
    };

    const handleToggleVisibility = async (board: Board) => {
        await visibilityMutation.mutateAsync({
            id: board.id,
            isHidden: !board.isHidden,
        });
    };

    const filteredBoards = useMemo(() => {
        const normalizedSearch = searchName.trim().toLowerCase();

        return boards.filter((board) => {
            const matchesName = normalizedSearch.length === 0
                || board.name.toLowerCase().includes(normalizedSearch);

            const matchesHidden = hiddenFilter === ""
                || (hiddenFilter === "true" ? board.isHidden : !board.isHidden);

            return matchesName && matchesHidden;
        });
    }, [boards, searchName, hiddenFilter]);

    return (
        <div style={{ padding: 20 }}>
            <h2>Доски</h2>

            <form onSubmit={handleSubmit} style={{ marginBottom: 20 }}>
                <input
                    placeholder="Название доски"
                    value={name}
                    onChange={e => setName(e.target.value)}
                    required
                />
                <input
                    placeholder="Короткое имя доски"
                    value={shortName}
                    onChange={e => setShortName(e.target.value)}
                    required
                />
                <input
                    placeholder="Описание"
                    value={description}
                    onChange={e => setDescription(e.target.value)}
                />
                <button type="submit">
                    {editingId ? "Обновить" : "Создать"}
                </button>
                {editingId && (
                    <button
                        type="button"
                        onClick={() => {
                            setEditingId(null);
                            setName("");
                            setShortName("");
                            setDescription("");
                        }}
                    >
                        Отмена
                    </button>
                )}
            </form>

            <div style={{ marginBottom: 16 }}>
                <input
                    placeholder="Поиск по имени"
                    value={searchName}
                    onChange={e => setSearchName(e.target.value)}
                />
                <select
                    value={hiddenFilter}
                    onChange={e => setHiddenFilter(e.target.value as HiddenFilter)}
                    style={{ marginLeft: 8 }}
                >
                    <option value="">Скрытие: все</option>
                    <option value="false">Только видимые</option>
                    <option value="true">Только скрытые</option>
                </select>
            </div>

            {isLoading ? (
                <p>Loading...</p>
            ) : (
                <table border={1} cellPadding={8}>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Имя</th>
                            <th>Короткое имя</th>
                            <th>Описание</th>
                            <th>Статус</th>
                            <th>Создано</th>
                            <th>Действия</th>
                        </tr>
                    </thead>
                    <tbody>
                        {filteredBoards.map(board => (
                            <tr key={board.id} style={{ opacity: board.isHidden ? 0.5 : 1 }}>
                                <td>{board.id}</td>
                                <td>{board.name}</td>
                                <td>{board.shortName}</td>
                                <td>{board.description}</td>
                                <td>{board.isHidden ? "Скрыта" : "Видима"}</td>
                                <td>{new Date(board.createdAt).toLocaleString()}</td>
                                <td>
                                    <button onClick={() => handleEdit(board)}>
                                        Редактировать
                                    </button>
                                    <button onClick={() => handleToggleVisibility(board)}>
                                        {board.isHidden ? "Показать" : "Скрыть"}
                                    </button>
                                    <button onClick={() => handleDelete(board.id)}>
                                        Удалить
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
};