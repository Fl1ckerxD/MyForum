import type { Thread } from "../types/thread";

interface ThreadListProps {
  threads: Thread[];
  onDelete: (threadId: number) => void;
  onLike: (threadId: number) => void;
}

const ThreadList = ({ threads, onDelete, onLike }: ThreadListProps) => {
  return (
    <section className="topic-section fade-in-up">
      <div className="table-bordered">
        <ul className="navbar-nav flex-grow-1">
          {threads.map((thread) => (
            <li key={thread.id}>
              <strong>{thread.user.username}</strong>
              <label className="welcome-text topic-time">
                {thread.createdAt}
              </label>
              <button
                type="button"
                className="button-transparent"
                onClick={() => onDelete(thread.id)}
              >
                <svg
                  className="delete-button-circle"
                  width="24"
                  height="24"
                  viewBox="0 0 24 24"
                >
                  <circle cx="12" cy="12" r="10" />
                  <path
                    d="M15 9L9 15M9 9L15 15"
                    stroke="#ffff"
                    strokeWidth="2"
                    strokeLinecap="round"
                  />
                </svg>
              </button>
              <p>{thread.content}</p>
              <button
                type="button"
                className="like-button submit-button"
                onClick={() => onLike(thread.id)}
              >
                Понравилось {thread.likes.length}
              </button>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
};

export default ThreadList;
