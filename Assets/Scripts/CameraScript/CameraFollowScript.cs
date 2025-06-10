using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    public Transform player;

    // プレイヤーからの横方向のオフセット
    public float horizontalOffset = 2f;

    [SerializeField]
    private WireActionScript wireActionScript;

    // スムージングの時間(数が大きいほどゆっくり移動)
    public float smoothTime = 0.3f;

    // SmoothDamp用
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (player == null || wireActionScript == null) return;

        bool isConnected = wireActionScript.IsConnected;

        // ワイヤー設置場所の座標を持ってくる
        Vector2 wirePos = wireActionScript.HookedPosition;

        // ターゲット位置を決定（X座標のみ滑らかに追従）
        float targetX;

        // ワイヤー使用時
        if (isConnected)
        {
            targetX = wirePos.x;
        }
        // ワイヤー不使用時
        else
        {
            targetX = player.position.x + horizontalOffset;
        }

        // 現在のカメラ位置を基にターゲット位置へスムーズに移動
        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(targetX, currentPos.y, currentPos.z);

        transform.position = Vector3.SmoothDamp(currentPos, targetPos, ref velocity, smoothTime);
    }
}