import { useState, useEffect, useCallback } from "react";
import Menu from "./components/Menu";
import GameCanvas from "./components/GameCanvas";
import History from "./components/History";
import { useUnityInstance } from "./hooks/useUnityInstance";
import "./App.css";

const SCREENS = {
  MENU: "menu",
  GAME: "game",
  HISTORY: "history",
};

function App() {
  const [currentScreen, setCurrentScreen] = useState(SCREENS.MENU);

  const { unityProvider, isLoaded, loadingProgression, sendMessage } =
    useUnityInstance(() => setCurrentScreen(SCREENS.MENU));

  const handleNavigate = useCallback(
    (screen) => () => setCurrentScreen(screen),
    []
  );

  useEffect(() => {
    if (isLoaded && currentScreen === SCREENS.GAME) {
      const startTimeout = setTimeout(() => {
        sendMessage("GameManager", "StartGame");
      }, 500);

      return () => clearTimeout(startTimeout);
    }
  }, [isLoaded, currentScreen, sendMessage]);

  const renderScreen = () => {
    switch (currentScreen) {
      case SCREENS.MENU:
        return (
          <Menu
            onPlayClick={handleNavigate(SCREENS.GAME)}
            onHistoryClick={handleNavigate(SCREENS.HISTORY)}
          />
        );

      case SCREENS.GAME:
        return (
          <GameCanvas
            unityProvider={unityProvider}
            isLoaded={isLoaded}
            loadingProgression={loadingProgression}
            onBack={handleNavigate(SCREENS.MENU)}
          />
        );

      case SCREENS.HISTORY:
        return <History onBack={handleNavigate(SCREENS.MENU)} />;

      default:
        return null;
    }
  };

  return <div className="app">{renderScreen()}</div>;
}

export default App;
