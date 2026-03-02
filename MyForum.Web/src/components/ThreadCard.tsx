import { CalendarDays, Hash, MessageSquare, Paperclip, Pin, Lock } from "lucide-react";
import { Link } from "react-router-dom";

interface ThreadCardProps {
  id: number;
  boardShortName: string;
  subject: string;
  postId: number;
  author: string;
  createdAt: Date;
  postCount: number;
  fileCount: number;
  isPinned?: boolean;
  isLocked?: boolean;
  variant?: "list" | "page";
}

const ThreadCard = ({
  id,
  boardShortName,
  subject,
  postId,
  author,
  createdAt,
  postCount,
  fileCount,
  isPinned = false,
  isLocked = false,
  variant = "list",
}: ThreadCardProps) => {
  return (
    <div className="thread-card-head">
      {isPinned && (
        <Pin
          size={14}
          className="thread-pin-icon"
          aria-label="Закреплено"
        />
      )}

      {isLocked && (
        <Lock
          size={14}
          aria-label="Закрыто"
        />
      )}

      {variant === "page" ? (
        <>
          <span className="title">{subject}</span>
          <strong className="muted">{author}</strong>
        </>
      ) : (
        <>
          <Link className="link thread-link" to={`/${boardShortName}/${id}`}>
            {subject}
          </Link>
          <strong>{author}</strong>
        </>
      )}

      <span className="muted thread-meta">
        <CalendarDays size={14} />
        {createdAt.toLocaleDateString()}
      </span>

      <span className="muted thread-meta">
        <Hash size={14} />
        {postId}
      </span>

      <span className="muted thread-meta">
        <MessageSquare size={14} />
        {postCount}
        <Paperclip size={14} />
        {fileCount}
      </span>
    </div>
  );
};

export default ThreadCard;
