import { useState, useEffect } from "react";
import { Menu } from "./components/Menu/Menu";
import { GameCanvas } from "./components/GameCanvas/GameCanvas";
import { History } from "./components/History/History";
import { useUnityInstance } from "./hooks/useUnityInstance";
import "./App.css";

function App() {
  const [currentScreen, setCurrentScreen] = useState("menu");
  const { unityProvider, isLoaded, loadingProgression, sendMessage } =
    useUnityInstance();

  const handlePlayClick = () => {
    setCurrentScreen("game");
  };

  // Quando o Unity carregar E estiver na tela de jogo, inicia o jogo
  useEffect(() => {
    if (isLoaded && currentScreen === "game") {
      console.log("🎮 Unity carregado! Iniciando jogo...");
      // Aguarda um momento para garantir que o Unity está pronto
      setTimeout(() => {
        sendMessage("GameManager", "StartGame");
      }, 500);
    }
  }, [isLoaded, currentScreen, sendMessage]);

  const handleHistoryClick = () => {
    setCurrentScreen("history");
  };

  const handleBackToMenu = () => {
    setCurrentScreen("menu");
  };

  return (
    <div className="app">
      {currentScreen === "menu" && (
        <Menu
          onPlayClick={handlePlayClick}
          onHistoryClick={handleHistoryClick}
        />
      )}

      {currentScreen === "game" && (
        <GameCanvas
          unityProvider={unityProvider}
          isLoaded={isLoaded}
          loadingProgression={loadingProgression}
          onBack={handleBackToMenu}
        />
      )}

      {currentScreen === "history" && <History onBack={handleBackToMenu} />}
    </div>
  );
}

export default App;
