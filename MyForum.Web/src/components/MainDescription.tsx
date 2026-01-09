import "../styles/ui/span.css";
import "../styles/ui/card.css";

const MainDescription = () => {
  return (
    <section className="fade-in-up delay-200ms">
      <div className="glassCard glassCard-compact">
        <p>
          <span className="highlight">MyForum</span> - это система форумов, где
          можно общаться быстро и свободно, где любая точка зрения имеет право
          на жизнь. От вас необходимо только соблюдение правил.
        </p>
      </div>
    </section>
  );
};

export default MainDescription;
