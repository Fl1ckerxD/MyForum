import { useEffect, useState } from "react";
import type { BoardName } from "../types/boardName";
import { getBoardNames } from "../api/boards.api";
import MainHeader from "../components/MainHeader/MainHeader";
import MainDescription from "../components/HomeDescription";
import MainBoardList from "../components/HomeBoardList";

export default function HomePage() {
  const [boards, setBoards] = useState<BoardName[]>([]);

  useEffect(() => {
    const loadBoards = async () => {
      const boards = await getBoardNames();
      setBoards(boards);
    };

    loadBoards();
  }, []);

  return (
    <main className="page-stack">
      <MainHeader />
      <MainDescription />
      <MainBoardList boards={boards} />
    </main>
  );
}
