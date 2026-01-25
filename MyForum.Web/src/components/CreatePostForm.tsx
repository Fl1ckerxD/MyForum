import { useState, useRef } from "react";
import { createPost } from "../api/posts.api";
import "../styles/layout/form.css";
import "../styles/ui/button.css";
import "../styles/ui/input.css";
import "../styles/ui/file.css";
import type { Post } from "../types/post";

interface Props {
  threadId: number;
  onCreated?: (post: Post) => void;
}

export default function CreatePostForm({ threadId, onCreated }: Props) {
  const [content, setContent] = useState("");
  const [authorName, setAuthorName] = useState("Аноним");
  const [files, setFiles] = useState<File[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    addFiles(Array.from(e.dataTransfer.files))
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const createdPost = await createPost({
        threadId,
        content,
        authorName,
        files,
      });

      setContent("");
      setFiles([]);
      onCreated?.(createdPost);
    } catch (err: any) {
      setError(err.message ?? "Ошибка при создании треда");
    } finally {
      setIsSubmitting(false);
    }
  };

  const addFiles = (newFiles: File[]) => {
    setFiles(prev => [
      ...prev,
      ...newFiles.filter(
        f => !prev.some(p => p.name === f.name && p.size === f.size)
      )
    ]);
  };

  return (
    <>
      <div className="form-container">
        <form className="form-post" onSubmit={handleSubmit}>
          <div className="form-group">
            <textarea
              className="textarea"
              placeholder="Комментарий. Макс. длина 15000"
              value={content}
              onChange={(e) => setContent(e.target.value)}
              disabled={isSubmitting}
              maxLength={15000}
              required
            />
          </div>

          <div className="form-group">
            <input
              type="text"
              className="input"
              placeholder="Имя (необязательно)"
              value={authorName}
              onChange={(e) => setAuthorName(e.target.value)}
            />
          </div>

          <div className="form-group">
            <div
              className="dropzone"
              onClick={() => fileInputRef.current?.click()}
              onDrop={handleDrop}
              onDragOver={(e) => e.preventDefault()}
            >
              <input
                ref={fileInputRef}
                className="input input-file"
                type="file"
                multiple
                onChange={(e) => {
                  if (e.target.files) {
                    addFiles(Array.from(e.target.files));
                    e.target.value = "";
                  }
                }}
              />
              Добавить файл
            </div>
          </div>

          <ul className="file-list">
            {files.map((file, index) => (
              <li key={index} className="file-item">
                <span>{file.name}</span>
                <button
                  className="mf-btn"
                  type="button"
                  onClick={() =>
                    setFiles(prev => prev.filter((_, i) => i !== index))
                  }
                >
                  ✕
                </button>
              </li>
            ))}
          </ul>

          {error && <div className="error-message">{error}</div>}

          <div className="form-footer">
            <button
              type="submit"
              className="mf-btn mf-btn-full"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Создание..." : "Создать"}
            </button>
          </div>
        </form>
      </div>
    </>
  );
}