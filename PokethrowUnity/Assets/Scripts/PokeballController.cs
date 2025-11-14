using UnityEngine;

public class PokeballController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private ForceBar forceBar;

    [Header("Configurações de Lançamento")]
    public float maxDragDistance = 3f;
    public float forceMultiplier = 10f;
    public float maxForce = 50f;

    [Header("Configurações Visuais")]
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 20;

    [Header("Configurações de Miss Detection")]
    [Tooltip("Tempo máximo antes de considerar miss (segundos)")]
    [SerializeField] private float missTimeout = 3f;
    
    [Tooltip("Posição Y mínima antes de considerar miss")]
    [SerializeField] private float minYPosition = -10f;
    
    [Tooltip("Usar detecção por câmera (OnBecameInvisible)")]
    [SerializeField] private bool useCameraDetection = true;

    private GameManager gameManager;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private CircleCollider2D circleCollider;

    private Vector3 startPosition;
    private Vector3 dragStartPosition;
    private bool isDragging = false;
    private bool wasThrown = false;
    private float throwForce = 0f;
    private float throwTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        mainCamera = Camera.main;
        startPosition = transform.position;

        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = trajectoryPoints;
            trajectoryLine.enabled = false;
        }

        if (forceBar == null)
            forceBar = FindObjectOfType<ForceBar>();
    }

    void Update()
    {
        if (wasThrown)
        {
            CheckForMiss();
            return;
        }

        HandleInput();
    }

    #region Input Handling
    
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = GetMouseWorldPosition();
            if (IsMouseOverPokeball(mousePos)) 
                OnPokeballPressed(mousePos);
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = GetMouseWorldPosition();
            OnPokeballDragged(mousePos);
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = GetMouseWorldPosition();
            OnPokeballReleased(mousePos);
        }
    }
    
    #endregion

    #region Miss Detection (NOVO!)
    
    private void CheckForMiss()
    {
        throwTime += Time.deltaTime;
        if (throwTime > missTimeout)
        {
            Debug.Log("⏰ Miss detectado por TIMEOUT");
            HandleMiss();
            return;
        }

        if (transform.position.y < minYPosition)
        {
            Debug.Log("📉 Miss detectado por POSIÇÃO Y");
            HandleMiss();
            return;
        }
    }

    void OnBecameInvisible()
    {
        if (wasThrown && useCameraDetection)
        {
            Debug.Log("👁️ Miss detectado por CÂMERA");
            HandleMiss();
        }
    }

    private void HandleMiss()
    {
        if (!wasThrown) return;
        
        wasThrown = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        if (gameManager != null)
        {
            gameManager.OnPokeballMissed();
        }
    }
    
    #endregion

    #region Drag & Launch Logic
    
    bool IsMouseOverPokeball(Vector3 mouseWorldPos)
    {
        if (circleCollider == null) return false;
        float distance = Vector2.Distance(mouseWorldPos, transform.position);
        float radius = circleCollider.radius * transform.localScale.x;
        return distance <= radius;
    }

    void OnPokeballPressed(Vector3 mouseWorldPos)
    {
        if (wasThrown) return;
        isDragging = true;
        dragStartPosition = mouseWorldPos;

        if (trajectoryLine != null) 
            trajectoryLine.enabled = true;
        
        forceBar?.Show();
        forceBar?.UpdateForce(0f);
    }

    void OnPokeballDragged(Vector3 mouseWorldPos)
    {
        if (!isDragging || wasThrown) return;
        
        Vector3 dragVector = dragStartPosition - mouseWorldPos;
        if (dragVector.magnitude > maxDragDistance)
            dragVector = dragVector.normalized * maxDragDistance;

        transform.position = startPosition - dragVector;

        if (trajectoryLine != null)
            ShowTrajectory(dragVector);

        if (forceBar != null)
        {
            float normalizedForce = dragVector.magnitude / maxDragDistance;
            forceBar.UpdateForce(normalizedForce);
        }
    }

    void OnPokeballReleased(Vector3 mouseWorldPos)
    {
        if (!isDragging || wasThrown) return;
        
        isDragging = false;
        
        if (trajectoryLine != null) 
            trajectoryLine.enabled = false;
        
        forceBar?.Hide();

        Vector3 dragVector = dragStartPosition - mouseWorldPos;
        float forceMagnitude = Mathf.Min(dragVector.magnitude * forceMultiplier, maxForce);
        throwForce = forceMagnitude;

        LaunchPokeball(dragVector.normalized * forceMagnitude);
    }

    void LaunchPokeball(Vector2 force)
    {
        wasThrown = true;
        throwTime = 0f;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 360f;
        rb.AddForce(force, ForceMode2D.Impulse);
        
        AudioManager.Instance?.PlayThrowSound();
    }

    void ShowTrajectory(Vector3 dragVector)
    {
        if (trajectoryLine == null) return;

        Vector2 velocity = dragVector.normalized * Mathf.Min(dragVector.magnitude * forceMultiplier, maxForce);
        Vector2 position = startPosition;
        float timeStep = 0.1f;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            trajectoryLine.SetPosition(i, position);
            velocity += Physics2D.gravity * rb.gravityScale * timeStep;
            position += velocity * timeStep;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePos);
    }
    
    #endregion

    #region Collision
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!wasThrown) return;
        
        if (collision.CompareTag("PokemonSilhouette") || 
            collision.gameObject.name.Contains("Silhouette"))
        {
            Debug.Log("🎯 ACERTOU o alvo!");
            
            wasThrown = false;
            
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            
            transform.position = collision.transform.position;
            
            if (gameManager != null)
            {
                float accuracy = CalculateAccuracy(collision);
                gameManager.OnPokeballHitTarget(throwForce, accuracy);
            }
        }
    }

    private float CalculateAccuracy(Collider2D targetCollider)
    {
        Vector2 targetCenter = targetCollider.bounds.center;
        Vector2 hitPoint = transform.position;
        
        float distance = Vector2.Distance(hitPoint, targetCenter);
        float maxDistance = targetCollider.bounds.extents.magnitude;
        
        float accuracy = 1f - Mathf.Clamp01(distance / maxDistance);
        
        Debug.Log($"🎯 Precisão: {accuracy:F2}");
        
        return accuracy;
    }
    
    #endregion

    #region Public API
    
    public void SetGameManager(GameManager manager) => gameManager = manager;
    public void SetForceBar(ForceBar bar) => forceBar = bar;

    public void ResetPokeball(Vector3 spawnPosition)
    {
        wasThrown = false;
        isDragging = false;
        throwTime = 0f;
        
        transform.position = spawnPosition;
        startPosition = spawnPosition;
        
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        forceBar?.Hide();
        
        Debug.Log("♻️ Pokébola resetada!");
    }
    
    #endregion

    #region Debug Gizmos
    
    void OnDrawGizmos()
    {
        if (isDragging && !wasThrown)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPosition, maxDragDistance);
            
            if (mainCamera != null)
            {
                Vector3 mousePos = GetMouseWorldPosition();
                Gizmos.color = Color.red;
                Gizmos.DrawLine(startPosition, mousePos);
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-100, minYPosition, 0), new Vector3(100, minYPosition, 0));
    }
    
    #endregion
}