using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    public Transform player;

    // プレイヤーからの横方向のオフセット
    public float horizontalOffset = 0f;

    [SerializeField]
    private WireActionScript wireActionScript;

    void LateUpdate()
    {
        if (player == null) return;

        bool isConnected = wireActionScript.IsConnected;

        // ワイヤー不使用時
        if (!isConnected)
        {
            horizontalOffset = 3f;

            // プレイヤーの位置にオフセットを加えた位置にカメラを移動
            Vector3 newPosition = transform.position;

            // プレイヤーのx位置 + オフセット（例：左側に表示するならマイナス）
            newPosition.x = player.position.x + horizontalOffset;

            // YとZはカメラの元の高さを維持
            transform.position = new Vector3(newPosition.x, transform.position.y, transform.position.z);
        }
        // ワイヤー使用時
        else
        {
            horizontalOffset = 3f;

            // プレイヤーの位置にオフセットを加えた位置にカメラを移動
            Vector3 newPosition = transform.position;

            // ここをワイヤーのターゲットの位置にする
            //newPosition.x = targetPos.x;
            // プレイヤーのx位置 + オフセット（例：左側に表示するならマイナス）
            newPosition.x = player.position.x + horizontalOffset;

            // YとZはカメラの元の高さを維持
            transform.position = new Vector3(newPosition.x, transform.position.y, transform.position.z);


            //return;
        }
    }
}