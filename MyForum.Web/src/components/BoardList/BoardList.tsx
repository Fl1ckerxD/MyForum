import { LayoutGrid } from "lucide-react";
import { Link } from "react-router-dom";
import type { BoardName } from "../../types/boardName";
import styles from "./BoardList.module.css";

interface Props {
  boards: BoardName[];
}

export default function BoardList({ boards }: Props) {
  return (
    <nav className={styles.boardList}>
      <h3 className={styles.boardTitle}>
        <LayoutGrid size={16} />
        Доски
      </h3>

      <ul className={styles.boardItems}>
        {boards.map((board) => (
          <li key={board.shortName} className={styles.boardItem}>
            <Link to={`/${board.shortName}`} className={`${styles.boardLink} link`}>
              /{board.shortName}/ - {board.name}
            </Link>
          </li>
        ))}
      </ul>
    </nav>
  );
}
