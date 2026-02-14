import { BrowserRouter, Routes, Route } from "react-router-dom";
import { LoginPage } from "./features/auth/LoginPage";
import { AdminLayout } from "./layouts/AdminLayout";
import './App.css'

function App() {
  return (
    <>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />

          <Route path="/admin" element={<AdminLayout />}>
            <Route path="/boards" element={<div>Boards page</div>} />
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
