using UnityEngine;

/// <summary>
/// 言語ごとに UI の X 座標（anchoredPosition.x）を切り替えるコンポーネント
/// 
/// 日本語と英語で文字数や見た目幅が異なる場合の
/// レイアウト微調整用
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class LocalizedPositionX : MonoBehaviour
{
    // 日本語表示時の X 座標
    [SerializeField] private float japaneseX = 113f;

    // 英語表示時の X 座標
    [SerializeField] private float englishX = 238f;

    // 対象となる RectTransform
    private RectTransform _rect;

    /// <summary>
    /// 初期化
    /// RectTransform をキャッシュする
    /// </summary>
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// オブジェクト有効化時に呼ばれる
    /// 言語変更イベントを購読し、現在言語を即反映する
    /// </summary>
    private void OnEnable()
    {
        // LocalizationManager がまだ生成されていない場合は何もしない
        if (LocalizationManager.Instance == null) return;

        // 言語変更イベント購読
        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;

        // 現在の言語を即反映
        OnLanguageChanged(LocalizationManager.Instance.CurrentLanguage);
    }

    /// <summary>
    /// オブジェクト無効化時に呼ばれる
    /// イベント購読を解除する
    /// </summary>
    private void OnDisable()
    {
        if (LocalizationManager.Instance == null) return;

        LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    /// <summary>
    /// 言語変更時に呼ばれ、X 座標を切り替える
    /// </summary>
    private void OnLanguageChanged(Language lang)
    {
        // 現在の anchoredPosition を取得
        var pos = _rect.anchoredPosition;

        // 言語ごとに X 座標を変更
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

        // 座標を反映
        _rect.anchoredPosition = pos;
    }
}
