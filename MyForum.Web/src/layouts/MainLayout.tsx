import { Link, Outlet } from "react-router-dom";
import BoardList from "../components/BoardList/BoardList";
import { useEffect, useState } from "react";
import type { BoardName } from "../types/boardName";
import { getBoardNames } from "../api/boards.api";
import "../styles/layout/layout.css";

export default function MainLayout() {
    const [boards, setBoards] = useState<BoardName[]>([]);

    useEffect(() => {
        const loadBoards = async () => {
            const boards = await getBoardNames();
            setBoards(boards);
        };

        loadBoards();
    }, []);

    return (
        <div className="layout">
            <aside className="layout-sidebar">
                <Link to="/" className="link p-2 pd-4">
                    Главная
                </Link>
                <BoardList boards={boards} />
            </aside>

            <main className="content">
                <Outlet />
            </main>
        </div>

    );
}
