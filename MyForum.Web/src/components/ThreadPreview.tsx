import PostFilesPreview from "./PostFilesPreview";
import ThreadCard from "./ThreadCard";
import type { Thread } from "../types/thread";

interface Props {
  thread: Thread;
  boardShortName: string;
}

const ThreadPreview = ({ thread, boardShortName }: Props) => {
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
      />

      <PostFilesPreview files={thread.originalPost.files} />

      <p>{thread.originalPost.content}</p>
    </>
  );
};

export default ThreadPreview;
