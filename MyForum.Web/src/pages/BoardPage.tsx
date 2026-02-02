import { useParams, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import type { Board } from "../types/board";
import { getBoard, getBoardThreads } from "../api/boards.api";
import BoardDescription from "../components/BoardDescription";
import ThreadList from "../components/ThreadList";
import CreateThreadForm from "../components/CreateThreadForm";
import ButtonVisibility from "../components/ButtonVisibility";
import type { Thread } from "../types/thread";
import { useInfiniteScroll } from "../hooks/useInfiniteScroll";

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
    loadMore: cursor => getBoardThreads(boardShortName!, cursor),
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
    return <div>Загрузка...</div>;
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
              onCreated={(threadId) => {
                navigate(`/${board.shortName}/${threadId}`);
              }}
            />
          </>
        )}
      </section>
      <ThreadList boardShortName={boardShortName!} threads={threads} />
      {hasMore && (
        <div ref={loaderRef} style={{ height: 40 }} />
      )}

      {loading && <div>Загрузка...</div>}
    </>
  );
}
