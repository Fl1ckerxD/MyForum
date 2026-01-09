import "../styles/ui/button.css";

interface Props {
  children: string;
  onClick?: () => void;
}

export default function ButtonVisibility({ children, onClick }: Props) {
  return (
    <button className="mf-btn-link mf-text-middle" onClick={onClick}>
      {children}
    </button>
  );
}
