import { LayoutGrid } from "lucide-react";
import { Link } from "react-router-dom";
import type { BoardName } from "../types/boardName";

interface BoardListProps {
  boards: BoardName[];
}

const MainBoardList = ({ boards }: BoardListProps) => {
  return (
    <section className="page-container page-container-medium fade-in-up delay-500ms">
      <div className="section-panel">
        <h3 className="board-title">
          <LayoutGrid size={16} />
          Доски
        </h3>
        <ul className="stack-list">
          {boards.map((board) => (
            <li key={board.shortName}>
              <Link to={`/${board.shortName}`} className="link link-board">
                /{board.shortName}/ - {board.name}
              </Link>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
};

export default MainBoardList;
