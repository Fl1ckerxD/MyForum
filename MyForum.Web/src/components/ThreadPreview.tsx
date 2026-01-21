import PostFilesPreview from "./PostFilesPreview";
import ThreadCard from "./ThreadCard";
import type { Thread } from "../types/thread";

interface Props {
  thread: Thread;
  boardShortName: string;
  variant?: "list" | "page";
}

const ThreadPreview = ({ thread, boardShortName, variant = "list" }: Props) => {
  return (
    <>
      <ThreadCard
        id={thread.id}
        boardShortName={boardShortName}
        subject={thread.subject}
        author={thread.originalPost.authorName}
        createdAt={thread.createdAt}
        postCount={thread.postCount}
        fileCount={thread.fileCount}
        variant={variant}
      />

      <PostFilesPreview files={thread.originalPost.files} variant={variant} />

      <p>{thread.originalPost.content}</p>
    </>
  );
};

export default ThreadPreview;
