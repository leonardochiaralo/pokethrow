using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Gerenciador principal do jogo
/// Controla o fluxo: iniciar jogo → sortear → capturar → exibir resultado
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Referências da Cena")]
    public GameObject silhouettePrefab;      // Silhueta do Pokémon
    public GameObject pokeballPrefab;        // Prefab da Pokébola
    public Transform spawnPoint;             // Ponto de spawn da pokébola
    public Transform targetPoint;            // Posição da silhueta (alvo)
    
    [Header("UI")]
    public Text feedbackText;                // Texto de feedback para o jogador
    public GameObject pokemonDisplay;        // Objeto que exibirá o Pokémon capturado
    public Image pokemonImage;               // Imagem do Pokémon
    public Text pokemonNameText;             // Nome do Pokémon
    
    [Header("Configurações")]
    public float resetDelay = 3f;            // Tempo antes de resetar após captura
    
    // Estado do jogo
    private int currentPokemonId;
    private GameObject currentPokeball;
    private GameObject currentSilhouette;
    private string currentPokemonData;
    private bool isPlaying = false;

    void Start()
    {
        // Inicializa o display do Pokémon como oculto
        if (pokemonDisplay != null)
            pokemonDisplay.SetActive(false);
    }

    /// <summary>
    /// Chamado pelo React quando o jogador clica em "Jogar"
    /// </summary>
    public void StartGame()
    {
        Debug.Log("========== START GAME CHAMADO ==========");

        if (isPlaying)
        {
            Debug.Log("⚠️ Jogo já está rodando!");
            return;
        }
        
        Debug.Log("🎮 Jogo iniciado!");
        isPlaying = true;
        
        // Limpa a cena
        Debug.Log("Limpando cena...");
        ResetScene();
        
        // Sorteia um Pokémon aleatório (1-150)
        currentPokemonId = Random.Range(1, 151);
        Debug.Log($"🎲 Pokémon sorteado: #{currentPokemonId}");
        
        // Cria a silhueta na cena
        Debug.Log("Criando silhueta...");
        SpawnSilhouette();
        
        // Cria a pokébola
        Debug.Log("Criando pokébola...");
        SpawnPokeball();
        
        // Solicita dados do Pokémon ao React
        Debug.Log($"Solicitando dados do Pokémon #{currentPokemonId}...");
        WebGLBridge.RequestPokemonData(currentPokemonId);

        UpdateFeedback("Arraste e solte a Pokébola!");
        Debug.Log("========== START GAME CONCLUÍDO ==========");
    }

    /// <summary>
    /// Cria a silhueta do Pokémon na cena
    /// </summary>
    void SpawnSilhouette()
    {
        if (currentSilhouette != null)
            Destroy(currentSilhouette);
        
        currentSilhouette = Instantiate(silhouettePrefab, targetPoint.position, Quaternion.identity);
        
        // Adiciona um collider para detectar a captura
        if (currentSilhouette.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = currentSilhouette.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 1f;
        }
        
        Debug.Log("👤 Silhueta criada!");
    }

    /// <summary>
    /// Cria a pokébola na posição inicial
    /// </summary>
    void SpawnPokeball()
    {
        Debug.Log("=== SPAWN POKEBALL INICIADO ===");

        if (currentPokeball != null)
        {
            Debug.Log("Destruindo pokébola anterior...");
            Destroy(currentPokeball);
        }

        if (pokeballPrefab == null)
        {
            Debug.LogError("❌ ERRO: pokeballPrefab está NULL! Arraste o prefab no Inspector!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("❌ ERRO: spawnPoint está NULL!");
            return;
        }

        Debug.Log($"SpawnPoint position: {spawnPoint.position}");

        currentPokeball = Instantiate(pokeballPrefab, spawnPoint.position, Quaternion.identity);

        if (currentPokeball == null)
        {
            Debug.LogError("❌ ERRO: Falha ao instanciar pokébola!");
            return;
        }

        Debug.Log($"✅ Pokébola instanciada! Position: {currentPokeball.transform.position}");
        
        // Configura o controller da pokébola
        PokeballController controller = currentPokeball.GetComponent<PokeballController>();
        if (controller != null)
        {
            Debug.Log("✅ PokeballController encontrado!");
            controller.SetGameManager(this);
        }
        else
        {
            Debug.LogError("❌ ERRO: PokeballController não encontrado no prefab!");
        }
            Debug.Log("⚪ Pokébola criada!");
    }

    /// <summary>
    /// Chamado quando a pokébola colide com a silhueta
    /// </summary>
    public void OnPokeballHitTarget(float throwForce, float accuracy)
    {
        Debug.Log($"💥 Colisão! Força: {throwForce:F2}, Precisão: {accuracy:F2}");
        
        // Calcula a chance de captura
        bool captureSuccess = CaptureSystem.CalculateCapture(throwForce, accuracy);
        
        if (captureSuccess)
        {
            HandleCaptureSuccess();
        }
        else
        {
            HandleCaptureFailed();
        }
    }

    /// <summary>
    /// Lida com captura bem-sucedida
    /// </summary>
    void HandleCaptureSuccess()
    {
        Debug.Log("✅ Captura bem-sucedida!");
        UpdateFeedback("Capturado! Carregando dados...");
        
        // Aguarda os dados do React
        StartCoroutine(WaitForPokemonData());
    }

    /// <summary>
    /// Aguarda os dados do Pokémon chegarem do React
    /// </summary>
    IEnumerator WaitForPokemonData()
    {
        float timeout = 5f;
        float elapsed = 0f;
        
        while (string.IsNullOrEmpty(currentPokemonData) && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!string.IsNullOrEmpty(currentPokemonData))
        {
            DisplayPokemon();
            
            // Notifica o React para salvar no histórico
            WebGLBridge.NotifyCaptureSuccess(currentPokemonData);
            
            // Reseta após um delay
            StartCoroutine(ResetAfterDelay());
        }
        else
        {
            Debug.LogError("❌ Timeout ao aguardar dados do Pokémon!");
            UpdateFeedback("Erro ao carregar Pokémon. Tente novamente.");
            StartCoroutine(ResetAfterDelay());
        }
    }

    /// <summary>
    /// Exibe o Pokémon capturado na tela
    /// </summary>
    void DisplayPokemon()
    {
        // Parse dos dados JSON
        PokemonData data = JsonUtility.FromJson<PokemonData>(currentPokemonData);
        
        if (pokemonDisplay != null)
        {
            pokemonDisplay.SetActive(true);
            
            if (pokemonNameText != null)
                pokemonNameText.text = $"#{data.id} {data.name.ToUpper()}";
            
            // Carrega a imagem do Pokémon
            if (pokemonImage != null && !string.IsNullOrEmpty(data.image))
            {
                StartCoroutine(LoadPokemonImage(data.image));
            }
        }
        
        // Remove a silhueta
        if (currentSilhouette != null)
            Destroy(currentSilhouette);
        
        UpdateFeedback($"Você capturou {data.name}!");
        Debug.Log($"🎉 Pokémon exibido: {data.name}");
    }

    /// <summary>
    /// Carrega a imagem do Pokémon da URL
    /// </summary>
    IEnumerator LoadPokemonImage(string url)
    {
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        
        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
            pokemonImage.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
        else
        {
            Debug.LogError($"❌ Erro ao carregar imagem: {request.error}");
        }
    }

    /// <summary>
    /// Lida com captura falhada
    /// </summary>
    void HandleCaptureFailed()
    {
        Debug.Log("❌ Captura falhou!");
        UpdateFeedback("Falhou! Tente novamente!");
        
        // Notifica o React
        WebGLBridge.NotifyCaptureFailed();
        
        // Reseta a pokébola após um delay
        StartCoroutine(ResetPokeballAfterDelay(2f));
    }

    /// <summary>
    /// Reseta apenas a pokébola (para tentar novamente)
    /// </summary>
    IEnumerator ResetPokeballAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentPokeball != null)
            Destroy(currentPokeball);
        
        SpawnPokeball();
        UpdateFeedback("Tente novamente! Arraste a Pokébola!");
    }

    /// <summary>
    /// Reseta o jogo inteiro após um delay
    /// </summary>
    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetScene();
        isPlaying = false;
        UpdateFeedback("Clique em 'Jogar' para capturar outro Pokémon!");
    }

    /// <summary>
    /// Limpa toda a cena
    /// </summary>
    void ResetScene()
    {
        if (currentPokeball != null)
            Destroy(currentPokeball);
        
        if (currentSilhouette != null)
            Destroy(currentSilhouette);
        
        if (pokemonDisplay != null)
            pokemonDisplay.SetActive(false);
        
        currentPokemonData = null;
    }

    /// <summary>
    /// Atualiza o texto de feedback na UI
    /// </summary>
    void UpdateFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
        
        Debug.Log($"💬 Feedback: {message}");
    }

    /// <summary>
    /// Recebe os dados do Pokémon vindos do React (chamado via SendMessage)
    /// </summary>
    public void ReceivePokemonData(string jsonData)
    {
        currentPokemonData = jsonData;
        Debug.Log($"📦 Dados recebidos do React: {jsonData}");
    }

    /// <summary>
    /// Chamado se houver erro ao buscar dados (chamado via SendMessage)
    /// </summary>
    public void OnPokemonDataError(string error)
    {
        Debug.LogError($"❌ Erro ao receber dados: {error}");
        UpdateFeedback("Erro ao buscar Pokémon. Tente novamente.");
        StartCoroutine(ResetAfterDelay());
    }
}

/// <summary>
/// Classe para deserializar os dados JSON do Pokémon
/// </summary>
[System.Serializable]
public class PokemonData
{
    public int id;
    public string name;
    public string image;
    public string[] types;
}