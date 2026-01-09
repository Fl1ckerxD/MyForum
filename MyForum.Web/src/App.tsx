import { BrowserRouter, Routes, Route } from "react-router-dom";
import HomePage from "./pages/HomePage";
import ThreadPage from "./pages/ThreadPage";
import BoardPage from "./pages/BoardPage";

const App = () => {
  return (
    <>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/:boardShortName" element={<BoardPage />} />
          <Route path="/:boardShortName/:threadId" element={<ThreadPage />} />
        </Routes>
      </BrowserRouter>
    </>
  );
};

export default App;
