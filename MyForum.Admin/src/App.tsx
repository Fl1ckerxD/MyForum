import { BrowserRouter, Routes, Route } from "react-router-dom";
import { LoginPage } from "./features/auth/LoginPage";
import { BoardsPage } from "./features/boards/BoardsPage";
import { AdminLayout } from "./layouts/AdminLayout";
import { ThreadsPage } from "./features/threads/ThreadPage";
import { ProtectedRoute } from "./features/auth/ProtectedRoute";
import "./styles/admin-theme.css";
import "./App.css";
import { PostsPage } from "./features/posts/PostsPage";
import { BansPage } from "./features/bans/BansPage";

function App() {
  return (
    <BrowserRouter basename="/admin">
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route element={<ProtectedRoute />}>
          <Route element={<AdminLayout />}>
            <Route path="/boards" element={<BoardsPage />} />
            <Route path="/threads" element={<ThreadsPage />} />
            <Route path="/threads/:threadId/posts" element={<PostsPage />} />
            <Route path="/bans" element={<BansPage />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
