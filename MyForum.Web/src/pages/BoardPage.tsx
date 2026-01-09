import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import type { Board } from "../types/board";
import { getBoard } from "../api/boards.api";
import BoardDescription from "../components/BoardDescription";
import ThreadList from "../components/ThreadList";
import CreateThreadForm from "../components/CreateThreadForm";
import ButtonVisibility from "../components/ButtonVisibility";

export default function BoardPage() {
  const { boardShortName } = useParams<{ boardShortName: string }>();
  const [board, setBoard] = useState<Board | null>(null);
  const [createThreadVisible, setCreateThreadVisible] = useState(false);

  useEffect(() => {
    if (!boardShortName) return;

    const loadBoard = async () => {
      const board = await getBoard(boardShortName);
      setBoard(board);
    };

    loadBoard();
  }, [boardShortName]);

  if (!boardShortName) {
    return <div>Invalid board</div>;
  }

  if (!board) {
    return <div>Loading...</div>;
  }

  return (
    <>
      <BoardDescription name={board.name} description={board.description} />
      <section className="fade-in-up delay-200ms d-flex align-items-center flex-column">
        {!createThreadVisible && (
          <ButtonVisibility onClick={() => setCreateThreadVisible(true)}>
            Создать тред
          </ButtonVisibility>
        )}
        {createThreadVisible && (
          <>
            <ButtonVisibility onClick={() => setCreateThreadVisible(false)}>
              Закрыть форму постинга
            </ButtonVisibility>
            <CreateThreadForm
              boardId={board.id}
              boardShortName={board.shortName}
              onCreated={() => {
                getBoard(board.shortName).then(setBoard);
              }}
            />
          </>
        )}
      </section>
      <ThreadList boardShortName={board.shortName} threads={board.threads} />
    </>
  );
}
