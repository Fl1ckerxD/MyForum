import { Link } from "react-router-dom";
import type { Thread } from "../types/thread";
import "../styles/layout/section.css";
import "../styles/ui/link.css";
import "../styles/ui/span.css";

interface ThreadListProps {
  boardShortName: string;
  threads: Thread[];
}

const ThreadList = ({ boardShortName, threads }: ThreadListProps) => {
  return (
    <section className="thread-section fade-in-up delay-200ms">
      <ul className="navbar-nav flex-grow-1">
        {threads.map((thread) => (
          <li className="section-bordered pt-3" key={thread.id}>
            <div className="d-flex align-items-center gap-3">
              <Link className="link" to={`/${boardShortName}/${thread.id}`}>
                {thread.subject}
              </Link>
              <strong>{thread.originalPost.authorName}</strong>
              <span className="muted align-self-center">
                {thread.createdAt.toLocaleDateString()}
              </span>
            </div>
            <div>
              <p>{thread.originalPost.content}</p>
            </div>
          </li>
        ))}
      </ul>
    </section>
  );
};

export default ThreadList;
