import "./Menu.css";

export const Menu = ({ onPlayClick, onHistoryClick }) => {
  return (
    <div className="menu-container">
      <div className="menu-content">
        <h1 className="game-title">
          <span className="poke">Poké</span>
          <span className="throw">Throw</span>
        </h1>

        <p className="game-subtitle">Capture Pokémons lendários!</p>

        <div className="menu-buttons">
          <button className="btn btn-play" onClick={onPlayClick}>
            <span className="btn-icon">🎮</span>
            Jogar
          </button>

          <button className="btn btn-history" onClick={onHistoryClick}>
            <span className="btn-icon">📜</span>
            Histórico
          </button>
        </div>

        <div className="menu-footer">
          <p>Arraste a Pokébola e solte para capturar!</p>
        </div>
      </div>
    </div>
  );
};
