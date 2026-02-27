import type { Thread } from "../types/thread";
import ThreadPreview from "./ThreadPreview";
import PostList from "./PostList";

interface ThreadListProps {
  boardShortName: string;
  threads: Thread[];
}

const ThreadList = ({ boardShortName, threads }: ThreadListProps) => {
  return (
    <section className="page-container thread-section fade-in-up delay-200ms">
      <ul className="stack-list thread-list">
        {threads.map((thread) => (
          <li className="section-bordered thread-item" key={thread.id}>
            <ThreadPreview thread={thread} boardShortName={boardShortName} />
            {thread.posts.length > 0 && (<PostList posts={thread.posts} threadId={thread.id} />)}
          </li>
        ))}
      </ul>
    </section>
  );
};

export default ThreadList;
