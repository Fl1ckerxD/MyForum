import type { BoardName } from "../types/boardName";

interface BoardListProps {
  boards: BoardName[];
}

const MainBoardList = ({ boards }: BoardListProps) => {
  return (
    <section className="categories-section fade-in-up delay-500ms">
      <div className="categories-grid">
        <div className="category-group staggered-fade-in">
          <h3 className="category-items">Доски</h3>
          <div className="category-items">
            <ul className="navbar-nav flex-grow-1">
              {boards.map((board) => (
                <li key={board.shortName}>
                  <a href={`/${board.shortName}`} className="category-item">
                    {board.name}
                  </a>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </section>
  );
};

export default MainBoardList;
