import { useState } from "react";
import { useAuth } from "./AuthContext";
import { useNavigate } from "react-router-dom";
import { Icon } from "../../components/Icon";
import "./LoginPage.css";

export const LoginPage = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [errors, setErrors] = useState<{
    username?: string;
    password?: string;
    general?: string;
  }>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    const result = await login(username, password);

    if (!result) {
      navigate("/boards");
    } else {
      setErrors(result);
      setIsSubmitting(false);
    }
  };

  return (
    <section className="login-page">
      <div className="login-page__shell">
        <aside className="login-page__promo">
          <span className="login-page__badge">
            <Icon name="shield" size={15} />
            Защищенный вход
          </span>
          <h1 className="login-page__promo-title">Панель управления MyForum</h1>
          <p className="login-page__promo-text">
            Управляйте досками, модерируйте контент, баньте нарушителей и отслеживайте состояние площадки в одном месте.
          </p>
        </aside>

        <form className="login-page__form" onSubmit={handleSubmit}>
          <div>
            <h2 className="login-page__title">Вход администратора</h2>
            <p className="login-page__subtitle">Используйте учетные данные администратора форума</p>
          </div>

          <div className="login-page__field">
            <label className="login-page__label" htmlFor="username">Логин</label>
            <input
              id="username"
              className="admin-field"
              placeholder="Введите логин"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              autoComplete="username"
            />
            {errors.username && <p className="login-page__error">{errors.username}</p>}
          </div>

          <div className="login-page__field">
            <label className="login-page__label" htmlFor="password">Пароль</label>
            <input
              id="password"
              type="password"
              className="admin-field"
              placeholder="Введите пароль"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
            />
            {errors.password && <p className="login-page__error">{errors.password}</p>}
          </div>

          {errors.general && <p className="login-page__error">{errors.general}</p>}

          <button type="submit" className="admin-btn admin-btn-primary login-page__submit" disabled={isSubmitting}>
            <Icon name="shield" size={16} />
            {isSubmitting ? "Проверка..." : "Войти"}
          </button>
        </form>
      </div>
    </section>
  );
};
