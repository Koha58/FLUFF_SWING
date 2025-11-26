using UnityEngine;

/// <summary>
/// 複数の移動ステップ（方向と距離）を順番に巡回する「動く床（Moving Platform）」の制御スクリプト。
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    // Inspectorで設定するための構造体を定義
    [System.Serializable]
    public struct MovingPlatformStep
    {
        [Tooltip("このフェーズで移動する方向 (Vector2)。normalizedされます。")]
        public Vector2 direction;
        [Tooltip("このフェーズで移動する距離")]
        public float distance;
    }

    [Header("移動設定")]
    [SerializeField]
    private float moveSpeed = 2f; // 移動速度は全ステップ共通

    [Header("移動ステップ設定")]
    [Tooltip("複数の移動フェーズ（方向と距離）を設定します。")]
    [SerializeField]
    private MovingPlatformStep[] moveSteps; // 複数の移動ステップの配列

    private Vector2 currentPos; // 現在のフェーズの始点 (前ステップの終点)
    private Vector2 targetPos;  // 現在のフェーズの終点 (現在の目標地点)
    private int currentStepIndex = 0; // 現在実行中のステップのインデックス

    void Start()
    {
        if (moveSteps == null || moveSteps.Length == 0)
        {
            Debug.LogError("MovingPlatformに移動ステップが設定されていません。");
            enabled = false;
            return;
        }

        // 初期位置を最初のステップの開始点として設定
        currentPos = transform.position;
        // 最初の目標地点を設定
        SetNextTargetPos();
    }

    void Update()
    {
        // 目的地（targetPos）へ一定速度で移動
        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // 目的地に到達したか判定
        // 閾値を小さくし、目標に近づいたら到達とみなす
        if (Vector2.Distance(transform.position, targetPos) < 0.01f)
        {
            // 目標地点にぴったり配置し、次のステップへ切り替える
            transform.position = targetPos;
            AdvanceToNextStep();
        }
    }

    /// <summary>
    /// 次の移動ステップへ進み、目標地点を更新する
    /// </summary>
    private void AdvanceToNextStep()
    {
        // 次のステップのインデックスを計算（巡回させる: 最後のステップの次は 0 に戻る）
        currentStepIndex = (currentStepIndex + 1) % moveSteps.Length;

        // 新しい開始位置は現在の到達位置（targetPos）
        currentPos = targetPos;

        // 次の目標位置を設定
        SetNextTargetPos();
    }

    /// <summary>
    /// 現在のステップに基づいて targetPos を計算する
    /// </summary>
    private void SetNextTargetPos()
    {
        MovingPlatformStep nextStep = moveSteps[currentStepIndex];

        // 現在の開始位置 (currentPos) から、次の方向と距離で目標位置を計算
        // 方向ベクトルを正規化して、距離を掛ける
        targetPos = currentPos + nextStep.direction.normalized * nextStep.distance;
    }

    // プレイヤーの子オブジェクト化ロジック (滑り落ち対策)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // プレイヤーのタグで判定
        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーをこの動く床の子オブジェクトにする
            collision.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // プレイヤーのタグで判定
        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーの親子関係を解除し、シーンルートに戻す
            collision.transform.SetParent(null);

            // 必要に応じてスケール継承の影響をリセットする場合はコメントを外す
            // collision.transform.localScale = Vector3.one; 
        }
    }
}