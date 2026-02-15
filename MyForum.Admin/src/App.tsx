import { BrowserRouter, Routes, Route } from "react-router-dom";
import { LoginPage } from "./features/auth/LoginPage";
import { BoardsPage } from "./features/boards/BoardsPage";
import { ProtectedRoute } from "./features/auth/ProtectedRoute";
import './App.css'

function App() {
  return (
    <>
      <BrowserRouter basename="/admin">
        <Routes>
          <Route path="/login" element={<LoginPage />}></Route>

          <Route element={<ProtectedRoute />}>
            <Route path="/boards" element={<BoardsPage />} />
            <Route path="/threads" element={<div>Boards page</div>} />
            <Route path="/posts" element={<div>Boards page</div>} />
            <Route path="/bans" element={<div>Boards page</div>} />
          </Route>
        </Routes>
      </BrowserRouter>
    </>
  )
}

export default App
