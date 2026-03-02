import type { Post } from "../types/post";
import PostCard from "./PostCard";

interface Props {
  posts: Post[];
  threadId: number;
  onReplyCreated?: (newPost: Post) => void;
}

const PostList = ({ posts, threadId, onReplyCreated }: Props) => {
  return (
    <section className="thread-section">
      <ul className="stack-list post-list">
        {posts.map((post) => (
          <li id={`post-${post.id}`} className="section-bordered post-item" key={post.id}>
            <PostCard post={post} threadId={threadId} onReplyCreated={onReplyCreated} />
          </li>
        ))}
      </ul>
    </section>
  );
};

export default PostList;
