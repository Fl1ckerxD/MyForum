import { Link } from "react-router-dom";
import type { BoardName } from "../types/boardName";
import "../styles/ui/link.css";

interface BoardListProps {
  boards: BoardName[];
}

const MainBoardList = ({ boards }: BoardListProps) => {
  return (
    <section className="fade-in-up delay-500ms">
      <div className="container mb-5">
        <h3 className="board-title">Доски</h3>
        <ul className="navbar-nav flex-grow-1">
          {boards.map((board) => (
            <li key={board.shortName}>
              <Link to={`/${board.shortName}`} className="link">
                {board.name}
              </Link>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
};

export default MainBoardList;
