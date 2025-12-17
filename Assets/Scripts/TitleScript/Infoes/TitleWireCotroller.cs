using UnityEngine;

/// <summary>
/// 手から針（ワイヤー先端）に向かってベジェ曲線を使って
/// 「しなやかなワイヤー」を描画するクラス。
/// チュートリアルなどで見せるデモ用のワイヤー演出に使用。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class TitleWireCotroller : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Transform playerHand; // ワイヤーの始点（プレイヤーの手の位置）
    [SerializeField] private Transform needle;     // ワイヤーの終点（針やフックの位置）

    [Header("パラメータ")]
    [SerializeField] private float bendAmount = 0.5f;   // ワイヤーのしなりの大きさ（曲げ量）
    [SerializeField, Range(6, 64)] private int segmentCount = 20; // ベジェ曲線を分割して描画する点の数
    [SerializeField] private float gravityInfluence = 0.5f; // 重力方向にしなりを寄せる割合

    private LineRenderer line; // ワイヤー描画用のLineRenderer

    private void Awake()
    {
        // LineRendererを取得
        line = GetComponent<LineRenderer>();

        // 初期状態では非表示にしておく（必要なタイミングで有効化）
        line.enabled = false;
    }

    private void LateUpdate()
    {
        // 無効化中なら処理しない
        if (!line.enabled) return;

        // --- 始点と終点の座標を取得 ---
        Vector3 start = playerHand.position; // 手の位置
        Vector3 end = needle.position;       // 針の位置

        // --- 中間点（ベジェ曲線の制御点）を計算 ---
        Vector3 mid = (start + end) * 0.5f; // 基本の中間点（単純な中間位置）

        // 手から針への方向ベクトル
        Vector3 dir = (end - start).normalized;

        // 重力方向ベクトル
        Vector3 gravityDir = Vector3.down;

        // 「手→針」方向に対して直角の方向を求める
        // （これがしなり方向の基準になる）
        Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized;

        // そのしなり方向を、重力方向に少し寄せて自然な垂れ感を出す
        normal = Vector3.Lerp(normal, gravityDir, gravityInfluence).normalized;

        // 針が手より上にある場合は、しなり方向を反転させて上向きに曲げる
        float verticalSign = Mathf.Sign(end.y - start.y);
        mid += normal * bendAmount * verticalSign;

        // --- ベジェ曲線でワイヤーの各点を補間 ---
        line.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            // t は 0 → 1 の補間パラメータ
            float t = i / (segmentCount - 1f);

            // ベジェ曲線上の座標を求めてLineRendererに設定
            line.SetPosition(i, GetQuadraticBezierPoint(start, mid, end, t));
        }
    }

    /// <summary>
    /// 2次ベジェ曲線の補間計算
    /// p0: 始点, p1: 制御点（中間点）, p2: 終点
    /// t: 0〜1 の補間値
    /// </summary>
    private Vector3 GetQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        // 2次ベジェ曲線の公式：
        // B(t) = (1-t)^2 * p0 + 2(1-t)t * p1 + t^2 * p2
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }
}
