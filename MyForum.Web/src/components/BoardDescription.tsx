import "../styles/ui/mf-text.css";

interface Props {
  name: string;
  description: string;
}

export default function BoardDescription({ name, description }: Props) {
  return (
    <section className="text-center fade-in">
      <h1 className="mf-text mf-text-big mf-text-primary">{name}</h1>
      <p className="mf-text-muted">{description}</p>
    </section>
  );
}
