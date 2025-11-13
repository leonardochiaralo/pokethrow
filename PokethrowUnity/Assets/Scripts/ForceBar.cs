using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla a barra de força visual
/// </summary>
public class ForceBar : MonoBehaviour
{
    [Header("Referências")]
    public GameObject fillBarObject;   // GameObject da barra ⬅️ MUDOU!
    public Image fillImage;            // Imagem da barra (para mudar cor)
    public Text percentageText;        // Texto de porcentagem (opcional)

    [Header("Configurações")]
    public float maxWidth = 280f;      // Largura máxima da barra
    public Color weakColor = Color.red;      // Cor fraca (0-33%)
    public Color mediumColor = Color.yellow; // Cor média (34-66%)
    public Color strongColor = Color.green;  // Cor forte (67-100%)

    private RectTransform fillBar;     // Cache do RectTransform

    void Start()
    {
        // Pega o RectTransform do GameObject
        if (fillBarObject != null)
        {
            fillBar = fillBarObject.GetComponent<RectTransform>();
        }

        // Começa oculto
        Hide();
    }

    /// <summary>
    /// Mostra a barra de força
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        Debug.Log("📊 Barra de força ATIVADA!");
    }

    /// <summary>
    /// Oculta a barra de força
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        Debug.Log("📊 Barra de força DESATIVADA!");
    }

    /// <summary>
    /// Atualiza a barra com base na força (0 a 1)
    /// </summary>
    public void UpdateForce(float normalizedForce)
    {
        // Normaliza entre 0 e 1
        normalizedForce = Mathf.Clamp01(normalizedForce);

        // Atualiza largura da barra
        if (fillBar != null)
        {
            fillBar.sizeDelta = new Vector2(maxWidth * normalizedForce, fillBar.sizeDelta.y);
        }

        // Atualiza cor baseada na força
        if (fillImage != null)
        {
            if (normalizedForce < 0.33f)
            {
                fillImage.color = weakColor;
            }
            else if (normalizedForce < 0.67f)
            {
                fillImage.color = mediumColor;
            }
            else
            {
                fillImage.color = strongColor;
            }
        }

        // Atualiza texto (se existir)
        if (percentageText != null)
        {
            int percentage = Mathf.RoundToInt(normalizedForce * 100);
            percentageText.text = $"{percentage}%";
        }
    }
}