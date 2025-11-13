using UnityEngine;

/// <summary>
/// プレイヤーの手からカーソル方向に「予測ライン（ワイヤーの照準線）」を描画するスクリプト。
/// 実際にワイヤーを発射する前に、方向を視覚的に示す用途に使う。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class TutorialWirePredictionLine : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Transform playerHand; // 始点（プレイヤーの手など、ワイヤー発射位置）
    [SerializeField] private Transform targetPoint; // 終点（マウスカーソルや狙い位置）

    [Header("設定")]
    [SerializeField] private float maxLength = 5f; // ラインの最大表示距離

    private LineRenderer line; // ライン描画コンポーネント

    private void Awake()
    {
        // LineRendererを取得
        line = GetComponent<LineRenderer>();

        // 初期は非表示（ワイヤーを狙う時だけ有効化する）
        line.enabled = false;

        // ラインは2点（始点と終点）で構成
        line.positionCount = 2;

        // ワールド座標で位置指定（ローカル空間ではなく）
        line.useWorldSpace = true;

        // ラインの太さ（見た目用の基本設定）
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;
    }

    private void LateUpdate()
    {
        // 非表示中なら更新しない
        if (!line.enabled) return;

        // --- 始点と終点の座標を取得 ---
        Vector3 start = playerHand.position;
        Vector3 end = targetPoint.position;

        // 手からカーソル方向へのベクトル
        Vector3 dir = end - start;

        // --- 最大長さを超えた場合は制限をかける ---
        if (dir.magnitude > maxLength)
        {
            // 向きはそのままに、長さだけ制限
            dir = dir.normalized * maxLength;
        }

        // 長さ制限後の終点を再計算
        end = start + dir;

        // --- LineRendererに位置を設定 ---
        line.SetPosition(0, start); // 始点
        line.SetPosition(1, end);   // 終点
    }
}
