using UnityEngine;
using UnityEngine.UI;

public class ForceBar : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameObject fillBarObject;
    [SerializeField] private Image fillImage;
    [SerializeField] private Text percentageText;

    [Header("Configurações")]
    [SerializeField] private float maxWidth = 280f;
    [SerializeField] private Color weakColor = Color.red;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color strongColor = Color.green;

    private RectTransform _fillBar;

    private void Awake()
    {
        if (fillBarObject != null)
            _fillBar = fillBarObject.GetComponent<RectTransform>();

        Hide();
    }

    public void Show()
    {
        SetActive(true);
        #if UNITY_EDITOR
            Debug.Log("📊 Barra de força ATIVADA!");
        #endif
    }

    public void Hide()
    {
        SetActive(false);
        #if UNITY_EDITOR
            Debug.Log("📊 Barra de força DESATIVADA!");
        #endif
    }

    public void UpdateForce(float normalizedForce)
    {
        normalizedForce = Mathf.Clamp01(normalizedForce);
        UpdateBarWidth(normalizedForce);
        UpdateBarColor(normalizedForce);
        UpdatePercentageText(normalizedForce);
    }

    private void SetActive(bool state)
    {
        if (gameObject.activeSelf != state)
            gameObject.SetActive(state);
    }

    private void UpdateBarWidth(float normalizedForce)
    {
        if (_fillBar == null) return;

        float newWidth = maxWidth * normalizedForce;
        _fillBar.sizeDelta = new Vector2(newWidth, _fillBar.sizeDelta.y);
    }

    private void UpdateBarColor(float normalizedForce)
    {
        if (fillImage == null) return;

        fillImage.color = normalizedForce switch
        {
            < 0.33f => weakColor,
            < 0.67f => mediumColor,
            _ => strongColor
        };
    }

    private void UpdatePercentageText(float normalizedForce)
    {
        if (percentageText == null) return;

        int percentage = Mathf.RoundToInt(normalizedForce * 100);
        percentageText.text = $"{percentage}%";
    }
}
