import { useEffect, type ReactNode } from "react";
import ReactDOM from "react-dom";
import { Icon, type IconName } from "./Icon";
import "./Modal.css";

type ModalProps = {
  open: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
  iconName?: IconName;
  footer?: ReactNode;
};

export const Modal = ({
  open,
  title,
  onClose,
  children,
  iconName,
  footer,
}: ModalProps) => {
  useEffect(() => {
    if (!open) return;

    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        onClose();
      }
    };

    document.addEventListener("keydown", handleEscape);
    return () => document.removeEventListener("keydown", handleEscape);
  }, [open, onClose]);

  if (!open) return null;

  return ReactDOM.createPortal(
    <div className="ui-modal__overlay" onClick={onClose}>
      <div
        className="admin-card ui-modal__card"
        onClick={(event) => event.stopPropagation()}
      >
        <h3 className="ui-modal__title">
          {iconName && <Icon name={iconName} size={16} />}
          {title}
        </h3>
        <div>{children}</div>
        {footer}
      </div>
    </div>,
    document.body,
  );
};
