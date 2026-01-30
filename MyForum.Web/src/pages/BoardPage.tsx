import { useParams, useNavigate } from "react-router-dom";
import { useEffect, useRef, useState } from "react";
import type { Board } from "../types/board";
import { getBoard, getBoardThreads } from "../api/boards.api";
import BoardDescription from "../components/BoardDescription";
import ThreadList from "../components/ThreadList";
import CreateThreadForm from "../components/CreateThreadForm";
import ButtonVisibility from "../components/ButtonVisibility";
import type { Thread } from "../types/thread";

export default function BoardPage() {
  const { boardShortName } = useParams<{ boardShortName: string }>();
  const [board, setBoard] = useState<Board | null>(null);
  const [createThreadVisible, setCreateThreadVisible] = useState(false);
  const [threads, setThreads] = useState<Thread[]>([]);
  const [cursor, setCursor] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(true);
  const [loading, setLoading] = useState(false);
  const loaderRef = useRef<HTMLDivElement | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (!boardShortName) return;

    const loadBoard = async () => {
      setLoading(true);

      const board = await getBoard(boardShortName);

      setBoard(board);
      setThreads(board?.threads);
      if (board?.threads && board.threads.length === 20) {
        setCursor(board?.threads && board.threads.length > 0 ? board.threads[board.threads.length - 1].lastBumpAt.toString() : null);
        setHasMore(board?.threads && board.threads.length === 20);
      }

      setLoading(false);
    };

    loadBoard();
  }, [boardShortName]);

  const loadThreads = async () => {
    if (loading || !hasMore || !cursor) return;

    setLoading(true);

    const result = await getBoardThreads(boardShortName!, cursor);

    setThreads(prev => [...prev, ...result.threads]);
    setCursor(result.nextCursor);
    setHasMore(Boolean(result.nextCursor));

    setLoading(false);
  };

  useEffect(() => {
    const observer = new IntersectionObserver(entries => {
      if (entries[0].isIntersecting) {
        loadThreads();
      }
    });

    if (loaderRef.current) {
      observer.observe(loaderRef.current);
    }

    return () => observer.disconnect();
  }, [cursor, hasMore]);

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
