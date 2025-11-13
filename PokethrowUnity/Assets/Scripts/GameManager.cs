using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Gerenciador principal do jogo com silhuetas dinâmicas
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Referências da Cena")]
    public GameObject silhouettePrefab;      // Prefab com SpriteRenderer
    public GameObject pokeballPrefab;
    public Transform spawnPoint;
    public Transform targetPoint;

    [Header("UI")]
    public ForceBar forceBar;
    public Text feedbackText;
    public GameObject pokemonDisplay;
    public Image pokemonImage;
    public Text pokemonNameText;
    
    [Header("Configurações")]
    public float resetDelay = 5f;
    
    [Header("Configurações de Silhueta")]
    public bool useLocalSilhouettes = false;  // true = pasta Resources, false = web
    public string silhouetteUrlPattern = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{0}.png";
    
    // Estado do jogo
    private int currentPokemonId;
    private GameObject currentPokeball;
    private GameObject currentSilhouette;
    private string currentPokemonData;
    private bool isPlaying = false;

    void Start()
    {
        if (pokemonDisplay != null)
            pokemonDisplay.SetActive(false);

        #if UNITY_EDITOR
        Debug.Log("🧪 Modo Editor: Iniciando jogo automaticamente em 1 segundo...");
        Invoke("StartGame", 1f);
        #endif
    }

    public void StartGame()
    {
        if (isPlaying) return;
        
        Debug.Log("🎮 Jogo iniciado!");
        isPlaying = true;

        ResetScene();
        
        // Sorteia um Pokémon aleatório (1-150)
        currentPokemonId = Random.Range(1, 151);
        Debug.Log($"🎲 Pokémon sorteado: #{currentPokemonId}");
        
        // Cria a silhueta
        SpawnSilhouette();
        
        // Cria a pokébola
        SpawnPokeball();
        
        // Solicita dados do Pokémon
        WebGLBridge.RequestPokemonData(currentPokemonId);
        
        UpdateFeedback("Arraste e solte a Pokébola!");
    }

    /// <summary>
    /// Cria a silhueta do Pokémon na cena
    /// </summary>
    void SpawnSilhouette()
    {
        if (currentSilhouette != null)
            Destroy(currentSilhouette);
        
        currentSilhouette = Instantiate(silhouettePrefab, targetPoint.position, Quaternion.identity);
        
        // Adiciona collider se não tiver
        if (currentSilhouette.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = currentSilhouette.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 1f;
        }
        
        // Adiciona tag
        currentSilhouette.tag = "PokemonSilhouette";
        
        // Carrega a imagem da silhueta
        StartCoroutine(LoadSilhouetteImage(currentPokemonId));
        
        Debug.Log("👤 Silhueta criada!");
    }

    /// <summary>
    /// Carrega a imagem da silhueta (local ou web)
    /// </summary>
    IEnumerator LoadSilhouetteImage(int pokemonId)
    {
        SpriteRenderer spriteRenderer = currentSilhouette.GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError("❌ SpriteRenderer não encontrado na silhueta!");
            yield break;
        }

        if (useLocalSilhouettes)
        {
            // Carrega da pasta Resources/Silhouettes
            Sprite silhouette = Resources.Load<Sprite>($"Silhouettes/{pokemonId}");
            
            if (silhouette != null)
            {
                spriteRenderer.sprite = silhouette;
                Debug.Log($"✅ Silhueta local carregada: {pokemonId}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Silhueta local não encontrada para Pokémon #{pokemonId}");
            }
        }
        else
        {
            // Carrega da web
            string url = string.Format(silhouetteUrlPattern, pokemonId);
            
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f  // Pixels per unit
                );
                
                spriteRenderer.sprite = sprite;

                // Aplica cor preta para criar efeito de silhueta
                spriteRenderer.color = Color.black;
                
                // Ajusta o tamanho da silhueta
                float tamanhoDesejado = 2f; // Mude este valor para aumentar/diminuir
                currentSilhouette.transform.localScale = Vector3.one * tamanhoDesejado;
                
                Debug.Log($"✅ Silhueta da web carregada: {pokemonId}");
            }
            else
            {
                Debug.LogError($"❌ Erro ao carregar silhueta da web: {request.error}");
            }
        }
        
        // Ajusta o tamanho da silhueta
        AdjustSilhouetteSize(spriteRenderer);
    }

    /// <summary>
    /// Ajusta o tamanho da silhueta para caber na tela
    /// </summary>
    void AdjustSilhouetteSize(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;
        
        // Tamanho máximo desejado (em unidades do mundo)
        float maxSize = 3f;
        
        // Calcula o tamanho atual
        Bounds bounds = spriteRenderer.bounds;
        float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y);
        
        // Calcula a escala necessária
        if (maxDimension > maxSize)
        {
            float scale = maxSize / maxDimension;
            currentSilhouette.transform.localScale = Vector3.one * scale;
        }
        
        Debug.Log($"📏 Silhueta ajustada. Tamanho: {bounds.size}");
    }

    void SpawnPokeball()
    {
        if (currentPokeball != null)
            Destroy(currentPokeball);
        
        currentPokeball = Instantiate(pokeballPrefab, spawnPoint.position, Quaternion.identity);
        
        PokeballController controller = currentPokeball.GetComponent<PokeballController>();
        if (controller != null)
        {
            controller.SetGameManager(this);

            if (forceBar != null)
            {
                controller.forceBar = forceBar;
                Debug.Log("✅ Barra de força conectada à pokébola!");
            }
            else
            {
                Debug.LogWarning("⚠️ ForceBar não está configurado no GameManager!");
            }
        }
        
        Debug.Log("⚪ Pokébola criada!");
    }

    public void OnPokeballHitTarget(float throwForce, float accuracy)
    {
        Debug.Log($"💥 Colisão! Força: {throwForce:F2}, Precisão: {accuracy:F2}");
        
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

    void HandleCaptureSuccess()
    {
        Debug.Log("✅ Captura bem-sucedida!");
        UpdateFeedback("Capturado! Carregando dados...");

        // ⬇️ TOCA SOM DE SUCESSO
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCaptureSuccessSound();
        }
        
        StartCoroutine(WaitForPokemonData());
    }

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
            WebGLBridge.NotifyCaptureSuccess(currentPokemonData);
            StartCoroutine(ResetAfterDelay());
        }
        else
        {
            Debug.LogError("❌ Timeout ao aguardar dados do Pokémon!");
            UpdateFeedback("Erro ao carregar Pokémon. Tente novamente.");
            StartCoroutine(ResetAfterDelay());
        }
    }

    void DisplayPokemon()
    {
        PokemonData data = JsonUtility.FromJson<PokemonData>(currentPokemonData);
        
        if (pokemonDisplay != null)
        {
            pokemonDisplay.SetActive(true);
            
            if (pokemonNameText != null)
                pokemonNameText.text = $"#{data.id} {data.name.ToUpper()}";
            
            if (pokemonImage != null && !string.IsNullOrEmpty(data.image))
            {
                StartCoroutine(LoadPokemonImage(data.image));
            }
        }
        
        // Remove a silhueta (substitui pela imagem colorida)
        if (currentSilhouette != null)
        {
            // Fade out animation (opcional)
            StartCoroutine(FadeOutSilhouette());
        }
        
        UpdateFeedback($"Você capturou {data.name}!");
        Debug.Log($"🎉 Pokémon exibido: {data.name}");
    }

    /// <summary>
    /// Efeito de fade out na silhueta
    /// </summary>
    IEnumerator FadeOutSilhouette()
    {
        SpriteRenderer spriteRenderer = currentSilhouette.GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Color startColor = spriteRenderer.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }
        
        Destroy(currentSilhouette);
    }

    IEnumerator LoadPokemonImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            pokemonImage.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            
            Debug.Log("✅ Imagem do Pokémon carregada!");
        }
        else
        {
            Debug.LogError($"❌ Erro ao carregar imagem: {request.error}");
        }
    }

    void HandleCaptureFailed()
    {
        Debug.Log("❌ Captura falhou!");
        UpdateFeedback("Falhou! Tente novamente!");

        // ⬇️ TOCA SOM DE FALHA
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCaptureFailSound();
        }
        
        WebGLBridge.NotifyCaptureFailed();
        
        StartCoroutine(ResetPokeballAfterDelay(2f));
    }

    IEnumerator ResetPokeballAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentPokeball != null)
            Destroy(currentPokeball);
        
        SpawnPokeball();
        UpdateFeedback("Tente novamente! Arraste a Pokébola!");
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        ResetScene();
        isPlaying = false;

        WebGLBridge.ReturnToMenu();
        Debug.Log("🔙 Voltando ao menu...");
    }

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

    void UpdateFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
        
        Debug.Log($"💬 Feedback: {message}");
    }

    public void ReceivePokemonData(string jsonData)
    {
        currentPokemonData = jsonData;
        Debug.Log($"📦 Dados recebidos do React: {jsonData}");
    }

    public void OnPokemonDataError(string error)
    {
        Debug.LogError($"❌ Erro ao receber dados: {error}");
        UpdateFeedback("Erro ao buscar Pokémon. Tente novamente.");
        StartCoroutine(ResetAfterDelay());
    }
}

[System.Serializable]
public class PokemonData
{
    public int id;
    public string name;
    public string image;
    public string[] types;
}