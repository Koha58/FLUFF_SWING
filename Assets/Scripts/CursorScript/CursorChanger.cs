using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// マウスの位置にUIのImageを追従させることで、カスタムカーソルを実現するクラス。
/// CanvasのImageをカーソルとして使い、サイズやアニメーションの自由度を高められる。
/// </summary>
public class CustomCursor : MonoBehaviour
{
    // UI上のカーソル画像（Imageコンポーネント）をInspectorでアタッチする
    [SerializeField] private Image cursorImage;

    // マウスクリックの基準位置（ホットスポット）を調整するためのオフセット
    [SerializeField] private Vector2 offset = Vector2.zero;

    void Start()
    {
        // OSデフォルトのカーソルを非表示にする（Imageカーソルのみを表示）
        Cursor.visible = false;
    }

    void Update()
    {
        // マウスのスクリーン座標をCanvasのローカル座標（anchoredPosition）に変換
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorImage.canvas.transform as RectTransform, // 対象のCanvas
            Input.mousePosition,                           // 現在のマウス座標（スクリーン座標）
            cursorImage.canvas.worldCamera,                // カメラ（OverlayならnullでもOK）
            out pos                                        // 変換後の座標
        );

        // UIカーソルをマウス位置に移動（オフセットも考慮）
        cursorImage.rectTransform.anchoredPosition = pos + offset;
    }
}

