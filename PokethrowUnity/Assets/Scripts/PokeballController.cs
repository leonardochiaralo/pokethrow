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
    [SerializeField] private LineRenderer trajectoryLine;
    public int trajectoryPoints = 20;
    
    private GameManager gameManager;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    
    private Vector3 startPosition;
    private Vector3 dragStartPosition;
    private bool isDragging = false;
    private bool wasThrown = false;
    private float throwForce = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        
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
        {
            forceBar = FindObjectOfType<ForceBar>();
            if (forceBar != null) Debug.Log("✅ ForceBar encontrada no Awake!");
        }

        Debug.Log("⚪ PokeballController inicializado!");
    }

    void Update()
    {
        if (wasThrown) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = GetMouseWorldPosition();
            if (IsMouseOverPokeball(mousePos)) OnPokeballPressed(mousePos);
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

        if (trajectoryLine != null) trajectoryLine.enabled = true;

        if (forceBar != null)
        {
            forceBar.Show();
            forceBar.UpdateForce(0f);
        }
    }

    void OnPokeballDragged(Vector3 mouseWorldPos)
    {
        if (!isDragging || wasThrown) return;

        Vector3 dragVector = dragStartPosition - mouseWorldPos;
        if (dragVector.magnitude > maxDragDistance) dragVector = dragVector.normalized * maxDragDistance;

        transform.position = startPosition - dragVector;

        if (trajectoryLine != null) ShowTrajectory(dragVector);
        if (forceBar != null) forceBar.UpdateForce(dragVector.magnitude / maxDragDistance);
    }

    void OnPokeballReleased(Vector3 mouseWorldPos)
    {
        if (!isDragging || wasThrown) return;

        isDragging = false;

        if (trajectoryLine != null) trajectoryLine.enabled = false;
        if (forceBar != null) forceBar.Hide();

        Vector3 dragVector = dragStartPosition - mouseWorldPos;
        float forceMagnitude = Mathf.Min(dragVector.magnitude * forceMultiplier, maxForce);
        throwForce = forceMagnitude;

        LaunchPokeball(dragVector.normalized * forceMagnitude);
    }

    void LaunchPokeball(Vector2 force)
    {
        wasThrown = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.AddForce(force, ForceMode2D.Impulse);
        rb.angularVelocity = 360f;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayThrowSound();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!wasThrown) return;

        if (collision.CompareTag("PokemonSilhouette") || collision.gameObject.name.Contains("Silhouette"))
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayHitSound();

            float accuracy = CalculateAccuracy(collision);
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            if (gameManager != null) gameManager.OnPokeballHitTarget(throwForce, accuracy);

            transform.position = collision.transform.position;
        }
    }

    float CalculateAccuracy(Collider2D targetCollider)
    {
        Vector2 targetCenter = targetCollider.bounds.center;
        Vector2 hitPoint = transform.position;
        float distance = Vector2.Distance(hitPoint, targetCenter);
        float maxDistance = targetCollider.bounds.extents.magnitude;
        return 1f - Mathf.Clamp01(distance / maxDistance);
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

    public void SetGameManager(GameManager manager) => gameManager = manager;

    public void SetForceBar(ForceBar bar)
    {
        forceBar = bar;
        Debug.Log("✅ ForceBar configurada via setter!");
    }

    void OnBecameInvisible()
    {
        if (wasThrown && gameManager != null)
        {
            gameManager.OnPokeballHitTarget(0f, 0f);
        }
    }
}
