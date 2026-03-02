import type { Thread } from "../types/thread";
import PostFilesPreview from "./PostFilesPreview";
import ThreadCard from "./ThreadCard";

interface Props {
  thread: Thread;
  boardShortName: string;
  variant?: "list" | "page";
}

const ThreadPreview = ({ thread, boardShortName, variant = "list" }: Props) => {
  return (
    <article id={`post-${thread.originalPost.id}`} className="thread-preview">
      <ThreadCard
        id={thread.id}
        boardShortName={boardShortName}
        subject={thread.subject}
        postId={thread.originalPost.id}
        author={thread.originalPost.authorName}
        createdAt={thread.createdAt}
        postCount={thread.postCount}
        fileCount={thread.fileCount}
        isPinned={thread.isPinned}
        isLocked={thread.isLocked}
        variant={variant}
      />

      <PostFilesPreview files={thread.originalPost.files} variant={variant} />

      <p className="thread-content">{thread.originalPost.content}</p>
    </article>
  );
};

export default ThreadPreview;
