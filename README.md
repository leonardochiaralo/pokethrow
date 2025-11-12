# 🎮 PokéThrow

Um mini-jogo interativo de captura de Pokémon desenvolvido com Unity WebGL + React, integrado com a PokéAPI.

![PokéThrow](https://img.shields.io/badge/Unity-2023-black?logo=unity)
![React](https://img.shields.io/badge/React-18-blue?logo=react)
![Vite](https://img.shields.io/badge/Vite-5-purple?logo=vite)

---

## 📋 Sobre o Projeto

PokéThrow é um jogo web onde o jogador deve capturar Pokémons misteriosos arremessando Pokébolas. O jogo combina:

- **Unity WebGL** para a mecânica de gameplay
- **React + Vite** para a interface web
- **PokéAPI** para dados dos Pokémons
- **Sistema de probabilidade** baseado em força e precisão do arremesso

---

## 🎯 Funcionalidades

### Gameplay

- ✅ Mecânica de arremesso (arrastar e soltar)
- ✅ Sistema de física realista
- ✅ Cálculo de captura baseado em força e precisão
- ✅ Sorteio aleatório de Pokémons (1-150)
- ✅ Feedback visual e mensagens

### Interface

- ✅ Menu inicial responsivo
- ✅ Loading screen animado
- ✅ Histórico de capturas persistente
- ✅ Design temático Pokémon
- ✅ Animações e transições suaves

### Integração

- ✅ Comunicação Unity ↔ React via WebGL Bridge
- ✅ Consumo da PokéAPI
- ✅ LocalStorage para histórico

---

## 🛠️ Tecnologias Utilizadas

### Front-end

- **React 18** - Biblioteca UI
- **Vite** - Build tool
- **Zustand** - Gerenciamento de estado
- **Axios** - Requisições HTTP
- **CSS3** - Estilização

### Gameplay

- **Unity 6000.2.10f1** - Engine de jogo
- **C#** - Linguagem de programação
- **WebGL** - Plataforma de build

### API

- **PokéAPI** - Dados dos Pokémons

---

## 📦 Estrutura do Projeto

```
pokethrow/
├── pokethrow-web/              # Aplicação React
│   ├── public/
│   │   └── unity/              # Build WebGL do Unity
│   │       └── Build/
│   │           ├── unity.loader.js
│   │           ├── unity.data
│   │           ├── unity.framework.js
│   │           └── unity.wasm
│   ├── src/
│   │   ├── components/
│   │   │   ├── Menu/
│   │   │   ├── GameCanvas/
│   │   │   └── History/
│   │   ├── hooks/
│   │   │   └── useUnityInstance.js
│   │   ├── services/
│   │   │   └── pokeApi.js
│   │   ├── store/
│   │   │   └── historyStore.js
│   │   ├── App.jsx
│   │   └── main.jsx
│   └── package.json
│
└── PokethrowUnity/             # Projeto Unity
    └── Assets/
        ├── Scenes/
        │   └── MainGame.unity
        ├── Scripts/
        │   ├── GameManager.cs
        │   ├── PokeballController.cs
        │   ├── CaptureSystem.cs
        │   └── WebGLBridge.cs
        └── Plugins/
            └── WebGL/
                └── ReactBridge.jslib
```

---

## 🚀 Como Executar

### Pré-requisitos

- Node.js (v18+)
- Unity 2023+
- npm ou yarn

### Instalação

1. **Clone o repositório**

```bash
git clone https://github.com/seu-usuario/pokethrow.git
cd pokethrow
```

2. **Instalar dependências do React**

```bash
cd pokethrow-web
npm install
```

3. **Executar o projeto**

```bash
npm run dev
```

4. **Acessar no navegador**

```
http://localhost:5173
```

---

## 🎮 Como Jogar

1. Clique em **"Jogar"** no menu inicial
2. Aguarde o jogo carregar
3. **Arraste** a Pokébola (círculo vermelho) para trás
4. **Solte** para lançar em direção à silhueta
5. Quanto mais **forte** e **preciso** o arremesso, maior a chance de captura!
6. Se capturar, o Pokémon será revelado e salvo no histórico
7. Se falhar, tente novamente!

---

## 📊 Sistema de Captura

A taxa de captura é calculada por:

```
Taxa Base: 50%
Bônus de Força: até +30% (baseado na força do arremesso)
Bônus de Precisão: até +20% (baseado na proximidade do centro)

Taxa Final = Base + Bônus Força + Bônus Precisão (máximo 100%)
```

---

## 🔧 Build Unity

Para gerar um novo build do Unity:

1. Abra o projeto Unity (`PokethrowUnity/`)
2. **Edit → Project Settings → Player**
   - Active Input Handling: **Input Manager (Old)**
3. **File → Build Settings**
   - Platform: **WebGL**
   - Compression Format: **Disabled**
4. **Build** e salve em `pokethrow-web/public/unity/`

---

## 🎨 Personalização

### Ajustar dificuldade

Edite `Assets/Scripts/CaptureSystem.cs`:

```csharp
private const float BASE_CAPTURE_RATE = 0.50f;      // Taxa base
private const float MAX_FORCE_BONUS = 0.30f;        // Bônus força
private const float MAX_ACCURACY_BONUS = 0.20f;     // Bônus precisão
```

### Alterar range de Pokémons

Edite `Assets/Scripts/GameManager.cs`:

```csharp
currentPokemonId = Random.Range(1, 151);  // 1-150 (Gen 1)
// Ou use Random.Range(1, 1011) para todas as gerações!
```

---

## 🐛 Troubleshooting

### Unity não carrega no navegador

- Verifique se os arquivos estão em `public/unity/Build/`
- Certifique-se que Compression Format está **Disabled**
- Limpe o cache do navegador (Ctrl+Shift+Delete)

### Input não funciona

- **Edit → Project Settings → Player**
- Active Input Handling: **Input Manager (Old)**

### Erro de CORS na PokéAPI

- A PokéAPI permite CORS, mas verifique sua conexão
- Teste a API diretamente: `https://pokeapi.co/api/v2/pokemon/25`

---

## 📄 Licença

Este projeto foi criado para fins educacionais.

Pokémon e todos os personagens relacionados são © Nintendo/Creatures Inc./GAME FREAK inc.

---

## 🙏 Créditos

- **PokéAPI** - https://pokeapi.co
- **Unity** - Game Engine
- **React** - UI Library
- **Sprites Oficiais** - The Pokémon Company

---

## 📧 Contato

Desenvolvido por [Seu Nome]

- GitHub: [@leonardochiaralo](https://github.com/leonardochiaralo)
- LinkedIn: [Leonardo Chiaralo](https://linkedin.com/in/leonardochiaralo)

---

**Divirta-se capturando Pokémons! 🎉**
