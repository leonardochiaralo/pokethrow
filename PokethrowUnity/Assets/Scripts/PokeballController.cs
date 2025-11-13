using UnityEngine;

/// <summary>
/// Controla o comportamento da Pokébola (versão WebGL compatível)
/// Mecânica: Arrastar → Calcular força → Lançar → Detectar colisão
/// </summary>
public class PokeballController : MonoBehaviour
{
    [Header("UI")]
    public ForceBar forceBar;  // Referência à barra de força
    
    [Header("Configurações de Lançamento")]
    public float maxDragDistance = 3f;       // Distância máxima de arrasto
    public float forceMultiplier = 10f;      // Multiplicador da força de lançamento
    public float maxForce = 50f;             // Força máxima permitida
    
    [Header("Configurações Visuais")]
    public LineRenderer trajectoryLine;      // Linha de trajetória (opcional)
    public int trajectoryPoints = 20;        // Número de pontos na trajetória
    
    // Referências
    private GameManager gameManager;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private CircleCollider2D circleCollider;
    
    // Estado
    private Vector3 startPosition;
    private Vector3 dragStartPosition;
    private bool isDragging = false;
    private bool wasThrown = false;
    private float throwForce = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        circleCollider = GetComponent<CircleCollider2D>();
        
        // Configurações iniciais do Rigidbody
        rb.gravityScale = 0f;  // Sem gravidade até ser lançada
        rb.bodyType = RigidbodyType2D.Kinematic;  // Kinematic até ser lançada
        
        mainCamera = Camera.main;
        startPosition = transform.position;

        // Configura LineRenderer se existir
        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = trajectoryPoints;
            trajectoryLine.enabled = false;
        }
        
        if (forceBar == null)
        {
            forceBar = FindObjectOfType<ForceBar>();
            if (forceBar != null)
            {
                Debug.Log("✅ ForceBar encontrada no Awake!");
            }
            else
            {
                Debug.LogWarning("⚠️ ForceBar não encontrada no Awake!");
            }
        }
        
        Debug.Log("⚪ PokeballController inicializado!");
    }

    void Update()
    {
        if (wasThrown) 
        {
            // Debug.Log("⚠️ Pokébola já foi lançada, ignorando input");
            return;
        }

        // Detecta clique do mouse/toque
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = GetMouseWorldPosition();
            Debug.Log($"🖱️ Clique detectado! Mouse World Position: {mousePos}");
            Debug.Log($"📍 Pokeball Position: {transform.position}");
        
            bool isOver = IsMouseOverPokeball(mousePos);
            Debug.Log($"🎯 Mouse está sobre pokébola? {isOver}");
        
            if (isOver)
            {
                OnPokeballPressed(mousePos);
            }
        }

        // Detecta arrasto
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = GetMouseWorldPosition();
            Debug.Log($"↔️ Arrastando... Mouse: {mousePos}");
            OnPokeballDragged(mousePos);
        }

        // Detecta soltar
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = GetMouseWorldPosition();
            Debug.Log($"🔓 Soltou o mouse! Mouse: {mousePos}");
            OnPokeballReleased(mousePos);
        }
    }
    /// <summary>
    /// Verifica se o mouse está sobre a pokébola
    /// </summary>
    bool IsMouseOverPokeball(Vector3 mouseWorldPos)
    {
        if (circleCollider == null) return false;
        
        float distance = Vector2.Distance(mouseWorldPos, transform.position);
        float radius = circleCollider.radius * transform.localScale.x;
        
        return distance <= radius;
    }

    /// <summary>
    /// Chamado quando o jogador pressiona sobre a pokébola
    /// </summary>
    void OnPokeballPressed(Vector3 mouseWorldPos)
    {
        if (wasThrown) return;
        
        isDragging = true;
        dragStartPosition = mouseWorldPos;
        
        Debug.Log("🖱️ Começou a arrastar a Pokébola");

        // Ativa a linha de trajetória
        if (trajectoryLine != null)
            trajectoryLine.enabled = true;

        if (forceBar == null)
        {
            forceBar = FindObjectOfType<ForceBar>();
            Debug.Log("🔍 Procurando ForceBar novamente...");
        }

        Debug.Log($"ForceBar antes de Show: {forceBar != null}");

        if (forceBar != null)
        {
            forceBar.Show();
            forceBar.UpdateForce(0f); // Começa em 0%
            Debug.Log("📊 Barra de força mostrada!");
        }
        else
        {
            Debug.LogError("❌ ForceBar ainda é null!");
        }
    }

    /// <summary>
    /// Chamado enquanto arrasta a pokébola
    /// </summary>
    void OnPokeballDragged(Vector3 mouseWorldPos)
    {
        if (!isDragging || wasThrown) return;
        
        Vector3 dragVector = dragStartPosition - mouseWorldPos;
        
        // Limita a distância de arrasto
        if (dragVector.magnitude > maxDragDistance)
        {
            dragVector = dragVector.normalized * maxDragDistance;
        }
        
        // Move a pokébola na direção oposta ao arrasto (como um estilingue)
        transform.position = startPosition - dragVector;

        // Atualiza a visualização da trajetória
        if (trajectoryLine != null)
        {
            ShowTrajectory(dragVector);
        }
        
        if (forceBar != null)
        {
            float normalizedForce = dragVector.magnitude / maxDragDistance;
            forceBar.UpdateForce(normalizedForce);
        }
    }

    /// <summary>
    /// Chamado quando solta a pokébola
    /// </summary>
    void OnPokeballReleased(Vector3 mouseWorldPos)
    {
        if (!isDragging || wasThrown) return;
        
        isDragging = false;

        // Oculta a linha de trajetória
        if (trajectoryLine != null)
            trajectoryLine.enabled = false;
            
        if (forceBar != null)
        {
            forceBar.Hide();
        }
        
        // Calcula a força do lançamento
        Vector3 dragVector = dragStartPosition - mouseWorldPos;
        
        // Limita a força
        float forceMagnitude = Mathf.Min(dragVector.magnitude * forceMultiplier, maxForce);
        throwForce = forceMagnitude;
        
        // Lança a pokébola
        LaunchPokeball(dragVector.normalized * forceMagnitude);
        
        Debug.Log($"🚀 Pokébola lançada! Força: {throwForce:F2}");
    }

    /// <summary>
    /// Lança a pokébola com a força calculada
    /// </summary>
    void LaunchPokeball(Vector2 force)
    {
        wasThrown = true;
        
        // Ativa física
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        
        // Aplica a força
        rb.AddForce(force, ForceMode2D.Impulse);

        // Adiciona rotação para efeito visual
        rb.angularVelocity = 360f;
        
        // ⬇️ TOCA SOM DE ARREMESSO
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayThrowSound();
        }
    }

    /// <summary>
    /// Detecta colisão com a silhueta do Pokémon
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!wasThrown) return;
        
        // Verifica se colidiu com a silhueta
        if (collision.CompareTag("PokemonSilhouette") || 
            collision.gameObject.name.Contains("Silhouette"))
        {
            Debug.Log("💥 Pokébola atingiu a silhueta!");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHitSound();
            }
            
            // Calcula a precisão baseada na distância do centro
            float accuracy = CalculateAccuracy(collision);
            
            // Para a pokébola
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            
            // Notifica o GameManager
            if (gameManager != null)
            {
                gameManager.OnPokeballHitTarget(throwForce, accuracy);
            }
            
            // Efeito visual: pokébola "gruda" no alvo
            transform.position = collision.transform.position;
        }
    }

    /// <summary>
    /// Calcula a precisão do arremesso (quão perto do centro acertou)
    /// </summary>
    float CalculateAccuracy(Collider2D targetCollider)
    {
        Vector2 targetCenter = targetCollider.bounds.center;
        Vector2 hitPoint = transform.position;
        
        float distance = Vector2.Distance(hitPoint, targetCenter);
        float maxDistance = targetCollider.bounds.extents.magnitude;
        
        // Precisão: 1.0 = centro perfeito, 0.0 = borda
        float accuracy = 1f - Mathf.Clamp01(distance / maxDistance);
        
        Debug.Log($"🎯 Precisão: {accuracy:F2} (distância do centro: {distance:F2})");
        
        return accuracy;
    }

    /// <summary>
    /// Mostra a trajetória prevista do lançamento
    /// </summary>
    void ShowTrajectory(Vector3 dragVector)
    {
        if (trajectoryLine == null) return;
        
        Vector2 velocity = dragVector.normalized * Mathf.Min(dragVector.magnitude * forceMultiplier, maxForce);
        Vector2 position = startPosition;
        float timeStep = 0.1f;
        
        for (int i = 0; i < trajectoryPoints; i++)
        {
            trajectoryLine.SetPosition(i, position);
            
            // Simula física
            velocity += Physics2D.gravity * rb.gravityScale * timeStep;
            position += velocity * timeStep;
        }
    }

    /// <summary>
    /// Converte posição do mouse para coordenadas do mundo
    /// </summary>
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    /// <summary>
    /// Define a referência ao GameManager
    /// </summary>
    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
        Debug.Log("✅ GameManager configurado no PokeballController");
    }

    /// <summary>
    /// Detecta se saiu da tela (para evitar pokébolas perdidas)
    /// </summary>
    void OnBecameInvisible()
    {
        if (wasThrown)
        {
            Debug.Log("⚠️ Pokébola saiu da tela!");
            
            // Notifica falha se ainda não acertou
            if (gameManager != null)
            {
                gameManager.OnPokeballHitTarget(0f, 0f); // Força e precisão zero = falha automática
            }
        }
    }

    // Visualização no Editor (Gizmos)
    void OnDrawGizmos()
    {
        if (isDragging && !wasThrown)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPosition, maxDragDistance);
            
            Vector3 currentMousePos = GetMouseWorldPosition();
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPosition, currentMousePos);
        }
    }
}