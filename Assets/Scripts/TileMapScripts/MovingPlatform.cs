using UnityEngine;

/// <summary>
/// 複数の移動ステップ（方向と距離）を順番に巡回する「動く床（Moving Platform）」の制御スクリプト。
/// - moveSteps で「方向 + 距離」を複数設定し、順番に繰り返す
/// - 到達判定は閾値で行い、到達時に次ステップへ進む
/// - 接触した Player / Enemy を一時的に子オブジェクト化して滑り落ちを防ぐ
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    #region === 設定用構造体 ===

    /// <summary>
    /// 1つの移動フェーズ（方向と距離）。
    /// direction は内部で normalized して距離を掛ける。
    /// </summary>
    [System.Serializable]
    public struct MovingPlatformStep
    {
        [Tooltip("このフェーズで移動する方向 (Vector2)。内部で normalized されます。")]
        public Vector2 direction;

        [Tooltip("このフェーズで移動する距離（ワールド単位）")]
        public float distance;
    }

    #endregion


    #region === Inspector 設定 ===

    [Header("移動設定")]
    [Tooltip("移動速度（全ステップ共通）")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("移動ステップ設定")]
    [Tooltip("複数の移動フェーズ（方向と距離）を設定します。最後まで行ったら最初に戻って巡回します。")]
    [SerializeField] private MovingPlatformStep[] moveSteps;

    [Header("到達判定")]
    [Tooltip("目標地点に到達したとみなす距離（小さすぎると到達しない/振動の原因になることがあります）")]
    [SerializeField] private float arriveThreshold = 0.01f;

    [Header("子オブジェクト化（滑り落ち対策）")]
    [Tooltip("Enemy を子にした直後、見た目が埋まる場合の持ち上げ量（ワールドY方向）")]
    [SerializeField] private float enemyParentingLiftY = 1.1f;

    [Header("タグ名")]
    [Tooltip("プレイヤー判定に使うタグ名")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("敵判定に使うタグ名")]
    [SerializeField] private string enemyTag = "Enemy";

    #endregion


    #region === 内部状態（宣言順：参照→状態→インデックス） ===

    /// <summary>現在のステップ開始地点（前ステップの終点）</summary>
    private Vector2 _currentPos;

    /// <summary>現在の目標地点（このステップの終点）</summary>
    private Vector2 _targetPos;

    /// <summary>現在実行中のステップ番号（moveSteps配列のindex）</summary>
    private int _currentStepIndex;

    #endregion


    #region === Unity Events ===

    private void Start()
    {
        // ステップが未設定なら動かせないので停止
        if (moveSteps == null || moveSteps.Length == 0)
        {
            Debug.LogError("[MovingPlatform] moveSteps が設定されていません。");
            enabled = false;
            return;
        }

        // 初期ステップ開始点は現在位置
        _currentPos = transform.position;

        // 最初の目標地点を計算
        _currentStepIndex = 0;
        SetNextTargetPos();
    }

    private void Update()
    {
        // 目標地点へ一定速度で移動（フレームレート依存を避けるため deltaTime を掛ける）
        transform.position = Vector2.MoveTowards(transform.position, _targetPos, moveSpeed * Time.deltaTime);

        // 目標に十分近づいたら「到達」とみなし、次のステップへ
        if (Vector2.Distance(transform.position, _targetPos) < arriveThreshold)
        {
            // 位置をピッタリ合わせて誤差を潰す（蓄積でズレるのを防ぐ）
            transform.position = _targetPos;

            // 次のステップへ進む
            AdvanceToNextStep();
        }
    }

    #endregion


    #region === 移動ステップ処理 ===

    /// <summary>
    /// 次の移動ステップへ進み、開始点と目標点を更新する。
    /// 最後まで行ったら最初に戻る（巡回）。
    /// </summary>
    private void AdvanceToNextStep()
    {
        // 次のステップ番号（巡回）
        _currentStepIndex = (_currentStepIndex + 1) % moveSteps.Length;

        // 新しい開始点は「今到達した地点」
        _currentPos = _targetPos;

        // 新しい目標地点を計算
        SetNextTargetPos();
    }

    /// <summary>
    /// 現在のステップ情報から「次の目標地点」を計算する。
    /// </summary>
    private void SetNextTargetPos()
    {
        MovingPlatformStep step = moveSteps[_currentStepIndex];

        // 方向がゼロベクトルのときは normalize で (0,0) のままなので、結果的に動かない
        // （必要ならここで警告を出してもOK）
        Vector2 dir = step.direction.normalized;

        // 開始点 + (方向 * 距離) が目標地点
        _targetPos = _currentPos + dir * step.distance;
    }

    #endregion


    #region === 接触時：子オブジェクト化（滑り落ち対策） ===

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Player：床の子にすることで、床の移動に追従しやすくする（滑り落ち/ズレ対策）
        if (collision.gameObject.CompareTag(playerTag))
        {
            collision.transform.SetParent(transform);
            return;
        }

        // Enemy：同様に床の子にする。さらに見た目が埋まる場合は少し持ち上げる
        if (collision.gameObject.CompareTag(enemyTag))
        {
            Transform enemy = collision.transform;

            enemy.SetParent(transform);

            // 親子付け直後に地形へ食い込む場合の応急処置（必要なければ 0 にする）
            enemy.position += Vector3.up * enemyParentingLiftY;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Player：床から降りたら親子関係を解除してシーン直下に戻す
        if (collision.gameObject.CompareTag(playerTag))
        {
            collision.transform.SetParent(null);
            return;
        }

        // Enemy：同様に解除
        if (collision.gameObject.CompareTag(enemyTag))
        {
            collision.transform.SetParent(null);
        }
    }

    #endregion
}
