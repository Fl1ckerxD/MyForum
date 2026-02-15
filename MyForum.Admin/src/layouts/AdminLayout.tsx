import { Link, Outlet } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";

export const AdminLayout = () => {
    const { logout } = useAuth();

    return (
        <div style={{ display: "flex", minHeight: "100vh" }}>
            <aside style={{ width: 220, padding: 20, borderRight: "1px solid #ddd" }}>
                <h3>Admin</h3>
                <nav style={{ display: "flex", flexDirection: "column", gap: 10 }}>
                    <Link to="/boards">Boards</Link>
                    <Link to="/threads">Threads</Link>
                    <Link to="/posts">Posts</Link>
                    <Link to="/bans">Bans</Link>
                </nav>
                <button onClick={logout} style={{ marginTop: 20 }}>
                    Logout
                </button>
            </aside>

            <main style={{ flex: 1, padding: 20 }}>
                <Outlet />
            </main>
        </div>
    );
};
