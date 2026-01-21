import type { Thread } from "../types/thread";
import "../styles/layout/section.css";
import "../styles/ui/link.css";
import "../styles/ui/span.css";
import ThreadPreview from "./ThreadPreview";

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
            <ThreadPreview thread={thread} boardShortName={boardShortName} />
          </li>
        ))}
      </ul>
    </section>
  );
};

export default ThreadList;
