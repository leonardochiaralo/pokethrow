import { useUnityContext } from "react-unity-webgl";
import { useEffect, useCallback } from "react";
import { pokeApi } from "../services/pokeApi";
import { useHistoryStore } from "../store/historyStore";

const BASE = import.meta.env.BASE_URL;

export function useUnityInstance(onReturnToMenu) {
  const addPokemon = useHistoryStore((state) => state.addPokemon);

  const {
    unityProvider,
    isLoaded,
    loadingProgression,
    sendMessage,
    addEventListener,
    removeEventListener,
  } = useUnityContext({
    loaderUrl: `${BASE}unity/Build/unity.loader.js`,
    dataUrl: `${BASE}unity/Build/unity.data`,
    frameworkUrl: `${BASE}unity/Build/unity.framework.js`,
    codeUrl: `${BASE}unity/Build/unity.wasm`,
  });

  const handleRequestPokemon = useCallback(
    async (pokemonIdStr) => {
      try {
        const pokemonId = parseInt(pokemonIdStr, 10);
        const pokemonData = await pokeApi.getPokemonById(pokemonId);

        sendMessage(
          "GameManager",
          "ReceivePokemonData",
          JSON.stringify(pokemonData)
        );
      } catch (error) {
        sendMessage("GameManager", "OnPokemonDataError", error.message);
      }
    },
    [sendMessage]
  );

  const handleCaptureSuccess = useCallback(
    (pokemonDataStr) => {
      try {
        const pokemonData = JSON.parse(pokemonDataStr);
        addPokemon(pokemonData);
      } catch (error) {
        console.error("Erro ao processar Pokémon capturado:", error);
      }
    },
    [addPokemon]
  );

  const handleCaptureFailed = useCallback(() => {
    console.warn("Captura falhou.");
  }, []);

  const handleReturnToMenu = useCallback(() => {
    if (onReturnToMenu) onReturnToMenu();
  }, [onReturnToMenu]);

  const handleUnityEvent = useCallback(
    (eventName, data) => {
      const eventHandlers = {
        RequestPokemonData: () => handleRequestPokemon(data),
        OnCaptureSuccess: () => handleCaptureSuccess(data),
        OnCaptureFailed: () => handleCaptureFailed(),
        ReturnToMenu: () => handleReturnToMenu(),
      };

      const handler = eventHandlers[eventName];
      if (handler) handler();
    },
    [
      handleRequestPokemon,
      handleCaptureSuccess,
      handleCaptureFailed,
      handleReturnToMenu,
    ]
  );

  useEffect(() => {
    window.unityToReact = handleUnityEvent;
    return () => {
      delete window.unityToReact;
    };
  }, [handleUnityEvent]);

  useEffect(() => {
    addEventListener("RequestPokemonData", handleRequestPokemon);
    addEventListener("OnCaptureSuccess", handleCaptureSuccess);
    addEventListener("OnCaptureFailed", handleCaptureFailed);

    return () => {
      removeEventListener("RequestPokemonData", handleRequestPokemon);
      removeEventListener("OnCaptureSuccess", handleCaptureSuccess);
      removeEventListener("OnCaptureFailed", handleCaptureFailed);
    };
  }, [
    addEventListener,
    removeEventListener,
    handleRequestPokemon,
    handleCaptureSuccess,
    handleCaptureFailed,
  ]);

  return { unityProvider, isLoaded, loadingProgression, sendMessage };
}
