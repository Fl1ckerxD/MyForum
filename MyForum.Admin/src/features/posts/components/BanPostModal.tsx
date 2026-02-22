import { Modal } from "../../../components/Modal";
import { Icon } from "../../../components/Icon";
import "./BanPostModal.css";

type BanPostModalProps = {
  postId: number | null;
  reason: string;
  expiresAt: string;
  boardOnly: boolean;
  isSubmitting: boolean;
  onReasonChange: (value: string) => void;
  onExpiresAtChange: (value: string) => void;
  onBoardOnlyChange: (value: boolean) => void;
  onSubmit: () => void;
  onClose: () => void;
};

export const BanPostModal = ({
  postId,
  reason,
  expiresAt,
  boardOnly,
  isSubmitting,
  onReasonChange,
  onExpiresAtChange,
  onBoardOnlyChange,
  onSubmit,
  onClose,
}: BanPostModalProps) => {
  return (
    <Modal
      open={postId !== null}
      onClose={onClose}
      title={`Бан пользователя (Post #${postId ?? "-"})`}
      iconName="shield"
      footer={
        <div className="ban-post-modal__actions">
          <button
            className="admin-btn admin-btn-primary"
            onClick={onSubmit}
            disabled={isSubmitting}
          >
            <Icon name="shield" size={14} />
            {isSubmitting ? "Отправка..." : "Подтвердить бан"}
          </button>
          <button className="admin-btn admin-btn-neutral" onClick={onClose}>
            Отмена
          </button>
        </div>
      }
    >
      <div className="ban-post-modal__body">
        <div className="ban-post-modal__field">
          <label className="ban-post-modal__label">Причина *</label>
          <textarea
            className="admin-textarea"
            value={reason}
            onChange={(event) => onReasonChange(event.target.value)}
            rows={3}
          />
        </div>

        <div className="ban-post-modal__field">
          <label className="ban-post-modal__label">Срок (необязательно)</label>
          <input
            className="admin-field"
            type="datetime-local"
            value={expiresAt}
            onChange={(event) => onExpiresAtChange(event.target.value)}
          />
        </div>

        <label className="ban-post-modal__label ban-post-modal__checkbox-label">
          <input
            type="checkbox"
            checked={boardOnly}
            onChange={(event) => onBoardOnlyChange(event.target.checked)}
          />
          Бан только для этой доски
        </label>
      </div>
    </Modal>
  );
};
