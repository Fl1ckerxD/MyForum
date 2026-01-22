import { Link } from "react-router-dom";
import "../styles/ui/text.css";

interface ThreadCardProps {
  id: number;
  boardShortName: string;
  subject: string;
  postId: number;
  author: string;
  createdAt: Date;
  postCount: number;
  fileCount: number;
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
  variant = "list",
}: ThreadCardProps) => {
  return (
    <div className="d-flex align-items-center gap-2">
      {variant === "page" ? (
        <>
          <span className="title">{subject}</span>
          <strong className="muted">{author}</strong>
        </>
      ) : (
        <>
          <Link className="link" to={`/${boardShortName}/${id}`}>
            {subject}
          </Link>
          <strong>{author}</strong>
        </>
      )}

      <span className="muted">{createdAt.toLocaleDateString()}</span>

      <span className="muted">№{postId}</span>

      <span className="muted">
        {postCount} постов / {fileCount} файлов
      </span>
    </div>
  );
};

export default ThreadCard;
