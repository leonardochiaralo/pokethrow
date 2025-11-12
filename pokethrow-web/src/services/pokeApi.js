import axios from "axios";

const API_BASE_URL = "https://pokeapi.co/api/v2";

export const pokeApi = {
  /**
   * Busca dados de um Pokémon pelo ID
   * @param {number} id - ID do Pokémon (1-150)
   * @returns {Promise<Object>} Dados do Pokémon
   */
  async getPokemonById(id) {
    try {
      const response = await axios.get(`${API_BASE_URL}/pokemon/${id}`);

      return {
        id: response.data.id,
        name: response.data.name,
        image: response.data.sprites.other["official-artwork"].front_default,
        types: response.data.types.map((t) => t.type.name),
        height: response.data.height,
        weight: response.data.weight,
        stats: response.data.stats.map((s) => ({
          name: s.stat.name,
          value: s.base_stat,
        })),
      };
    } catch (error) {
      console.error("Erro ao buscar Pokémon:", error);
      throw new Error("Falha ao buscar dados do Pokémon");
    }
  },

  /**
   * Gera um ID aleatório entre 1 e 150
   * @returns {number} ID aleatório
   */
  getRandomPokemonId() {
    return Math.floor(Math.random() * 150) + 1;
  },
};
