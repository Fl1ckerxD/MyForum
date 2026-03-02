import { Bookmark } from "lucide-react";
interface Props {
  name: string;
  description: string;
}

export default function BoardDescription({ name, description }: Props) {
  return (
    <section className="page-container text-center fade-in">
      <div className="board-header">
        <Bookmark size={18} className="mf-text-primary" />
        <h1 className="mf-text mf-text-big mf-text-primary">{name}</h1>
      </div>
      <p className="mf-text-muted">{description}</p>
    </section>
  );
}
