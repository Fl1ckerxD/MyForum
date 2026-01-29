import { useState } from "react";
import type { Post } from "../types/post";
import CreatePostForm from "./CreatePostForm";

interface Props {
  post: Post;
  threadId: number;
  onReplyCreated?: (post: Post) => void;
}

const PostCard = ({ post, threadId, onReplyCreated }: Props) => {
  const [replyVisible, setReplyVisible] = useState(false);

  return (
    <div>
      <div className="d-flex align-items-center gap-2">
        <strong>{post.authorName}</strong>
        <span className="muted">
          {new Date(post.createdAt).toLocaleString()}
        </span>
        <span className="muted">№{post.id}</span>
      </div>

      {post.replyToPostId && (
        <button
          className="mf-btn-link fw-normal"
          onClick={() => scrollToPost(post.replyToPostId!)}
        >
          &gt;&gt;{post.replyToPostId}
        </button>
      )}

      <p>{post.content}</p>

      <button
        className="mf-btn-link mf-btn-reg"
        onClick={() => setReplyVisible(v => !v)}
      >
        Ответить
      </button>

      {replyVisible && (
        <div className="reply-form">
          <CreatePostForm
            threadId={threadId}
            replyToPostId={post.id}
            variant="reply"
            onCreated={newPost => {
              onReplyCreated?.(newPost);
              setReplyVisible(false);
            }}
          />
        </div>
      )}
    </div>
  );
};

const scrollToPost = (postId: number) => {
  const element = document.getElementById(`post-${postId}`);

  if (!element) return;

  element.scrollIntoView({
    behavior: "smooth",
    block: "center",
  });

  element.classList.add("post-highlight");

  setTimeout(() => {
    element.classList.remove("post-highlight");
  }, 1500);
}

export default PostCard;
