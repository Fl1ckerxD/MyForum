import { Paperclip, UserRound, X } from "lucide-react";
import { useRef, useState } from "react";
import { createPost } from "../api/posts.api";
import type { Post } from "../types/post";

interface Props {
  threadId: number;
  replyToPostId?: number;
  onCreated?: (post: Post) => void;
  variant?: "thread" | "reply";
}

export default function CreatePostForm({
  threadId,
  replyToPostId,
  onCreated,
  variant = "thread",
}: Props) {
  const [content, setContent] = useState("");
  const [authorName, setAuthorName] = useState("Аноним");
  const [files, setFiles] = useState<File[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const maxFiles = 5;
  const maxFileSizeBytes = 10 * 1024 * 1024;
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDrop = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    addFiles(Array.from(event.dataTransfer.files));
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const createdPost = await createPost({
        threadId,
        content,
        authorName,
        files,
        replyToPostId,
      });

      setContent("");
      setFiles([]);
      onCreated?.(createdPost);
    } catch (err: any) {
      setError(err.message ?? "Ошибка при создании поста");
    } finally {
      setIsSubmitting(false);
    }
  };

  const addFiles = (newFiles: File[]) => {
    setFiles((prev) => {
      const uniqueFiles = newFiles.filter(
        (file) =>
          !prev.some(
            (prevFile) => prevFile.name === file.name && prevFile.size === file.size,
          ),
      );

      const oversizedFiles = uniqueFiles.filter(
        (file) => file.size > maxFileSizeBytes,
      );

      const validFiles = uniqueFiles.filter(
        (file) => file.size <= maxFileSizeBytes,
      );

      const availableSlots = Math.max(maxFiles - prev.length, 0);
      const filesToAdd = validFiles.slice(0, availableSlots);

      if (oversizedFiles.length > 0) {
        setError("Размер каждого файла не может превышать 10 МБ.");
      } else if (validFiles.length > availableSlots) {
        setError("Нельзя прикреплять больше 5 файлов к одному посту.");
      } else {
        setError(null);
      }

      return [...prev, ...filesToAdd];
    });
  };

  return (
    <div className="form-container">
      <form className="form-post" onSubmit={handleSubmit}>
        <div className="form-group form-group-pad">
          <label className="form-label" htmlFor="post-content">
            Комментарий
          </label>
          <textarea
            id="post-content"
            className={`textarea ${variant === "reply" ? "reply" : ""}`}
            placeholder="Текст вашего комментария..."
            value={content}
            onChange={(event) => setContent(event.target.value)}
            disabled={isSubmitting}
            maxLength={1500}
            required
          />
          <p className="form-note">Осталось символов: {1500 - content.length}</p>
        </div>

        <div className="form-group form-group-pad">
          <label className="form-label" htmlFor="thread-author">
            <UserRound size={14} />
            Имя
          </label>
          <input
            id="post-author"
            type="text"
            className="input"
            placeholder="Имя (необязательно)"
            value={authorName}
            onChange={(event) => setAuthorName(event.target.value)}
          />
        </div>

        <div className="form-group form-group-pad">
          <div
            className="dropzone"
            onClick={() => fileInputRef.current?.click()}
            onDrop={handleDrop}
            onDragOver={(event) => event.preventDefault()}
          >
            <input
              ref={fileInputRef}
              className="input input-file"
              type="file"
              multiple
              onChange={(event) => {
                if (event.target.files) {
                  addFiles(Array.from(event.target.files));
                  event.target.value = "";
                }
              }}
            />
            <Paperclip size={14} />
            Добавить файл
          </div>
        </div>

        {files.length > 0 && (
          <ul className="file-list">
            {files.map((file, index) => (
              <li key={index} className="file-item">
                <span>{file.name}</span>
                <button
                  className="mf-btn mf-btn-sm d-flex align-center"
                  type="button"
                  onClick={() =>
                    setFiles((prev) =>
                      prev.filter((_, fileIndex) => fileIndex !== index),
                    )
                  }
                >
                  <X size={14} />
                </button>
              </li>
            ))}
          </ul>
        )}

        {error && <div className="error-message">{error}</div>}

        <div className="form-footer">
          <button
            type="submit"
            className="mf-btn mf-btn-full"
            disabled={isSubmitting}
          >
            {isSubmitting ? "Создание..." : variant === "reply" ? "Ответить" : "Создать"}
          </button>
        </div>
      </form>
    </div>
  );
}
