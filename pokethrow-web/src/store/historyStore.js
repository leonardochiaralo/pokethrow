import { create } from "zustand";
import { persist } from "zustand/middleware";

export const useHistoryStore = create(
  persist(
    (set, get) => ({
      capturedPokemons: [],

      addPokemon: (pokemon) =>
        set((state) => ({
          capturedPokemons: [
            {
              ...pokemon,
              capturedAt: new Date().toISOString(),
              id: Date.now(),
            },
            ...state.capturedPokemons,
          ],
        })),

      clearHistory: () => set({ capturedPokemons: [] }),

      getTotalCaptures: () => get().capturedPokemons.length,
    }),
    { name: "pokethrow-history" }
  )
);
