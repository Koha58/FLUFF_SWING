using UnityEngine;

public class CrushUp2D : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float crushGap = 0.05f; // この隙間以下なら潰れ
    [SerializeField] private string playerTag = "Player";

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(playerTag)) return;

        var playerCol = collision.collider; // プレイヤー側の当たったCollider
        Bounds b = playerCol.bounds;

        // プレイヤーの足元から少し下をチェック
        Vector2 origin = new Vector2(b.center.x, b.min.y - 0.01f);
        float dist = crushGap;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, dist, groundLayer);
        if (hit.collider != null)
        {
            // ここで「潰れた」処理：位置を押し上げる
            // 応急：地面にめり込む前に上へ逃がす
            collision.transform.position += Vector3.up * 0.2f;
        }
    }
}
