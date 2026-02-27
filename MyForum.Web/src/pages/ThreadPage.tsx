import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { getThread, getThreadPosts } from "../api/threads.api";
import BoardDescription from "../components/BoardDescription";
import ButtonVisibility from "../components/ButtonVisibility";
import CreatePostForm from "../components/CreatePostForm";
import PostList from "../components/PostList";
import ThreadPreview from "../components/ThreadPreview";
import { useInfiniteScroll } from "../hooks/useInfiniteScroll";
import type { Post } from "../types/post";
import type { Thread } from "../types/thread";

export default function ThreadPage() {
  const { boardShortName, threadId } = useParams<{
    boardShortName: string;
    threadId: string;
  }>();
  const parsedThreadId = Number(threadId);
  const [thread, setThread] = useState<Thread | null>(null);
  const [createPostVisible, setCreatePostVisible] = useState(false);

  const {
    items: posts,
    setItems,
    setCursor,
    hasMore,
    loading,
    loaderRef,
  } = useInfiniteScroll<Post>({
    loadMore: (cursor) => getThreadPosts(parsedThreadId!, cursor, 20),
  });

  useEffect(() => {
    if (!boardShortName || !threadId || Number.isNaN(parsedThreadId)) return;

    const loadThread = async () => {
      const response = await getThread(boardShortName, parsedThreadId);

      setThread(response.thread);
      setItems(response.thread.posts);
      setCursor(response.nextCursor?.toString() || null);
    };

    loadThread();
  }, [boardShortName, parsedThreadId]);

  if (!boardShortName || !threadId || Number.isNaN(parsedThreadId)) {
    return <div className="page-container loading">Invalid URL</div>;
  }

  if (!thread) {
    return <div className="page-container loading">Загрузка...</div>;
  }

  return (
    <main className="page-stack">
      <BoardDescription
        name={thread.board.name}
        description={thread.board.description}
      />

      <section className="page-container action-block fade-in-up delay-200ms">
        {!createPostVisible && (
          <ButtonVisibility onClick={() => setCreatePostVisible(true)}>
            Ответить в тред
          </ButtonVisibility>
        )}

        {createPostVisible && (
          <>
            <ButtonVisibility onClick={() => setCreatePostVisible(false)}>
              Закрыть форму постинга
            </ButtonVisibility>
            <CreatePostForm
              threadId={parsedThreadId}
              onCreated={(post) => {
                setItems((prev) => [...prev, post]);
                setCreatePostVisible(false);
              }}
            />
          </>
        )}
      </section>

      <section className="page-container thread-section fade-in-up delay-200ms">
        <ThreadPreview
          thread={thread}
          boardShortName={boardShortName}
          variant="page"
        />
        <PostList
          posts={posts}
          threadId={parsedThreadId}
          onReplyCreated={(newPost) => {
            setItems((prev) => [...prev, newPost]);
          }}
        />
      </section>

      {hasMore && <div ref={loaderRef} className="scroll-loader" />}
      {loading && <div className="page-container loading">Загрузка...</div>}
    </main>
  );
}
