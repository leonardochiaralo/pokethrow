import axios from "axios";

const API_BASE_URL = "https://pokeapi.co/api/v2";

async function getPokemonById(id) {
  try {
    const { data } = await axios.get(`${API_BASE_URL}/pokemon/${id}`);

    return {
      id: data.id,
      name: data.name,
      image: data.sprites.other["official-artwork"].front_default,
      types: data.types.map((t) => t.type.name),
      height: data.height,
      weight: data.weight,
      stats: data.stats.map((s) => ({
        name: s.stat.name,
        value: s.base_stat,
      })),
    };
  } catch (error) {
    console.error("Erro ao buscar Pokémon:", error);
    throw new Error("Falha ao buscar dados do Pokémon");
  }
}

function getRandomPokemonId() {
  return Math.floor(Math.random() * 150) + 1;
}

export const pokeApi = {
  getPokemonById,
  getRandomPokemonId,
};
