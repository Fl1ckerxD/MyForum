import http from "k6/http";
import { check, sleep } from "k6";
import { FormData } from "https://jslib.k6.io/formdata/0.0.2/index.js";

const BASE_URL = (__ENV.BASE_URL || "http://localhost:80").replace(/\/+$/, "");
const DEFAULT_BOARD_SHORT_NAME = __ENV.BOARD_SHORT_NAME || "b";

export const options = {
  // Базовая стартовая нагрузка: 10 виртуальных пользователей в течение 1 минуты.
  vus: 10,
  duration: "1m",
};

function postMultipart(url, fields) {
  // API принимает только multipart/form-data для POST /threads и POST /posts.
  const formData = new FormData();
  Object.entries(fields).forEach(([key, value]) => formData.append(key, value));

  return http.post(url, formData.body(), {
    headers: {
      "Content-Type": `multipart/form-data; boundary=${formData.boundary}`,
    },
  });
}

function chooseBoardShortName() {
  // Если доска задана через env, используем ее; иначе берем случайную из доступных.
  if (DEFAULT_BOARD_SHORT_NAME) {
    return DEFAULT_BOARD_SHORT_NAME;
  }

  const boardsRes = http.get(`${BASE_URL}/api/boards`);
  const boardsOk = check(boardsRes, {
    "GET /api/boards status is 200": (r) => r.status === 200,
  });

  if (!boardsOk) {
    return null;
  }

  const boards = boardsRes.json();
  if (!Array.isArray(boards) || boards.length === 0) {
    return null;
  }

  return boards[Math.floor(Math.random() * boards.length)].shortName;
}

export default function () {
  // Реалистичный пользовательский флоу:
  // 1) выбрать доску -> 2) создать тред -> 3) открыть тред -> 4) написать 5 постов.
  const boardShortName = chooseBoardShortName();
  if (!boardShortName) {
    return;
  }

  // Открытие доски, чтобы убедиться, что она существует и получить ее ID для создания треда.
  const boardRes = http.get(`${BASE_URL}/api/boards/${boardShortName}`);
  const boardOk = check(boardRes, {
    "GET /api/boards/{shortName} status is 200": (r) => r.status === 200,
  });

  if (!boardOk) {
    return;
  }

  const board = boardRes.json("board");
  if (!board || !board.id) {
    return;
  }

  // Создание треда. Уникальный суффикс в теме и контенте, чтобы не было коллизий между итерациями.
  const uniqueSuffix = `${__VU}-${__ITER}-${Date.now()}`;
  const createThreadRes = postMultipart(`${BASE_URL}/api/threads`, {
    BoardId: String(board.id),
    BoardShortName: board.shortName,
    Subject: `Load test thread ${uniqueSuffix}`,
    "OriginalPost.Content": `Original post content ${uniqueSuffix}`,
    "OriginalPost.AuthorName": `k6user${__VU}`,
  });

  const createThreadOk = check(createThreadRes, {
    "POST /api/threads status is 201": (r) => r.status === 201,
    "POST /api/threads has threadId": (r) => !!r.json("threadId"),
  });

  if (!createThreadOk) {
    return;
  }

  // Получаем ID созданного треда затем заходим в него, чтобы убедиться, что он доступен.
  const threadId = createThreadRes.json("threadId");

  const openThreadRes = http.get(
    `${BASE_URL}/api/threads/${board.shortName}/${threadId}`,
  );
  check(openThreadRes, {
    "GET /api/threads/{board}/{threadId} status is 200": (r) =>
      r.status === 200,
  });

  sleep(0.3);

  // Пишем 5 постов в тред с небольшими паузами между ними.
  for (let i = 1; i <= 5; i += 1) {
    const createPostRes = postMultipart(`${BASE_URL}/api/posts`, {
      ThreadId: String(threadId),
      Content: `Reply #${i} from VU ${__VU}, iter ${__ITER}`,
      AuthorName: `k6user${__VU}`,
      ReplyToPostId: "",
    });

    check(createPostRes, {
      "POST /api/posts status is 201": (r) => r.status === 201,
      "POST /api/posts has id": (r) => !!r.json("id"),
    });

    sleep(0.2);
  }
}
