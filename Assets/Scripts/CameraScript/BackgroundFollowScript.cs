using UnityEngine;

/// <summary>
/// プレイヤーの座標に基づいてカメラ（背景）を追従させるシンプルなスクリプト。
/// 主に動く床などの状況で、プレイヤーの絶対座標の変化に追従するために使用します。
/// </summary>
public class BackgroundFollowScript : MonoBehaviour
{
    /// <summary>
    /// ワイヤーアクションの状態（接続状態など）を管理するスクリプト
    /// </summary>
    [SerializeField]
    private WireActionScript wireActionScript;

    /// <summary>
    /// プレイヤーの動きを管理するスクリプト
    /// </summary>
    [SerializeField]
    private PlayerMove playerMove;

    public Transform player;

    /// <summary>
    /// プレイヤーからの横方向のオフセット
    /// </summary>
    public float horizontalOffset = 2f;

    /// <summary>
    /// スムージングの時間
    /// (数が大きいほどゆっくり移動)
    /// </summary>
    public float smoothTime = 0.3f;

    /// <summary>
    /// SmoothDamp用
    /// </summary>
    private Vector3 velocity = Vector3.zero;

    /// <summary>
    /// 前フレームのisConnectedの状態
    /// </summary>
    private bool wasConnected = false;

    /// <summary>
    /// 遅延の持続時間(秒)
    /// (数が大きいほど遅延時間が長くなる)
    /// </summary>
    private float delayDuration = 0.3f;

    /// <summary>
    /// 遅延タイマー(0未満なら未使用)
    /// </summary>
    private float delayTimer = -1f;

    [Header("デッドゾーン設定")]
    [Tooltip("カメラが上下に動き出すまでの許容範囲（カメラの中心からの距離）")]
    public float deadZoneY = 2f;

    void LateUpdate()
    {
        if (player == null || wireActionScript == null || playerMove == null) return;

        bool isConnected = wireActionScript.IsConnected;
        bool isGrounded = playerMove.IsGrounded;

        Vector3 currentPos = transform.position;

        // ワイヤー切断直後の処理(遅延スタート)
        if (wasConnected && !isConnected)
        {
            delayTimer = delayDuration;
        }

        // ターゲット位置を決定
        float targetX;
        // Y軸は一旦現在のカメラ位置に固定
        float targetY = currentPos.y;

        // ワイヤー使用時
        if (isConnected)
        {
            // ワイヤー設置場所の座標を持ってくる
            Vector2 wirePos = wireActionScript.HookedPosition;

            targetX = wirePos.x;
            targetY = wirePos.y - 1; // ワイヤーポイントを追従
        }
        // ワイヤー不使用時
        else
        {
            // 動く床に乗っているときなどに、カメラが動かないようにする遅延タイマー
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;

                // 遅延中はカメラ移動スキップ
                wasConnected = isConnected;
                return;
            }

            // 1. X軸の追従（左右の追従は維持）
            targetX = player.position.x + horizontalOffset;

            // 2. Y軸の追従 (デッドゾーンロジックを適用)
            // プレイヤーとカメラの中心のY軸距離を計算
            float deltaY = player.position.y - currentPos.y;

            // --- プレイヤーがデッドゾーンの上限を超えた場合 ---
            if (deltaY > deadZoneY)
            {
                // プレイヤーがデッドゾーン境界（currentPos.y + deadZoneY）に位置するようにターゲットYを設定
                targetY = player.position.y - deadZoneY;
            }
            // --- プレイヤーがデッドゾーンの下限を超えた場合 ---
            else if (deltaY < -deadZoneY)
            {
                // プレイヤーがデッドゾーン境界（currentPos.y - deadZoneY）に位置するようにターゲットYを設定
                targetY = player.position.y + deadZoneY;
            }
            // --- デッドゾーン内（-deadZoneY から +deadZoneY の間）の場合 ---
            else
            {
                // targetY は currentPos.y のまま維持される（カメラは動かない）
                targetY = currentPos.y;
            }
        }

        // 現在のカメラ位置を基にターゲット位置へスムーズに移動
        Vector3 targetPos = new Vector3(targetX, targetY, currentPos.z);

        transform.position = Vector3.SmoothDamp(currentPos, targetPos, ref velocity, smoothTime);

        // 現在の状態を保存
        wasConnected = isConnected;
    }
}