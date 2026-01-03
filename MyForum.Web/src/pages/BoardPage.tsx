import { useParams } from "react-router-dom";

export default function BoardPage() {
  const { boardShortName } = useParams<{ boardShortName: string }>();

  return (
    <div>
      <h1>/{boardShortName}/</h1>
      {/* загрузка тредов */}
    </div>
  );
}
