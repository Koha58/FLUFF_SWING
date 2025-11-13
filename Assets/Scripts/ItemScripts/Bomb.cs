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

    /// <summary>
    /// ターゲット座標に向けて放物線で飛ばす
    /// </summary>
    /// <param name="targetPosition">爆弾が届く座標</param>
    /// <param name="flightTime">到達までの時間（秒）</param>
    /// <param name="damage">爆発ダメージ</param>
    public void Launch(Vector2 targetPosition, float flightTime, int damage)
    {
        explosionDamage = damage;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        Vector2 startPosition = transform.position;

        // ターゲットまでの距離
        Vector2 distance = targetPosition - startPosition;

        // 必要な初速度を計算（放物線公式）
        // Vx = dx / t, Vy = (dy - 0.5 * g * t^2) / t
        float vx = distance.x / flightTime;
        float vy = (distance.y - 0.5f * Physics2D.gravity.y * flightTime * flightTime) / flightTime;

        // Rigidbody2D に速度をセット
        rb.linearVelocity = new Vector2(vx, vy);
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
