import { useEffect, useState } from "react";
import type { BoardName } from "../types/boardName";
import { getBoardNames } from "../api/boards.api";
import MainHeader from "../components/MainHeader";
import MainDescription from "../components/MainDescription";
import MainBoardList from "../components/MainBoardList";

export default function HomePage() {
  const [boards, setBoards] = useState<BoardName[]>([]);

  useEffect(() => {
    getBoardNames().then(setBoards);
  }, []);

  return (
    <>
      <MainHeader />
      <MainDescription />
      <MainBoardList boards={boards} />
    </>
  );
}
