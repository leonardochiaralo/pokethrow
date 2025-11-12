import { create } from "zustand";
import { persist } from "zustand/middleware";

export const useHistoryStore = create(
  persist(
    (set, get) => ({
      capturedPokemons: [],

      addPokemon: (pokemon) => {
        set((state) => ({
          capturedPokemons: [
            {
              ...pokemon,
              capturedAt: new Date().toISOString(),
              id: Date.now(), // ID único para a captura
            },
            ...state.capturedPokemons,
          ],
        }));
      },

      clearHistory: () => set({ capturedPokemons: [] }),

      getTotalCaptures: () => get().capturedPokemons.length,
    }),
    {
      name: "pokethrow-history", // Nome da chave no localStorage
    }
  )
);
