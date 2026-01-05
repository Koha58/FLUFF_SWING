using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class LocalizedPositionX : MonoBehaviour
{
    [SerializeField] private float japaneseX = 113f;
    [SerializeField] private float englishX = 238f;

    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance == null) return;

        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        OnLanguageChanged(LocalizationManager.Instance.CurrentLanguage);
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance == null) return;
        LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(Language lang)
    {
        var pos = _rect.anchoredPosition;

        switch (lang)
        {
            case Language.English:
                pos.x = englishX;
                break;
            case Language.Japanese:
            default:
                pos.x = japaneseX;
                break;
        }

        _rect.anchoredPosition = pos;
    }
}
