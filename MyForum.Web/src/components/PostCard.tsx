import type { Post } from "../types/post";

interface Props {
  post: Post;
}

const PostCard = ({ post }: Props) => {
  return (
    <div>
      <div className="d-flex align-items-center gap-2">
        <strong>{post.authorName}</strong>
        <span className="muted">
          {new Date(post.createdAt).toLocaleString()}
        </span>
      </div>
      <p>{post.content}</p>
    </div>
  );
};

export default PostCard;
