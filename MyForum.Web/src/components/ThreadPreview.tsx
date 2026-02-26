import PostFilesPreview from "./PostFilesPreview";
import ThreadCard from "./ThreadCard";
import type { Thread } from "../types/thread";
import PostList from "./PostList";

interface Props {
  thread: Thread;
  boardShortName: string;
  variant?: "list" | "page";
}

const ThreadPreview = ({ thread, boardShortName, variant = "list" }: Props) => {
  return (
    <div id={`post-${thread.originalPost.id}`}>
      {thread.isPinned && (<p>Закрепленно</p>)}

      <ThreadCard
        id={thread.id}
        boardShortName={boardShortName}
        subject={thread.subject}
        postId={thread.originalPost.id}
        author={thread.originalPost.authorName}
        createdAt={thread.createdAt}
        postCount={thread.postCount}
        fileCount={thread.fileCount}
        variant={variant}
      />

      <PostFilesPreview files={thread.originalPost.files} variant={variant} />

      <p>{thread.originalPost.content}</p>

      <PostList posts={thread.posts} threadId={thread.id} />
    </div>
  );
};

export default ThreadPreview;
