using UnityEngine;

/// <summary>
/// 投げられる爆弾の挙動を管理するクラス。
/// - 投げられた後、物理演算で飛ぶ
/// - 地形や敵に当たると爆発する
/// - 爆発エフェクト・SEを再生し、自身を破壊する
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bomb : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("爆発エフェクト")]
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("爆発SE設定")]
    [SerializeField] private AudioClip explosionSE;  // ← ★ プレイヤー/敵で変えるSE

    [Header("爆発設定")]
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private LayerMask damageableLayers;
    private int explosionDamage;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;
        Explode();
    }

    /// <summary>
    /// 爆発処理
    /// </summary>
    private void Explode()
    {
        hasExploded = true;

        // 🎵 爆発SEを再生（AudioManager経由で統一管理）
        if (explosionSE != null)
        {
            AudioManager.Instance?.PlaySE(explosionSE);
        }

        // 💥 爆発エフェクトを生成
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1.0f);
        }

        // 💢 範囲内のオブジェクトにダメージ
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(explosionDamage);
            }
        }

        // 🧨 最後に自分を破壊
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
