import styles from "./MainHeader.module.css";

const MainHeader = () => {
  return (
    <section>
      <div className="d-flex flex-column align-items-center">
        <h1 className={styles.logoText}>Myforum</h1>
        <p className={styles.welcomeText}>Добро пожаловать.</p>
      </div>
    </section>
  );
};

export default MainHeader;
