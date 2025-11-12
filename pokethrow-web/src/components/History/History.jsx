import { useHistoryStore } from "../../store/historyStore";
import "./History.css";

export const History = ({ onBack }) => {
  const { capturedPokemons, clearHistory } = useHistoryStore();

  const formatDate = (dateStr) => {
    return new Date(dateStr).toLocaleString("pt-BR", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="history-container">
      <div className="history-header">
        <button className="btn-back" onClick={onBack}>
          ← Voltar
        </button>
        <h2>Histórico de Capturas</h2>
        {capturedPokemons.length > 0 && (
          <button className="btn-clear" onClick={clearHistory}>
            Limpar Histórico
          </button>
        )}
      </div>

      <div className="history-stats">
        <div className="stat-card">
          <span className="stat-value">{capturedPokemons.length}</span>
          <span className="stat-label">Pokémons Capturados</span>
        </div>
      </div>

      <div className="history-list">
        {capturedPokemons.length === 0 ? (
          <div className="empty-state">
            <p className="empty-icon">🔍</p>
            <p>Nenhum Pokémon capturado ainda.</p>
            <p className="empty-hint">Comece a jogar para capturar Pokémons!</p>
          </div>
        ) : (
          capturedPokemons.map((pokemon) => (
            <div key={pokemon.id} className="pokemon-card">
              <div className="pokemon-image">
                <img src={pokemon.image} alt={pokemon.name} />
              </div>
              <div className="pokemon-info">
                <h3 className="pokemon-name">
                  #{pokemon.id} {pokemon.name}
                </h3>
                <div className="pokemon-types">
                  {pokemon.types.map((type) => (
                    <span key={type} className={`type-badge type-${type}`}>
                      {type}
                    </span>
                  ))}
                </div>
                <p className="capture-date">
                  Capturado em: {formatDate(pokemon.capturedAt)}
                </p>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};
