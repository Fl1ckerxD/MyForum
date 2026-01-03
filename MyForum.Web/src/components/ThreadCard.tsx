interface ThreadCardProps {
  title: string;
  author: string;
  createdAt: string;
  onClick: () => void;
}

const ThreadCard = ({ title, author, createdAt, onClick }: ThreadCardProps) => {
  return (
    <div className="thread-card" onClick={onClick}>
      <h2 className="thread-title">{title}</h2>
      <p className="thread-author">Posted by: {author}</p>
      <p className="thread-date">{new Date(createdAt).toLocaleDateString()}</p>
    </div>
  );
};

export default ThreadCard;
