import { useParams } from "react-router-dom";
import BoardDescription from "../components/BoardDescription";
import ButtonVisibility from "../components/ButtonVisibility";
import { useEffect, useState } from "react";
import type { Thread } from "../types/thread";
import ThreadPreview from "../components/ThreadPreview";
import { getThread } from "../api/threads.api";
import PostList from "../components/PostList";
import CreatePostForm from "../components/CreatePostForm";
import type { Post } from "../types/post";

export default function ThreadPage() {
  const { boardShortName, threadId } = useParams<{
    boardShortName: string;
    threadId: string;
  }>();
  const parsedThreadId = Number(threadId);
  const [thread, setThread] = useState<Thread | null>(null);
  const [createPostVisible, setCreatePostVisible] = useState(false);
  const [posts, setPosts] = useState<Post[]>([]);

  useEffect(() => {
    if (!boardShortName || !threadId || Number.isNaN(parsedThreadId)) return;

    const loadThread = async () => {
      const thread = await getThread(boardShortName, parsedThreadId);
      setThread(thread);
      setPosts(thread.posts);
    };

    loadThread();
  }, [boardShortName, parsedThreadId]);

  if (!boardShortName || !threadId || Number.isNaN(parsedThreadId)) {
    return <div>Invalid URL</div>;
  }

  if (!thread) {
    return <div>Loading...</div>;
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
              setPosts(prev => [...prev, post]);
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
        <PostList posts={posts} />
      </section>
    </>
  );
}
