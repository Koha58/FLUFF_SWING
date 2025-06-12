using UnityEngine;

/// <summary>
/// 投げられる爆弾の挙動を管理するクラス。
/// - 投げられた後、物理演算で飛ぶ
/// - 地形や敵に当たると爆発する
/// - 爆発エフェクトを再生し、自身を破壊する
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bomb : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("爆発エフェクト")]
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("爆発設定")]
    private float explosionRadius = 2f;    // 爆発範囲
    private int explosionDamage;       // 爆発ダメージ
    [SerializeField] private LayerMask damageableLayers;     // ダメージ判定対象レイヤー

    private bool hasExploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(float direction, float force, int damage)
    {
        rb.linearVelocity = new Vector2(force * direction, force * 0.5f);
        explosionDamage = damage;
    }

    /// <summary>
    /// 衝突判定（地形・敵などに当たったら爆発）
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;  // 既に爆発済みなら無視

        Explode();
    }

    /// <summary>
    /// 爆発処理
    /// - エフェクト生成
    /// - 範囲内のダメージ処理
    /// - 爆弾オブジェクト破棄
    /// </summary>
    private void Explode()
    {
        hasExploded = true;

        // 爆発エフェクトの生成
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 範囲内のダメージ判定
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(explosionDamage);
            }
        }

        // 自身の削除
        Destroy(gameObject);
    }

    /// <summary>
    /// エディタ上で爆発範囲を可視化（Gizmo表示）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
