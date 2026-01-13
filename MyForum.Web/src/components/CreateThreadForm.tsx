import { useState } from "react";
import { createThread } from "../api/threads.api";
import "../styles/layout/form.css";
import "../styles/ui/button.css";
import "../styles/ui/input.css";

interface CreateThreadFormProps {
  boardId: number;
  boardShortName: string;
  onCreated?: (threadId: number) => void;
}

export default function CreateThreadForm({
  boardId,
  boardShortName,
  onCreated,
}: CreateThreadFormProps) {
  const [subject, setSubject] = useState("");
  const [content, setContent] = useState("");
  const [authorName, setAuthorName] = useState("Аноним");
  const [files, setFiles] = useState<File[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const response = await createThread({
        boardId,
        boardShortName,
        subject,
        content,
        authorName,
        files,
      });

      setSubject("");
      setContent("");
      setFiles([]);
      onCreated?.(response.threadId);
    } catch (err: any) {
      setError(err.message ?? "Ошибка при создании треда");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <>
      <div className="form-container">
        <form className="form-post" onSubmit={handleSubmit}>
          <div className="form-header form-group">
            <input
              type="text"
              className="input"
              placeholder="Название"
              value={subject}
              onChange={(e) => setSubject(e.target.value)}
              disabled={isSubmitting}
              required
            />
          </div>

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
            <input
              className="input"
              type="file"
              multiple
              onChange={(e) =>
                setFiles(e.target.files ? Array.from(e.target.files) : [])
              }
            />
          </div>

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
