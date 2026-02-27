import { PanelTopClose, PanelTopOpen } from "lucide-react";

interface Props {
  children: string;
  onClick?: () => void;
}

export default function ButtonVisibility({ children, onClick }: Props) {
  const isCloseAction = children.toLowerCase().includes("закрыть");

  return (
    <button className="mf-btn-link mf-text-middle" onClick={onClick}>
      {isCloseAction ? <PanelTopClose size={16} /> : <PanelTopOpen size={16} />}
      {children}
    </button>
  );
}
