using UnityEngine;

/// <summary>
/// プレイヤーの攻撃処理および被ダメージ処理を管理するクラス。
/// - 最も近い敵を自動で判定し、近距離か遠距離かで攻撃方法を切り替える。
/// - IDamageableインターフェースを実装し、攻撃を受ける側の処理も行う。
/// </summary>
public class PlayerAttack : MonoBehaviour, IDamageable
{
    /// <summary>キャラクターのステータス情報（攻撃力やHP、攻撃範囲など）</summary>
    [SerializeField] private CharacterStatus status;

    // アニメーション制御スクリプト
    [SerializeField] private PlayerAnimatorController animatorController;

    [SerializeField] private GameObject bombPrefab;   // 爆弾のPrefab（Inspectorでセット）
    private float throwForce = 10f;  // 爆弾の投げる力

    /// <summary>現在のHP</summary>
    private int currentHP;

    private void Start()
    {
        // 初期HPをステータスの最大HPで初期化
        currentHP = status.maxHP;
    }

    /// <summary>
    /// 最も近い敵を探し、自動で近距離か遠距離攻撃を選択して実行する
    /// </summary>
    public void PerformAutoAttack()
    {
        var target = FindNearestEnemy();

        if (target != null)
        {
            Transform targetTransform = ((MonoBehaviour)target).transform;
            float distance = Vector2.Distance(transform.position, targetTransform.position);

            if (status.meleeRange > 0f && distance <= status.meleeRange)
            {
                Debug.Log("Executing MeleeAttack()");
                MeleeAttack(target);
                return;
            }
            else if (status.attackRadius > 0f && distance <= status.attackRadius)
            {
                Debug.Log("Executing RangedAttack()");
                RangedAttack(target);
                return;
            }

            Debug.Log("Target is out of attack range.");
        }

        // 敵がいない、または範囲外だった場合でも空振り近接攻撃
        Debug.Log("No valid target. Executing empty MeleeAttack.");
        MeleeAttack(null);
    }

    /// <summary>
    /// 攻撃可能範囲内で最も近い敵を検索して返す
    /// </summary>
    /// <returns>最も近い敵のIDamageableコンポーネント、敵がいなければnull</returns>
    private IDamageable FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        IDamageable nearest = null;
        float minDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist && dist <= status.attackRadius)
            {
                var damageable = enemy.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    nearest = damageable;
                    minDist = dist;
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// 近距離攻撃を実行する
    /// </summary>
    private void MeleeAttack(IDamageable target)
    {
        // ターゲットがいれば方向を計算、いなければ右向きで仮定（または現在の向き）
        float direction = 1f;
        if (target != null)
        {
            Vector2 targetDir = ((MonoBehaviour)target).transform.position - transform.position;
            direction = Mathf.Sign(targetDir.x);
        }

        // アニメーション再生
        animatorController?.PlayMeleeAttackAnimation(direction);

        // 攻撃が当たる対象がいる場合のみダメージ処理
        if (target != null)
        {
            target.TakeDamage(status.attack);
            Debug.Log("Performed melee attack on target.");
        }
        else
        {
            Debug.Log("Performed empty melee attack.");
        }
    }

    /// <summary>
    /// 遠距離攻撃を実行する
    /// </summary>
    private void RangedAttack(IDamageable target)
    {
        // ターゲットがいれば方向を計算、いなければ右向きで仮定（または現在の向き）
        float direction = 1f;
        if (target != null)
        {
            Vector2 targetDir = ((MonoBehaviour)target).transform.position - transform.position;
            direction = Mathf.Sign(targetDir.x);
        }

        // アニメーション再生
        animatorController?.PlayRangedAttackAnimation(direction);
    }

    public void ThrowBomb(float direction)
    {
        if (bombPrefab == null) return;

        GameObject bombObject = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Bomb bomb = bombObject.GetComponent<Bomb>();

        if (bomb != null)
        {
            bomb.Launch(direction, throwForce, status.attack);  // 攻撃力も渡す
        }
    }

    /// <summary>
    /// ダメージを受ける処理（IDamageableインターフェース実装）
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"Player took {damage} damage. HP: {currentHP}");

        if (currentHP <= 0)
        {
            OnDead();
        }
    }

    /// <summary>
    /// HPが0以下になった時の処理
    /// </summary>
    private void OnDead()
    {
        Debug.Log("Player died.");
        // ゲームオーバー処理など
    }
}
