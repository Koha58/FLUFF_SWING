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

    [Header("▼ 爆発エフェクト")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionEffectLifeTime = 1.0f;

    [Header("▼ 爆発SE設定")]
    [SerializeField] private AudioClip explosionSE;

    [Header("▼ 爆発設定")]
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private LayerMask damageableLayers;
    private int explosionDamage;

    [Header("▼ 投擲設定（Launch）")]
    [SerializeField] private float launchUpwardRatio = 0.5f;

    [Header("▼ 飛び方の設定（LaunchToward）")]
    [SerializeField] private float defaultFlightTime = 0.8f;

    [Header("▼ サウンド距離管理")]
    [SerializeField] private float soundMaxDistance = 15f; // 爆発音が届く最大距離

    private Transform playerTransform; // プレイヤーのTransform (外部から設定される)
    private bool hasExploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 投擲直後、EnemyThrowAttackなどからプレイヤー情報を設定する
    /// </summary>
    /// <param name="player">プレイヤーのTransform</param>
    public void Setup(Transform player)
    {
        playerTransform = player;
    }

    /// <summary>
    /// 指定した方向へ、一定の力で単純に投げ飛ばします。
    /// 基本的なノックバックや吹き飛ばし処理として使用します。
    /// </summary>
    /// <param name="direction">投げる向き (-1=左, 1=右)</param>
    /// <param name="force">投げ飛ばす力の大きさ</param>
    /// <param name="damage">命中時に与えるダメージ値</param>
    public void Launch(float direction, float force, int damage)
    {
        explosionDamage = damage;

        // 斜め上方向の比率を Inspector から調整可能に
        rb.linearVelocity = new Vector2(
            force * direction,
            force * launchUpwardRatio
        );
    }


    /// <summary>
    /// ターゲット座標へ向けて、指定時間で着弾する放物線軌道で投擲します。
    /// </summary>
    /// <param name="targetPos">着弾させたいターゲットの座標</param>
    /// <param name="damage">命中時に与えるダメージ値</param>
    /// <param name="flightTime">飛行にかける時間（秒）</param>
    public void LaunchToward(Vector2 targetPos, int damage, float flightTime = -1f)
    {
        explosionDamage = damage;

        // デフォルト飛行時間を Inspector で調整可能に
        float T = (flightTime > 0f) ? flightTime : defaultFlightTime;

        Vector2 start = transform.position;
        Vector2 toTarget = targetPos - start;

        float g = Mathf.Abs(Physics2D.gravity.y);

        // 放物線の初速を計算
        float vx = toTarget.x / T;
        float vy = (toTarget.y / T) + (0.5f * g * T);

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
        if (hasExploded) return;
        hasExploded = true;

        // --- SE再生 (距離減衰適用) ---
        if (explosionSE != null && playerTransform != null)
        {
            // プレイヤーとの距離を計算
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= soundMaxDistance)
            {
                // 距離に応じて音量を減衰 (0: 近い -> 1: 遠い)
                float distanceRatio = distanceToPlayer / soundMaxDistance;
                float attenuatedVolume = 1.0f * (1f - distanceRatio); // 1.0fは標準音量

                // 距離減衰後の音量で再生
                AudioManager.Instance?.PlaySE(explosionSE, attenuatedVolume);
            }
            // 範囲外の場合は音を鳴らさない
        }
        else if (explosionSE != null)
        {
            // プレイヤーTransformがない場合は、標準音量で再生（フォールバック）
            // AudioManager.Instance?.PlaySE(explosionSE); 
        }
        // -----------------------------

        // エフェクト生成（寿命も Inspector 設定）
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, explosionEffectLifeTime);
        }

        // 範囲ダメージ
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(explosionDamage);
            }
        }

        // 自身を破壊
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}