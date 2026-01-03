import React, { useState } from "react";

interface CommentFormProps {
  postId: number;
  categoryName: string;
  onCommentAdded: () => void;
}

const CommentForm = ({
  postId,
  categoryName,
  onCommentAdded,
}: CommentFormProps) => {
  const [content, setContent] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (content.trim() === "") {
      setError("Комментарий не может быть пустым.");
      return;
    }

    try {
      // Call the API to add the comment
      // await api.addComment(postId, { content, categoryName });
      setContent("");
      setError("");
      onCommentAdded(); // Notify parent component to refresh comments
    } catch (err) {
      setError("Ошибка при добавлении комментария.");
    }
  };

  return (
    <div className="form-container">
      <form className="comment-form" onSubmit={handleSubmit}>
        <div className="form-group">
          <textarea
            className="comment-textarea"
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder="Комментарий. Макс. длина 15000"
          />
        </div>
        {error && <div className="error-message">{error}</div>}
        <div className="form-footer">
          <button type="submit" className="submit-button">
            Отправить
          </button>
        </div>
      </form>
    </div>
  );
};

export default CommentForm;
