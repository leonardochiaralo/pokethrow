import { useState, useEffect } from "react";
import { Menu } from "./components/Menu/Menu";
import { GameCanvas } from "./components/GameCanvas/GameCanvas";
import { History } from "./components/History/History";
import { useUnityInstance } from "./hooks/useUnityInstance";
import "./App.css";

function App() {
  const [currentScreen, setCurrentScreen] = useState("menu");

  // ⬅️ PASSA O CALLBACK!
  const { unityProvider, isLoaded, loadingProgression, sendMessage } =
    useUnityInstance(() => {
      setCurrentScreen("menu"); // Volta ao menu
    });

  const handlePlayClick = () => {
    setCurrentScreen("game");
  };

  useEffect(() => {
    if (isLoaded && currentScreen === "game") {
      console.log("🎮 Unity carregado! Iniciando jogo...");
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
