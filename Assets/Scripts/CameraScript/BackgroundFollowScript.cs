using UnityEngine;

/// <summary>
/// プレイヤーの座標に基づいてカメラ（背景）を追従させるスクリプト。
/// X軸は制限なし。Y軸はデッドゾーン追従で、ワールド座標の下限(minY)クランプを行います。
/// </summary>
public class BackgroundFollowScript : MonoBehaviour
{
    // =================================================================================
    // 📢 公開設定フィールド (Inspectorで設定)
    // =================================================================================

    [Header("▼ 追従対象とオフセット")]
    [SerializeField] private WireActionScript wireActionScript;
    [SerializeField] private PlayerMove playerMove;
    public Transform player;
    public float horizontalOffset = 2f; // X軸のプレイヤーからのオフセット距離
    public float smoothTime = 0.3f;     // カメラ移動のスムージング時間

    [Header("▼ デッドゾーン設定")]
    [Tooltip("カメラが上下に動き出すまでの許容範囲（カメラの中心からの距離）")]
    public float deadZoneY = 2f;

    [Header("▼ カメラの境界設定")]
    [Tooltip("カメラが下方向に移動できるワールド座標の最小値")]
    public float minY = 0f; // カメラのY座標の下限

    // =================================================================================
    // 🛡️ 内部状態変数 (実行時に変化)
    // =================================================================================

    private Vector3 velocity = Vector3.zero; // SmoothDamp用の速度参照変数
    private bool wasConnected = false;      // 前フレームのワイヤー接続状態
    private float delayDuration = 0.3f;     // ワイヤー切断後のカメラ停止遅延時間
    private float delayTimer = -1f;         // 遅延タイマー

    // =================================================================================
    // Unity イベント関数
    // =================================================================================

    void LateUpdate()
    {
        if (player == null || wireActionScript == null || playerMove == null) return;

        bool isConnected = wireActionScript.IsConnected;
        Vector3 currentPos = transform.position;

        // ワイヤー切断直後の処理(遅延スタート)
        if (wasConnected && !isConnected)
        {
            delayTimer = delayDuration;
        }

        float targetX;
        float targetY = currentPos.y;

        // --- ワイヤー使用時の追従ロジック ---
        if (isConnected)
        {
            Vector2 wirePos = wireActionScript.HookedPosition;
            targetX = wirePos.x;
            targetY = wirePos.y - 1; // ワイヤーのフック位置から少し下にターゲットを設定
        }
        // --- ワイヤー不使用時の追従ロジック ---
        else
        {
            // 動く床に乗っているときなどに、カメラが動かないようにする遅延タイマー
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;
                wasConnected = isConnected;
                return;
            }

            // 1. X軸の追従
            targetX = player.position.x + horizontalOffset;

            // 2. Y軸の追従 (デッドゾーンロジックを適用)
            float deltaY = player.position.y - currentPos.y;

            if (deltaY > deadZoneY)
            {
                // プレイヤーがデッドゾーンの上限を超えた場合、カメラを上昇させる
                targetY = player.position.y - deadZoneY;
            }
            else if (deltaY < -deadZoneY)
            {
                // プレイヤーがデッドゾーンの下限を超えた場合、カメラを下降させる
                targetY = player.position.y + deadZoneY;
            }
            else
            {
                // デッドゾーン内ではY軸を動かさない
                targetY = currentPos.y;
            }
        }

        // 現在のカメラ位置を基にターゲット位置へスムーズに移動
        Vector3 targetPos = new Vector3(targetX, targetY, currentPos.z);
        Vector3 newCameraPos = Vector3.SmoothDamp(currentPos, targetPos, ref velocity, smoothTime);

        // Y軸の下限クランプを適用
        // newCameraPos.yがminYより小さくならないようにする
        newCameraPos.y = Mathf.Max(newCameraPos.y, minY);

        // 制限のある最終位置を適用
        transform.position = newCameraPos;

        // 現在の状態を保存
        wasConnected = isConnected;
    }
}