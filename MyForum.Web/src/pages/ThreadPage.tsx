import { useParams } from "react-router-dom";

export default function ThreadPage() {
  const { boardShortName, threadId } = useParams<{
    boardShortName: string;
    threadId: string;
  }>();

  return (
    <div>
      <h1>
        /{boardShortName}/{threadId}
      </h1>
      {/* загрузка постов */}
    </div>
  );
}
