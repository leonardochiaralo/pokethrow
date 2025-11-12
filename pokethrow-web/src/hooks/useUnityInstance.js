import { useUnityContext } from "react-unity-webgl";
import { useEffect, useCallback } from "react";
import { pokeApi } from "../services/pokeApi";
import { useHistoryStore } from "../store/historyStore";

export const useUnityInstance = () => {
  const addPokemon = useHistoryStore((state) => state.addPokemon);

  const {
    unityProvider,
    isLoaded,
    loadingProgression,
    sendMessage,
    addEventListener,
    removeEventListener,
  } = useUnityContext({
    loaderUrl: "/unity/Build/unity.loader.js",
    dataUrl: "/unity/Build/unity.data",
    frameworkUrl: "/unity/Build/unity.framework.js",
    codeUrl: "/unity/Build/unity.wasm",
  });

  // Unity solicita dados do Pokémon
  const handleRequestPokemon = useCallback(
    async (pokemonIdStr) => {
      try {
        const pokemonId = parseInt(pokemonIdStr);
        console.log("🔍 Buscando Pokémon ID:", pokemonId);

        const pokemonData = await pokeApi.getPokemonById(pokemonId);

        console.log("✅ Dados recebidos:", pokemonData);

        // Envia dados de volta para Unity
        sendMessage(
          "GameManager",
          "ReceivePokemonData",
          JSON.stringify(pokemonData)
        );
      } catch (error) {
        console.error("❌ Erro ao buscar Pokémon:", error);
        sendMessage("GameManager", "OnPokemonDataError", error.message);
      }
    },
    [sendMessage]
  );

  // Unity notifica captura bem-sucedida
  const handleCaptureSuccess = useCallback(
    (pokemonDataStr) => {
      try {
        const pokemonData = JSON.parse(pokemonDataStr);
        addPokemon(pokemonData);
        console.log("🎉 Pokémon capturado e salvo:", pokemonData.name);
      } catch (error) {
        console.error("❌ Erro ao salvar captura:", error);
      }
    },
    [addPokemon]
  );

  // Unity notifica captura falhada
  const handleCaptureFailed = useCallback(() => {
    console.log("❌ Captura falhou!");
  }, []);

  // Registra a função global que o Unity vai chamar via .jslib
  useEffect(() => {
    window.unityToReact = (eventName, data) => {
      console.log("[Unity → React]", eventName, data);

      if (eventName === "RequestPokemonData") {
        handleRequestPokemon(data);
      } else if (eventName === "OnCaptureSuccess") {
        handleCaptureSuccess(data);
      } else if (eventName === "OnCaptureFailed") {
        handleCaptureFailed();
      }
    };

    return () => {
      delete window.unityToReact;
    };
  }, [handleRequestPokemon, handleCaptureSuccess, handleCaptureFailed]);

  // Listeners do react-unity-webgl (backup/alternativa)
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

  return {
    unityProvider,
    isLoaded,
    loadingProgression,
    sendMessage,
  };
};
