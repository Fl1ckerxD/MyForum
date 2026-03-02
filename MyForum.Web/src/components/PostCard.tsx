import { CornerUpRight, Hash, MessageSquareReply } from "lucide-react";
import { useState } from "react";
import type { Post } from "../types/post";
import CreatePostForm from "./CreatePostForm";
import PostFilesPreview from "./PostFilesPreview";

interface Props {
  post: Post;
  threadId: number;
  onReplyCreated?: (post: Post) => void;
}

const PostCard = ({ post, threadId, onReplyCreated }: Props) => {
  const [replyVisible, setReplyVisible] = useState(false);

  return (
    <article className="post-card">
      <div className="post-meta">
        <strong>{post.authorName}</strong>
        <span className="muted">{new Date(post.createdAt).toLocaleString()}</span>
        <span className="muted post-id">
          <Hash size={13} />
          {post.id}
        </span>
      </div>

      {post.replyToPostId && (
        <button
          className="mf-btn-link"
          onClick={() => scrollToPost(post.replyToPostId!)}
        >
          <CornerUpRight size={14} />
          {post.replyToPostId}
        </button>
      )}

      <p className="post-content">{post.content}</p>

      <PostFilesPreview files={post.files} />

      <button
        className="mf-btn-link mf-btn-reg"
        onClick={() => setReplyVisible((visible) => !visible)}
      >
        <MessageSquareReply size={14} />
        Ответить
      </button>

      {replyVisible && (
        <div className="reply-form">
          <CreatePostForm
            threadId={threadId}
            replyToPostId={post.id}
            variant="reply"
            onCreated={(newPost) => {
              onReplyCreated?.(newPost);
              setReplyVisible(false);
            }}
          />
        </div>
      )}
    </article>
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
};

export default PostCard;
