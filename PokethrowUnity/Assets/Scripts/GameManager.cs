using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    [Header("Referências da Cena")]
    [SerializeField] private GameObject silhouettePrefab;
    [SerializeField] private GameObject pokeballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform targetPoint;

    [Header("UI")]
    [SerializeField] private ForceBar forceBar;
    [SerializeField] private Text feedbackText;
    [SerializeField] private GameObject pokemonDisplay;
    [SerializeField] private Image pokemonImage;
    [SerializeField] private Text pokemonNameText;

    [Header("Configurações")]
    [SerializeField] private float resetDelay = 5f;

    [Header("Configurações de Silhueta")]
    [SerializeField] private bool useLocalSilhouettes = false;
    [SerializeField] private string silhouetteUrlPattern =
        "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{0}.png";

    private int _currentPokemonId;
    private GameObject _currentPokeball;
    private GameObject _currentSilhouette;
    private string _currentPokemonData;
    private bool _isPlaying = false;

    private void Start()
    {
        if (pokemonDisplay != null)
            pokemonDisplay.SetActive(false);

        #if UNITY_EDITOR
            Debug.Log("🧪 Modo Editor: iniciando jogo automaticamente em 1 segundo...");
            Invoke(nameof(StartGame), 1f);
        #endif
    }

    public void StartGame()
    {
        if (_isPlaying) return;

        _isPlaying = true;
        ResetScene();

        _currentPokemonId = Random.Range(1, 151);
        Debug.Log($"🎲 Pokémon sorteado: #{_currentPokemonId}");

        SpawnSilhouette();
        SpawnPokeball();
        WebGLBridge.RequestPokemonData(_currentPokemonId);

        UpdateFeedback("Arraste e solte a Pokébola!");
        Debug.Log("🎮 Jogo iniciado!");
    }

    private void SpawnSilhouette()
    {
        if (_currentSilhouette != null)
            Destroy(_currentSilhouette);

        _currentSilhouette = Instantiate(silhouettePrefab, targetPoint.position, Quaternion.identity);
        AddColliderIfMissing(_currentSilhouette);
        _currentSilhouette.tag = "PokemonSilhouette";

        StartCoroutine(LoadSilhouetteImage(_currentPokemonId));
        Debug.Log("👤 Silhueta criada!");
    }

    private void AddColliderIfMissing(GameObject obj)
    {
        if (obj.GetComponent<Collider2D>() == null)
        {
            var collider = obj.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 1f;
        }
    }

    private IEnumerator LoadSilhouetteImage(int pokemonId)
    {
        var spriteRenderer = _currentSilhouette.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("❌ SpriteRenderer não encontrado na silhueta!");
            yield break;
        }

        if (useLocalSilhouettes)
        {
            LoadLocalSilhouette(spriteRenderer, pokemonId);
        }
        else
        {
            yield return LoadWebSilhouette(spriteRenderer, pokemonId);
        }

        AdjustSilhouetteSize(spriteRenderer);
    }

    private void LoadLocalSilhouette(SpriteRenderer renderer, int id)
    {
        var sprite = Resources.Load<Sprite>($"Silhouettes/{id}");
        if (sprite != null)
        {
            renderer.sprite = sprite;
            Debug.Log($"✅ Silhueta local carregada: {id}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Silhueta local não encontrada: #{id}");
        }
    }

    private IEnumerator LoadWebSilhouette(SpriteRenderer renderer, int id)
    {
        string url = string.Format(silhouetteUrlPattern, id);
        using var request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            renderer.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);

            renderer.color = Color.black;
            _currentSilhouette.transform.localScale = Vector3.one * 2f;
            Debug.Log($"✅ Silhueta web carregada: {id}");
        }
        else
        {
            Debug.LogError($"❌ Erro ao carregar silhueta web: {request.error}");
        }
    }

    private void AdjustSilhouetteSize(SpriteRenderer renderer)
    {
        if (renderer?.sprite == null) return;

        float maxSize = 3f;
        float maxDimension = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.y);
        if (maxDimension > maxSize)
            _currentSilhouette.transform.localScale = Vector3.one * (maxSize / maxDimension);

        Debug.Log($"📏 Silhueta ajustada. Tamanho: {renderer.bounds.size}");
    }

    private void SpawnPokeball()
    {
        if (_currentPokeball != null)
            Destroy(_currentPokeball);

        _currentPokeball = Instantiate(pokeballPrefab, spawnPoint.position, Quaternion.identity);

        if (_currentPokeball.TryGetComponent(out PokeballController controller))
        {
            controller.SetGameManager(this);
            controller.SetForceBar(forceBar);

            if (forceBar != null)
                Debug.Log("✅ Barra de força conectada!");
            else
                Debug.LogWarning("⚠️ ForceBar não configurada!");
        }

        Debug.Log("⚪ Pokébola criada!");
    }

    public void OnPokeballHitTarget(float throwForce, float accuracy)
    {
        Debug.Log($"💥 Colisão! Força: {throwForce:F2}, Precisão: {accuracy:F2}");
        bool success = CaptureSystem.CalculateCapture(throwForce, accuracy);

        if (success) HandleCaptureSuccess();
        else HandleCaptureFailed();
    }

    private void HandleCaptureSuccess()
    {
        Debug.Log("✅ Captura bem-sucedida!");
        UpdateFeedback("Capturado! Carregando dados...");
        AudioManager.Instance?.PlayCaptureSuccessSound();
        StartCoroutine(WaitForPokemonData());
    }

    private IEnumerator WaitForPokemonData()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (string.IsNullOrEmpty(_currentPokemonData) && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!string.IsNullOrEmpty(_currentPokemonData))
        {
            DisplayPokemon();
            WebGLBridge.NotifyCaptureSuccess(_currentPokemonData);
            StartCoroutine(ResetAfterDelay());
        }
        else
        {
            Debug.LogError("❌ Timeout ao aguardar dados do Pokémon!");
            UpdateFeedback("Erro ao carregar Pokémon. Tente novamente.");
            StartCoroutine(ResetAfterDelay());
        }
    }

    private void DisplayPokemon()
    {
        var data = JsonUtility.FromJson<PokemonData>(_currentPokemonData);
        if (pokemonDisplay != null) pokemonDisplay.SetActive(true);
        if (pokemonNameText != null) pokemonNameText.text = $"#{data.id} {data.name.ToUpper()}";

        if (pokemonImage != null && !string.IsNullOrEmpty(data.image))
            StartCoroutine(LoadPokemonImage(data.image));

        if (_currentSilhouette != null)
            StartCoroutine(FadeOutSilhouette());

        UpdateFeedback($"Você capturou {data.name}!");
        Debug.Log($"🎉 Pokémon exibido: {data.name}");
    }

    private IEnumerator FadeOutSilhouette()
    {
        var renderer = _currentSilhouette.GetComponent<SpriteRenderer>();
        if (renderer == null) yield break;

        float duration = 0.5f, elapsed = 0f;
        Color startColor = renderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            renderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(_currentSilhouette);
    }

    private IEnumerator LoadPokemonImage(string url)
    {
        using var request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            pokemonImage.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            Debug.Log("✅ Imagem do Pokémon carregada!");
        }
        else
        {
            Debug.LogError($"❌ Erro ao carregar imagem: {request.error}");
        }
    }

    private void HandleCaptureFailed()
    {
        Debug.Log("❌ Captura falhou!");
        UpdateFeedback("Falhou! Tente novamente!");
        AudioManager.Instance?.PlayCaptureFailSound();
        WebGLBridge.NotifyCaptureFailed();
        StartCoroutine(ResetPokeballAfterDelay(2f));
    }

    private IEnumerator ResetPokeballAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_currentPokeball != null) Destroy(_currentPokeball);
        SpawnPokeball();
        UpdateFeedback("Tente novamente! Arraste a Pokébola!");
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetScene();
        _isPlaying = false;
        WebGLBridge.ReturnToMenu();
        Debug.Log("🔙 Voltando ao menu...");
    }

    private void ResetScene()
    {
        if (_currentPokeball != null) Destroy(_currentPokeball);
        if (_currentSilhouette != null) Destroy(_currentSilhouette);
        if (pokemonDisplay != null) pokemonDisplay.SetActive(false);
        _currentPokemonData = null;
    }

    private void UpdateFeedback(string message)
    {
        if (feedbackText != null) feedbackText.text = message;
        Debug.Log($"💬 Feedback: {message}");
    }

    public void ReceivePokemonData(string jsonData)
    {
        _currentPokemonData = jsonData;
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
