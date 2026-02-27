import { Paperclip, Type, UserRound, X } from "lucide-react";
import { useRef, useState } from "react";
import { createThread } from "../api/threads.api";

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

  const addFiles = (newFiles: File[]) => {
    setFiles((prev) => [
      ...prev,
      ...newFiles.filter(
        (file) =>
          !prev.some(
            (prevFile) => prevFile.name === file.name && prevFile.size === file.size,
          ),
      ),
    ]);
  };

  return (
    <div className="form-container">
      <form className="form-post" onSubmit={handleSubmit}>
        <div className="form-head">
          <h3 className="form-title">Новый тред</h3>
          <p className="form-subtitle">/{boardShortName}/ · публичная публикация</p>
        </div>

        <div className="form-group form-group-pad">
          <label className="form-label" htmlFor="thread-subject">
            <Type size={14} />
            Тема
          </label>
          <input
            id="thread-subject"
            type="text"
            className="input"
            placeholder="Название треда"
            value={subject}
            onChange={(event) => setSubject(event.target.value)}
            disabled={isSubmitting}
            required
          />
        </div>

        <div className="form-group form-group-pad">
          <label className="form-label" htmlFor="thread-content">
            Комментарий
          </label>
          <textarea
            id="thread-content"
            className="textarea"
            placeholder="Текст первого поста..."
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
            id="thread-author"
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
            {isSubmitting ? "Создание..." : "Создать тред"}
          </button>
        </div>
      </form>
    </div>
  );
}
