import { House } from "lucide-react";
import { useEffect, useState } from "react";
import { Link, Outlet } from "react-router-dom";
import { getBoardNames } from "../api/boards.api";
import BoardList from "../components/BoardList/BoardList";
import type { BoardName } from "../types/boardName";
import "./MainLayout.css";

export default function MainLayout() {
  const [boards, setBoards] = useState<BoardName[]>([]);

  useEffect(() => {
    const loadBoards = async () => {
      const boardNames = await getBoardNames();
      setBoards(boardNames);
    };

    loadBoards();
  }, []);

  return (
    <div className="layout">
      <aside className="layout-sidebar">
        <Link to="/" className="link layout-home-link">
          <House size={16} />
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
