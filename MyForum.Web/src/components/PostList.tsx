import type { Post } from "../types/post";
import PostCard from "./PostCard";
import "../styles/layout/section.css";
import "../styles/ui/link.css";
import "../styles/ui/span.css";

interface Props {
  posts: Post[];
}

const PostList = ({ posts }: Props) => {
  return (
    <section className="thread-section fade-in-up delay-200ms">
      <ul className="navbar-nav flex-grow-1">
        {posts.map((post) => (
          <li className="section-bordered pt-3" key={post.id}>
            <PostCard post={post} />
          </li>
        ))}
      </ul>
    </section>
  );
};

export default PostList;
