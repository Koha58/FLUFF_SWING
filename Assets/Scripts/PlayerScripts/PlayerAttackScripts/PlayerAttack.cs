using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// プレイヤーの攻撃処理および被ダメージ処理を管理するクラス。
/// - 最も近い敵を自動で判定し、近距離か遠距離かで攻撃方法を切り替える。
/// - IDamageableインターフェースを実装し、攻撃を受ける側の処理も行う。
/// </summary>
public class PlayerAttack : MonoBehaviour, IDamageable
{
    #region === Inspector Settings ===

    /// <summary>
    /// プレイヤーのステータス情報（攻撃力やHP、攻撃範囲など）
    /// </summary>
    [SerializeField] private CharacterStatus status;

    /// <summary>
    /// プレイヤーの移動制御スクリプト
    /// </summary>
    [SerializeField] private PlayerMove playerMove;

    /// <summary>
    /// アニメーション制御スクリプト
    /// </summary>
    [SerializeField] private PlayerAnimatorController animatorController;

    /// <summary>
    /// プレイヤーのライフUI表示用スクリプト
    /// （HPに応じてハートを表示）
    /// </summary>
    [SerializeField] private PlayerHealthUI healthUI;

    /// <summary>
    /// 爆弾Prefab（Inspectorでセット）
    /// </summary>
    [SerializeField] private GameObject bombPrefab;

    #endregion


    #region === Constants & Private Fields ===

    /// <summary>
    /// 爆弾を投げる力
    /// </summary>
    private float throwForce = 7f;

    /// <summary>
    /// 現在のHP
    /// </summary>
    private int currentHP;

    /// <summary>
    /// 被ダメージ後の無敵時間（秒）
    ///</summary>
    private float invincibleTime = 3.0f;

    /// <summary>
    /// 無敵時の点滅間隔（秒）
    /// </summary>
    private const float BlinkInterval = 0.1f;

    /// <summary>
    /// 無敵状態かどうか
    /// </summary>
    private bool isInvincible = false;

    /// <summary>
    /// SpriteRenderer（点滅用）
    ///</summary>
    private SpriteRenderer spriteRenderer;

    #endregion


    private void Start()
    {
        // 初期HPをステータスの最大HPで初期化
        currentHP = status.maxHP;

        // このオブジェクトのSpriteRendererコンポーネントを取得してキャッシュする
        // → 無敵時の点滅表示を制御するために使用
        spriteRenderer = GetComponent<SpriteRenderer>();

        // UI初期化
        healthUI?.SetMaxHealth(currentHP);
    }


    private void Update()
    {
        // プレイヤーの足元タイルを取得
        var groundTile = playerMove.CurrentGroundTile;

        // 足元タイルが存在し、かつHazardタイプなら設定されたダメージを与える
        if (groundTile != null && groundTile.tileType == CustomTile.TileType.Hazard)
        {
            TakeDamage(groundTile.damageAmount);
        }
    }


    /// <summary>
    /// 最も近い敵を探し、自動で近距離か遠距離攻撃を選択して実行する
    /// </summary>
    public void PerformAutoAttack()
    {
        // 攻撃可能範囲内で最も近い敵を探す
        var target = FindNearestEnemy();

        if (target != null)
        {
            // 敵の位置とプレイヤーの距離を計算
            Transform targetTransform = ((MonoBehaviour)target).transform;
            float distance = Vector2.Distance(transform.position, targetTransform.position);

            // 近距離攻撃範囲内なら近距離攻撃を実行
            if (status.meleeRange > 0f && distance <= status.meleeRange)
            {
                Debug.Log("Executing MeleeAttack()");
                MeleeAttack(target);
                return;
            }
            // 遠距離攻撃範囲内なら遠距離攻撃を実行
            else if (status.attackRadius > 0f && distance <= status.attackRadius)
            {
                Debug.Log("Executing RangedAttack()");
                RangedAttack(target);
                return;
            }

            Debug.Log("Target is out of attack range.");
        }

        // 敵がいない、または範囲外だった場合は空振り近接攻撃を行う
        Debug.Log("No valid target. Executing empty MeleeAttack.");
        MeleeAttack(null);
    }


    /// <summary>
    /// 攻撃可能範囲内で最も近い敵を検索して返す
    /// </summary>
    /// <returns>最も近い敵のIDamageableコンポーネント、敵がいなければnull</returns>
    private IDamageable FindNearestEnemy()
    {
        // タグ"Enemy"の全オブジェクトを取得
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        IDamageable nearest = null;
        float minDist = float.MaxValue;

        // 各敵との距離を計測し、最も近いものを特定
        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);

            // 攻撃範囲内の最短距離を更新
            if (dist < minDist && dist <= status.attackRadius)
            {
                var damageable = enemy.GetComponent<IDamageable>();

                // IDamageable実装の敵のみを対象とする
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
    /// <param name="target">攻撃対象(IDamageable)。nullの場合は空振り攻撃</param>
    private void MeleeAttack(IDamageable target)
    {
        // 攻撃方向を決定（ターゲットの位置に基づくか、右向き固定）
        float direction = 1f;
        if (target != null)
        {
            Vector2 targetDir = ((MonoBehaviour)target).transform.position - transform.position;
            direction = Mathf.Sign(targetDir.x);
        }

        // 攻撃アニメーションを再生（向き指定あり）
        animatorController?.PlayMeleeAttackAnimation(direction);

        // 対象がいればダメージを与える
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
    /// 遠距離攻撃を実行する（アニメーション再生のみ。爆弾の生成はアニメーションイベントで行う）
    /// </summary>
    /// <param name="target">攻撃対象(IDamageable)。nullも可</param>
    private void RangedAttack(IDamageable target)
    {
        // 攻撃方向を決定
        float direction = 1f;
        if (target != null)
        {
            Vector2 targetDir = ((MonoBehaviour)target).transform.position - transform.position;
            direction = Mathf.Sign(targetDir.x);
        }

        // プレイヤーの向きをターゲットの方向に合わせる（必要なら左右反転）
        if (direction != Mathf.Sign(transform.localScale.x))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        // 遠距離攻撃アニメーションを再生
        animatorController?.PlayRangedAttackAnimation(direction);
    }


    /// <summary>
    /// 爆弾を投げる処理（アニメーションイベント等から呼び出す）
    /// </summary>
    /// <param name="direction">投げる方向（1か-1）</param>
    public void ThrowBomb(float direction)
    {
        if (bombPrefab == null) return;

        // 爆弾オブジェクトを生成し、初期位置をプレイヤー位置に設定
        GameObject bombObject = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Bomb bomb = bombObject.GetComponent<Bomb>();

        if (bomb != null)
        {
            // 爆弾を指定方向に発射（左向きなら力を逆に）
            bomb.Launch(direction, throwForce, status.attack);
        }
    }


    /// <summary>
    /// ダメージを受ける処理（IDamageableインターフェースの実装）
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    public void TakeDamage(int damage)
    {
        if (isInvincible)
        {
            Debug.Log("Player is invincible. Damage ignored.");
            return;
        }

        currentHP -= damage;
        Debug.Log($"Player took {damage} damage. HP: {currentHP}");

        float direction = Mathf.Sign(transform.localScale.x);
        animatorController?.PlayDamageAnimation(direction);

        // UI更新
        healthUI?.UpdateHealth(currentHP);

        if (currentHP <= 0)
        {
            OnDead();
        }
        else
        {
            // 無敵状態にして点滅開始
            StartCoroutine(InvincibleCoroutine());
        }
    }


    /// <summary>
    /// プレイヤーのHPを指定量回復する。
    /// 最大HPを超えて回復することも可能。
    /// </summary>
    /// <param name="amount">回復するHPの量</param>
    public void Heal(int amount)
    {
        // 回復量を現在のHPに加算（最大HPを超えてもよい）
        currentHP += amount;

        // デバッグログに回復情報を表示
        Debug.Log($"Player healed by {amount}. HP: {currentHP}");

        // HP UIを更新（nullチェック付き）
        healthUI?.UpdateHealth(currentHP);
    }


    /// <summary>
    /// HPが0以下になった時の処理（ゲームオーバー等）
    /// </summary>
    private void OnDead()
    {
        Debug.Log("Player died.");
        GameManager.Instance.OnPlayerDead();
    }


    /// <summary>
    /// ダメージ後の無敵状態を管理し、一定時間プレイヤーを点滅させるコルーチン。
    /// 攻撃を受けた後に呼び出され、
    /// 連続でダメージを受けないようにする演出を行う。
    /// </summary>
    private IEnumerator InvincibleCoroutine()
    {
        // 無敵フラグをONにする
        isInvincible = true;

        // 経過時間を初期化
        float elapsed = 0f;

        // 点滅状態を管理するフラグ（true=表示、false=非表示）
        bool visible = true;

        // 無敵時間が経過するまで繰り返す
        while (elapsed < invincibleTime)
        {
            // 表示/非表示を交互に切り替える
            visible = !visible;

            // スプライトの表示状態を更新
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }

            // 点滅間隔分待機する
            yield return new WaitForSeconds(BlinkInterval);

            // 経過時間を加算
            elapsed += BlinkInterval;
        }

        // 点滅終了後、必ず表示状態に戻す
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // 無敵状態を解除する
        isInvincible = false;
    }
}