using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// クレジットシーンで使用される単一の画像オブジェクトの表示・非表示を制御するコントローラー。
/// スクロール位置に基づいて、透明度（アルファ値）を調整し、画像をフェードイン/アウトさせる。
/// 画像の位置は固定であり、移動は行わない（スライドイン処理は含まない）。
/// CreditScrollerからリスト形式で管理され、UpdateImageStateメソッドにより毎フレーム更新される。
/// </summary>
public class CreditImageController : MonoBehaviour
{
    // === プライベートフィールド ===
    private Image _image;
    private RectTransform _rectTransform;

    // === インスペクター設定: Visibility Control ===
    [Header("Visibility Control")]
    [Tooltip("フェードイン/アウトにかかるスクロール距離。この距離内で透明度が0から1に変化する。")]
    [SerializeField] float fadeDistance = 300f;

    [Tooltip("コンテンツY位置がこの値に達するとフェードインが完了する（最大透明度に達する点）。")]
    [SerializeField] float startContentYOffset = 0f;

    [Tooltip("コンテンツY位置がこの値に達するとフェードアウトが開始する（透明度が減少し始める点）。")]
    [SerializeField] float endContentYOffset = 1000f;

    // 初期位置を保持（現在は移動処理がないため、主にデバッグ用）
    private Vector2 _initialPosition;

    void Awake()
    {
        // 必要なコンポーネントの取得
        _image = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();

        // Imageコンポーネントがない場合の警告
        if (_image == null)
        {
            Debug.LogError("CreditImageController requires an Image component on the same GameObject.");
            enabled = false;
        }
    }

    /// <summary>
    /// CreditScrollerの開始時に呼ばれる初期化処理。
    /// </summary>
    /// <param name="viewW">Viewportの幅 (未使用)</param>
    /// <param name="viewH">Viewportの高さ (未使用)</param>
    /// <param name="contentHeight">コンテンツ全体の高さ (未使用)</param>
    public void Initialize(float viewW, float viewH, float contentHeight)
    {
        // 初期透明度を0に設定し、画像を非表示にする
        Color c = _image.color;
        c.a = 0f;
        _image.color = c;

        // 初期位置を保存
        if (_rectTransform != null)
        {
            _initialPosition = _rectTransform.anchoredPosition;
        }
    }

    /// <summary>
    /// クレジットスクローラーから毎フレーム呼び出される更新メソッド。
    /// 現在のコンテンツスクロール位置 (currentContentY) に基づいて画像の透明度を制御する。
    /// </summary>
    /// <param name="currentContentY">コンテンツの現在のアンカー付きY座標 (Contentの下端がViewport下端でY=0)</param>
    /// <param name="contentHeight">コンテンツ全体の高さ (未使用)</param>
    public void UpdateImageState(float currentContentY, float contentHeight)
    {
        if (_image == null || !enabled) return;

        // ====================================================================
        // 1. フェードインの比率を計算 (表示開始ロジック)
        // ====================================================================

        // フェードインの開始点（透明度0になるY座標）
        float contentStartPoint = startContentYOffset - fadeDistance;
        // フェードインの終了点（透明度1になるY座標）
        float contentEndPoint = startContentYOffset;
        float fadeInRatio = 1f;

        if (currentContentY < contentStartPoint)
        {
            // スクロール開始点より手前: 完全に見えない
            fadeInRatio = 0f;
        }
        else if (currentContentY < contentEndPoint)
        {
            // スクロール開始点と終了点の間: フェードイン中
            // (現在のY - 開始点) / フェード距離
            fadeInRatio = (currentContentY - contentStartPoint) / fadeDistance;
        }
        // else: フェードイン完了 (1f)

        // ====================================================================
        // 2. フェードアウトの比率を計算 (表示終了ロジック)
        // ====================================================================

        // フェードアウトの開始点（透明度1を維持するY座標）
        float contentOutStartPoint = endContentYOffset;
        // フェードアウトの終了点（透明度0になるY座標）
        float contentOutEndPoint = endContentYOffset + fadeDistance;

        float fadeOutRatio = 1f;
        if (currentContentY > contentOutEndPoint)
        {
            // スクロール終了点より奥: 完全に見えない
            fadeOutRatio = 0f;
        }
        else if (currentContentY > contentOutStartPoint)
        {
            // フェードアウト開始点と終了点の間: フェードアウト中
            // 1 - (現在のY - フェードアウト開始点) / フェード距離
            fadeOutRatio = 1f - ((currentContentY - contentOutStartPoint) / fadeDistance);
        }
        // else: フェードアウト未開始 (1f)

        // ====================================================================
        // 3. 総合的な透明度の設定
        // ====================================================================

        // フェードインとフェードアウトの小さい方の値を採用し、最終的なアルファ値とする
        // (例: フェードイン完了前にフェードアウトが始まった場合、透明度が下がり始める)
        float alpha = Mathf.Clamp01(Mathf.Min(fadeInRatio, fadeOutRatio));

        // 画像にアルファ値を適用
        Color c = _image.color;
        c.a = alpha;
        _image.color = c;

        // 4. 位置調整（移動処理なし）
        // 位置は固定（_initialPosition）に保たれる。
    }
}