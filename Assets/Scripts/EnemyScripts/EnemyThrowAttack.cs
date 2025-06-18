using UnityEngine;

/// <summary>
/// 敵が爆弾を投げる専用クラス
/// - Animatorのイベントから呼ばれる
/// </summary>
public class EnemyThrowAttack : MonoBehaviour
{
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private float throwForce = 7f;

    private Transform player;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
    }

    /// <summary>
    /// アニメーションイベントから呼ばれて爆弾を生成・投げる
    /// </summary>
    public void Throw()
    {
        if (bombPrefab == null || player == null) return;

        GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Bomb bombScript = bomb.GetComponent<Bomb>();

        if (bombScript != null)
        {
            // 敵からプレイヤー方向へ飛ばす
            Vector2 dir = (player.position - transform.position).normalized;
            bombScript.Launch(dir.x, throwForce, 20); // 20 = ダメージ例
        }
    }
}
