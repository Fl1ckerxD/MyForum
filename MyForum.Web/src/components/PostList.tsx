import type { Post } from "../types/post";
import PostCard from "./PostCard";
import "../styles/layout/section.css";
import "../styles/ui/link.css";
import "../styles/ui/span.css";

interface Props {
  posts: Post[];
  threadId: number;
  onReplyCreated?: (newPost: Post) => void;
}

const PostList = ({ posts, threadId, onReplyCreated }: Props) => {
  return (
    <section className="thread-section fade-in-up delay-200ms">
      <ul className="navbar-nav flex-grow-1">
        {posts.map((post) => (
          <li id={`post-${post.id}`} className="section-bordered pt-3" key={post.id}>
            <PostCard post={post} threadId={threadId} onReplyCreated={onReplyCreated} />
          </li>
        ))}
      </ul>
    </section>
  );
};

export default PostList;
