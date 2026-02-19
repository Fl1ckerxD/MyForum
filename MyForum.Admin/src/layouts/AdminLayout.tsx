import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import { Icon } from "../components/ui/Icon";
import "./AdminLayout.css";

const navItems = [
  { to: "/boards", label: "Доски", icon: "boards" as const },
  { to: "/threads", label: "Треды", icon: "threads" as const },
  { to: "/bans", label: "Баны", icon: "bans" as const },
];

export const AdminLayout = () => {
  const { logout } = useAuth();

  return (
    <div className="admin-layout">
      <aside className="admin-layout__sidebar">
        <div className="admin-layout__brand">
          <span className="admin-layout__logo">
            <Icon name="dashboard" size={18} />
          </span>
          <div>
            <div className="admin-layout__title">MyForum Admin</div>
            <div className="admin-layout__subtitle">Управление платформой</div>
          </div>
        </div>

        <nav className="admin-layout__nav">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} className="admin-layout__nav-link">
              <Icon name={item.icon} size={16} />
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="admin-layout__grow" />

        <button type="button" className="admin-btn admin-layout__logout" onClick={logout}>
          <Icon name="logout" size={16} />
          Выйти
        </button>
      </aside>

      <main className="admin-layout__main">
        <Outlet />
      </main>
    </div>
  );
};
