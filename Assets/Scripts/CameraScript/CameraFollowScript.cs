using Unity.VisualScripting;
using UnityEngine;

public class CameraFollowScript : MonoBehaviour
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
    public float smoothTime = 0.5f;

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
        float targetY;

        // ワイヤー使用時
        if (isConnected)
        {
            // ワイヤー設置場所の座標を持ってくる
            Vector2 wirePos = wireActionScript.HookedPosition;

            targetX = wirePos.x;
            targetY = wirePos.y-1;
        }
        // ワイヤー不使用時
        else
        {
            if (!isGrounded)
            {

            }
            // 遅延タイマー中ならその場で静止
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;

                // 遅延中はカメラ移動スキップ
                wasConnected = isConnected;
                return;
            }

            targetX = player.position.x + horizontalOffset;
            targetY = player.position.y + 1;
        }

        // 現在のカメラ位置を基にターゲット位置へスムーズに移動
        Vector3 targetPos = new Vector3(targetX, targetY, currentPos.z);

        transform.position = Vector3.SmoothDamp(currentPos, targetPos, ref velocity, smoothTime);

        // 現在の状態を保存
        wasConnected = isConnected;
    }
}

//地面についたら時間に関わらず移動させる