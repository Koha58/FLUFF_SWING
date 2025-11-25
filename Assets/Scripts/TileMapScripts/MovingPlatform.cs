using UnityEngine;

/// <summary>
/// 一定区間を往復する「動く床（Moving Platform）」の制御スクリプト。
/// 指定した方向に決められた距離だけ移動し、端に着いたら自動で折り返す。
/// Tilemap ではなく通常のオブジェクト（SpriteやTilemapCollider付きObject）に使用。
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    private Vector2 moveDirection = Vector2.right;
    // 移動する方向（右なら (1,0)、上なら (0,1)）
    // 斜め方向も可能（例: (1,1)）

    [SerializeField]
    private float moveDistance = 3f;
    // 往復する距離

    [SerializeField]
    private float moveSpeed = 2f;
    // 移動速度（1秒あたりの移動距離）

    private Vector2 startPos;
    // スタート地点（最初の位置）

    private Vector2 targetPos;
    // 目的の位置（スタート地点 + 移動方向 × 距離）

    private bool movingToTarget = true;
    // true：目的地へ移動中、false：スタート地点へ戻り中

    void Start()
    {
        // 初期位置を記録
        startPos = transform.position;

        // 指定方向へ「距離分」先の目標位置を計算
        targetPos = startPos + moveDirection.normalized * moveDistance;
        // normalized で方向の長さを1に揃えて、距離 × 方向 に変換
    }

    void Update()
    {
        // 現在位置
        Vector2 currentPos = transform.position;

        // 今向かっている目的地（折り返し地点またはスタート地点）
        Vector2 destination = movingToTarget ? targetPos : startPos;

        // フレームごとに目的地へ一定速度で移動
        transform.position = Vector2.MoveTowards(currentPos, destination, moveSpeed * Time.deltaTime);

        // 目的地との距離が十分近ければ到達とみなし、方向を反転
        if (Vector2.Distance(transform.position, destination) < 0.01f)
        {
            movingToTarget = !movingToTarget;
        }
    }
}
