using UnityEngine;

/// <summary>
/// 上下からプレイヤーが挟まれた（潰れた）状態を検知し、
/// 強制的にプレイヤーを上方向へ移動させてスタックを防ぐ救済クラス。
/// </summary>
public class CrushUp2D : MonoBehaviour
{
    // 地面として判定するレイヤー
    [SerializeField] private LayerMask groundLayer;

    // 上下が「潰れている」と判定するための隙間距離
    [SerializeField] private float crushGap = 0.05f;

    // プレイヤー判定用タグ
    [SerializeField] private string playerTag = "Player";

    // Rayの始点をCollider境界から少しずらすためのオフセット
    private const float RayOffset = 0.01f;

    // 潰れた際にプレイヤーを押し上げる距離
    private const float EscapeUpDistance = 0.1f;

    /// <summary>
    /// 衝突中（毎フレーム）呼ばれる処理
    /// 上下から挟まれている状態を検知したら救済処理を行う
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 衝突相手がプレイヤーでなければ処理しない
        if (!collision.gameObject.CompareTag(playerTag)) return;

        // プレイヤーのColliderとBoundsを取得
        Collider2D playerCol = collision.collider;
        Bounds bounds = playerCol.bounds;

        // =========================
        // 1. 足元チェック
        // =========================
        // Colliderの底面から少し下にRayの始点を置く
        Vector2 footRayOrigin = new Vector2(
            bounds.center.x,
            bounds.min.y - RayOffset
        );

        // 下方向にRayを飛ばして、足元が地面で塞がれているか確認
        RaycastHit2D footHit = Physics2D.Raycast(
            footRayOrigin,
            Vector2.down,
            crushGap,
            groundLayer
        );

        // =========================
        // 2. 頭上チェック
        // =========================
        // Colliderの上面から少し上にRayの始点を置く
        Vector2 headRayOrigin = new Vector2(
            bounds.center.x,
            bounds.max.y + RayOffset
        );

        // 上方向にRayを飛ばして、頭上が地面で塞がれているか確認
        RaycastHit2D headHit = Physics2D.Raycast(
            headRayOrigin,
            Vector2.up,
            crushGap,
            groundLayer
        );

        // =========================
        // 潰れ判定 & 救済処理
        // =========================
        // ・足元に地面がある
        // ・頭上にも地面がある
        // → 上下から挟まれている（潰れている）と判断
        if (footHit.collider != null && headHit.collider != null)
        {
            // プレイヤーを上方向へ少し押し上げて脱出させる
            collision.transform.position += Vector3.up * EscapeUpDistance;
        }
    }
}
