using UnityEngine;

/// <summary>
/// プレイヤーの攻撃処理および被ダメージ処理を管理するクラス。
/// - 最も近い敵を自動で判定し、近距離か遠距離かで攻撃方法を切り替える。
/// - IDamageableインターフェースを実装し、攻撃を受ける側の処理も行う。
/// </summary>
public class PlayerAttack : MonoBehaviour, IDamageable
{
    // キャラクターのステータス情報（攻撃力や最大HPなど）
    [SerializeField] private CharacterStatus status;

    // 近距離攻撃が可能な範囲（単位：ワールド単位）
    [SerializeField] private float meleeRange = 1.5f;

    // 攻撃可能な最大範囲（この範囲内の敵をターゲットにする）
    [SerializeField] private float attackRadius = 5f;

    // 現在のHP
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
        if (target == null) return;

        // 敵のTransformを取得
        Transform targetTransform = ((MonoBehaviour)target).transform;
        float distance = Vector2.Distance(transform.position, targetTransform.position);

        // 距離によって攻撃方法を切り替える
        if (distance <= meleeRange)
        {
            MeleeAttack(target);
        }
        else
        {
            RangedAttack(target);
        }
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
            if (dist < minDist && dist <= attackRadius)
            {
                nearest = enemy.GetComponent<IDamageable>();
                minDist = dist;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 近距離攻撃を実行する
    /// </summary>
    /// <param name="target">攻撃対象</param>
    private void MeleeAttack(IDamageable target)
    {
        target.TakeDamage(status.attack);
        Debug.Log("Performed melee attack.");
    }

    /// <summary>
    /// 遠距離攻撃を実行する
    /// </summary>
    /// <param name="target">攻撃対象</param>
    private void RangedAttack(IDamageable target)
    {
        target.TakeDamage(status.attack);
        Debug.Log("Performed ranged attack.");
    }

    /// <summary>
    /// ダメージを受ける処理（IDamageableインターフェース実装）
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
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
        // ゲームオーバー処理など追加可能
    }
}
