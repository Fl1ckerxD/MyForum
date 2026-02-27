import styles from "./MainHeader.module.css";

const MainHeader = () => {
  return (
    <section className="page-container fade-in">
      <div className={styles.headerInner}>
        <h1 className={styles.logoText}>MyForum</h1>
        <p className={styles.welcomeText}>Добро пожаловать.</p>
      </div>
    </section>
  );
};

export default MainHeader;
