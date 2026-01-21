import { Link } from "react-router-dom";

interface ThreadCardProps {
  id: number;
  boardShortName: string;
  subject: string;
  author: string;
  createdAt: Date;
  postCount: number;
  fileCount: number;
}

const ThreadCard = ({
  id,
  boardShortName,
  subject,
  author,
  createdAt,
  postCount,
  fileCount,
}: ThreadCardProps) => {
  return (
    <div className="d-flex align-items-center gap-3">
      <Link className="link" to={`/${boardShortName}/${id}`}>
        {subject}
      </Link>

      <strong>{author}</strong>

      <span className="muted align-self-center">
        {createdAt.toLocaleDateString()}
      </span>

      <span className="muted align-self-center">
        {postCount} постов / {fileCount} файлов
      </span>
    </div>
  );
};

export default ThreadCard;
