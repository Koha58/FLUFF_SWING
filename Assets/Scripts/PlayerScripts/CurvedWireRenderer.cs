using UnityEngine;

/// <summary>
/// 曲線ワイヤーを LineRenderer で描画するクラス。
/// 始点と終点の間に放物線状のカーブを描画する。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class CurvedWireRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [SerializeField, Range(2, 20)]
    // 曲線を構成する頂点の数（最低2以上）
    private int segmentCount = 10;

    // ワイヤーの始点と終点（外部から設定される）
    public Vector3 StartPoint { get; set; }
    public Vector3 EndPoint { get; set; }

    // 曲線の高さ（放物線の湾曲の強さ）
    public float curveHeight = 1f;

    private void Awake()
    {
        // LineRenderer の初期設定
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount;
    }

    private void Update()
    {
        // 頂点数が変更された場合に更新
        if (lineRenderer.positionCount != segmentCount)
            lineRenderer.positionCount = segmentCount;

        // カーブ描画を実行
        DrawCurve(StartPoint, EndPoint);
    }

    /// <summary>
    /// 始点と終点を結ぶ放物線状のカーブを描画します。
    /// </summary>
    /// <param name="start">ワイヤーの始点</param>
    /// <param name="end">ワイヤーの終点</param>
    private void DrawCurve(Vector3 start, Vector3 end)
    {
        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1); // 0〜1の区間値
            Vector3 point = Vector3.Lerp(start, end, t); // 始点と終点の線形補間

            // 放物線状に高さを加算（t*(1-t)により中間が最も高くなる）
            point.y += curveHeight * 4f * t * (1 - t);

            // 計算された点を LineRenderer にセット
            lineRenderer.SetPosition(i, point);
        }
    }

    /// <summary>
    /// ワイヤーの表示／非表示を切り替えます。
    /// </summary>
    /// <param name="visible">trueで表示、falseで非表示</param>
    public void SetVisible(bool visible)
    {
        if (lineRenderer == null)
        {
            Debug.LogError("lineRenderer is null!");
            return;
        }
        lineRenderer.enabled = visible;
    }
}