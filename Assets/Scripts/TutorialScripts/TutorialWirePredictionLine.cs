using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TutorialWirePredictionLine : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Transform playerHand; // 始点（プレイヤーの手など）
    [SerializeField] private Transform targetPoint; // 終点（カーソル）

    [Header("設定")]
    [SerializeField] private float maxLength = 5f; // 表示最大距離

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = false; // 初期は非表示
        line.positionCount = 2;
        line.useWorldSpace = true;

        // ベーシック設定（見た目調整）
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;
    }

    private void LateUpdate()
    {
        if (!line.enabled) return;

        Vector3 start = playerHand.position;
        Vector3 end = targetPoint.position;
        Vector3 dir = (end - start);

        // 長さ制限
        if (dir.magnitude > maxLength)
            dir = dir.normalized * maxLength;

        end = start + dir;

        // 線の更新
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}
