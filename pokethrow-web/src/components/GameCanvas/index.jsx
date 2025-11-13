import { Unity } from "react-unity-webgl";
import "./GameCanvas.css";

function GameCanvas({ unityProvider, isLoaded, loadingProgression, onBack }) {
  const renderLoadingOverlay = () => (
    <div className="loading-overlay">
      <div className="loading-content">
        <div className="pokeball-loader"></div>
        <p>Carregando o jogo...</p>
        <div className="progress-bar">
          <div
            className="progress-fill"
            style={{ width: `${loadingProgression * 100}%` }}
          />
        </div>
        <span>{Math.round(loadingProgression * 100)}%</span>
      </div>
    </div>
  );

  return (
    <div className="game-canvas-container">
      <header className="game-header">
        <button className="btn-back" onClick={onBack}>
          ← Voltar ao Menu
        </button>
        <h2>Capture o Pokémon!</h2>
      </header>
      {!isLoaded && renderLoadingOverlay()}
      <div className="unity-wrapper">
        <Unity
          unityProvider={unityProvider}
          style={{
            width: "100%",
            height: "100%",
            visibility: isLoaded ? "visible" : "hidden",
          }}
        />
      </div>
    </div>
  );
}

export default GameCanvas;
