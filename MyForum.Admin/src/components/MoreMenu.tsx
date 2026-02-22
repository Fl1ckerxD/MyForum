import { useEffect, useId, useState, type ReactNode } from "react";
import "./MoreMenu.css";

type MoreMenuProps = {
  children: ReactNode;
  label?: string;
  className?: string;
};

export const MoreMenu = ({ children, label = "Еще", className }: MoreMenuProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const menuId = useId();
  const classes = className ? `more-menu ${className}` : "more-menu";

  useEffect(() => {
    const onMenuOpen = (event: Event) => {
      const customEvent = event as CustomEvent<{ id: string }>;
      if (customEvent.detail.id !== menuId) {
        setIsOpen(false);
      }
    };

    window.addEventListener("more-menu:open", onMenuOpen as EventListener);
    return () => window.removeEventListener("more-menu:open", onMenuOpen as EventListener);
  }, [menuId]);

  const handleToggle = (open: boolean) => {
    setIsOpen(open);
    if (open) {
      window.dispatchEvent(new CustomEvent("more-menu:open", { detail: { id: menuId } }));
    }
  };

  return (
    <details className={classes} open={isOpen} onToggle={(event) => handleToggle(event.currentTarget.open)}>
      <summary className="admin-btn admin-btn-neutral more-menu__trigger">{label}</summary>
      <div className="more-menu__panel">{children}</div>
    </details>
  );
};
