import { useParams } from "react-router-dom";
import BoardDescription from "../components/BoardDescription";
import ButtonVisibility from "../components/ButtonVisibility";
import { useEffect, useState } from "react";
import type { Thread } from "../types/thread";
import ThreadPreview from "../components/ThreadPreview";
import { getThread, getThreadPosts } from "../api/threads.api";
import PostList from "../components/PostList";
import CreatePostForm from "../components/CreatePostForm";
import type { Post } from "../types/post";
import { useInfiniteScroll } from "../hooks/useInfiniteScroll";

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
    loadMore: cursor => getThreadPosts(parsedThreadId!, cursor, 20),
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
    return <div>Invalid URL</div>;
  }

  if (!thread) {
    return <div>Загрузка...</div>;
  }

  return (
    <>
      <BoardDescription
        name={thread.board.name}
        description={thread.board.description}
      />
      <section className="fade-in-up delay-200ms d-flex align-items-center flex-column">
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
            <CreatePostForm threadId={parsedThreadId} onCreated={post => {
              setItems(prev => [...prev, post]);
              setCreatePostVisible(false);
            }} />
          </>
        )}
      </section>
      <section className="thread-section fade-in-up delay-200ms">
        <ThreadPreview
          thread={thread}
          boardShortName={boardShortName}
          variant="page"
        />
        <PostList posts={posts} threadId={parsedThreadId} onReplyCreated={newPost => {
          setItems(prev => [...prev, newPost])
        }} />
        {hasMore && (
          <div ref={loaderRef} style={{ height: 40 }} />
        )}

        {loading && <div>Загрузка...</div>}
      </section>
    </>
  );
}
