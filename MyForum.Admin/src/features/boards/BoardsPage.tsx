import { useEffect, useState } from "react";
import type { Board } from "../../types/board";
import { boardsApi } from "./boardsApi";

export const BoardsPage = () => {
    const [boards, setBoards] = useState<Board[]>([]);
    const [loading, setLoading] = useState(true);

    const [name, setName] = useState("");
    const [shortName, setShortName] = useState("");
    const [description, setDescription] = useState("");
    const [isHidden, setHidden] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);

    const loadBoards = async () => {
        setLoading(true);
        const data = await boardsApi.getAll();
        setBoards(data);
        setLoading(false);
    };

    useEffect(() => {
        loadBoards();
    }, []);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (editingId) {
            await boardsApi.update(editingId, { name, shortName, description, isHidden });
        } else {
            await boardsApi.create({ name, shortName, description });
        }

        setName("");
        setShortName("");
        setDescription("");
        setHidden(false);
        setEditingId(null);
        await loadBoards();
    };

    const handleEdit = (board: Board) => {
        setName(board.name);
        setShortName(board.shortName);
        setDescription(board.description);
        setEditingId(board.id);
    };

    const handleDelete = async (id: number) => {
        if (!confirm("Delete this board?")) return;
        await boardsApi.delete(id);
        await loadBoards();
    };

    return (
        <div>
            <h2>Boards</h2>

            <form onSubmit={handleSubmit} style={{ marginBottom: 20 }}>
                <input
                    placeholder="Board name"
                    value={name}
                    onChange={e => setName(e.target.value)}
                    required
                />
                <input
                    placeholder="Board short name"
                    value={shortName}
                    onChange={e => setShortName(e.target.value)}
                    required
                />
                <input
                    placeholder="Description"
                    value={description}
                    onChange={e => setDescription(e.target.value)}
                />
                <button type="submit">
                    {editingId ? "Update" : "Create"}
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
                        Cancel
                    </button>
                )}
            </form>

            {loading ? (
                <p>Loading...</p>
            ) : (
                <table border={1} cellPadding={8}>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Name</th>
                            <th>ShortName</th>
                            <th>Description</th>
                            <th>Created</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {boards.map(board => (
                            <tr key={board.id}>
                                <td>{board.id}</td>
                                <td>{board.name}</td>
                                <td>{board.shortName}</td>
                                <td>{board.description}</td>
                                <td>{new Date(board.createdAt).toLocaleString()}</td>
                                <td>
                                    <button onClick={() => handleEdit(board)}>
                                        Edit
                                    </button>
                                    <button onClick={() => handleDelete(board.id)}>
                                        Delete
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