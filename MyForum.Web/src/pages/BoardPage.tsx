import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getBoard, getBoardThreads } from "../api/boards.api";
import BoardDescription from "../components/BoardDescription";
import ButtonVisibility from "../components/ButtonVisibility";
import CreateThreadForm from "../components/CreateThreadForm";
import ThreadList from "../components/ThreadList";
import { useInfiniteScroll } from "../hooks/useInfiniteScroll";
import type { Board } from "../types/board";
import type { Thread } from "../types/thread";

export default function BoardPage() {
  const { boardShortName } = useParams<{ boardShortName: string }>();
  const [board, setBoard] = useState<Board | null>(null);
  const [createThreadVisible, setCreateThreadVisible] = useState(false);
  const navigate = useNavigate();

  const {
    items: threads,
    setItems,
    setCursor,
    hasMore,
    loading,
    loaderRef,
  } = useInfiniteScroll<Thread>({
    loadMore: (cursor) => getBoardThreads(boardShortName!, cursor, 20),
  });

  useEffect(() => {
    const loadBoard = async () => {
      const response = await getBoard(boardShortName!);

      setBoard(response.board);
      setItems(response.board.threads);
      setCursor(response.nextCursor);
    };

    loadBoard();
  }, [boardShortName]);

  if (!board) {
    return <div className="page-container loading">Загрузка...</div>;
  }

  return (
    <main className="page-stack">
      <BoardDescription name={board.name} description={board.description} />

      <section className="page-container action-block fade-in-up delay-200ms">
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
              onCreated={(threadId) => {
                navigate(`/${board.shortName}/${threadId}`);
              }}
            />
          </>
        )}
      </section>

      <ThreadList boardShortName={boardShortName!} threads={threads} />

      {hasMore && <div ref={loaderRef} className="scroll-loader" />}
      {loading && <div className="page-container loading">Загрузка...</div>}
    </main>
  );
}
