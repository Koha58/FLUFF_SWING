using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// プレイヤーの攻撃処理・被ダメージ処理をまとめて管理するクラス。
/// ・近くの敵を自動検出して攻撃タイプを切り替える
/// ・IDamageableを実装して、被弾時の処理も担当する
/// ・アニメーション・SE・UI連携まで含めた統合管理
/// </summary>
public class PlayerAttack : MonoBehaviour, IDamageable
{
    #region === Inspector Settings ===

    [Header("▼ プレイヤーステータス関連")]
    [SerializeField] private CharacterStatus status; // 攻撃力・HP・攻撃範囲などの基本パラメータ

    [Header("▼ 連携スクリプト")]
    [SerializeField] private PlayerMove playerMove;                     // 足元タイル検出などを行う移動スクリプト
    [SerializeField] private PlayerAnimatorController animatorController; // 攻撃や被弾アニメーションを制御
    [SerializeField] private PlayerHealthUI healthUI;                   // HP表示UI

    [Header("▼ 攻撃関連")]
    [SerializeField] private GameObject bombPrefab;  // 遠距離攻撃に使う爆弾プレハブ

    [Header("▼ サウンドエフェクト")]
    [SerializeField] private AudioClip meleeAttackSE;   // 近距離攻撃SE
    [SerializeField] private AudioClip rangedAttackSE;  // 遠距離攻撃SE
    [SerializeField] private AudioClip damageSE;        // 被ダメージ時SE

    #endregion


    #region === Private Fields & Constants ===

    private float throwForce = 7f;          // 爆弾を投げる初速
    private int currentHP;                  // 現在のHP
    private float invincibleTime = 3.0f;    // 無敵継続時間
    private const float BlinkInterval = 0.1f; // 点滅間隔（無敵演出用）
    private bool isInvincible = false;      // 無敵中フラグ
    private bool isDead = false;            // 死亡済みフラグ
    private SpriteRenderer spriteRenderer;  // 無敵点滅で使用

    #endregion


    private void Start()
    {
        // 現在HPを最大HPで初期化
        currentHP = status.maxHP;

        // SpriteRendererを取得（無敵点滅に使う）
        spriteRenderer = GetComponent<SpriteRenderer>();

        // HP UIを初期化
        healthUI?.SetMaxHealth(currentHP);
    }


    private void Update()
    {
        // 足元タイルを取得（PlayerMove側で管理）
        var groundTile = playerMove.CurrentGroundTile;

        // 足元が「ダメージ床(Hazard)」なら、その分のダメージを受ける
        if (groundTile != null && groundTile.tileType == CustomTile.TileType.Hazard)
        {
            TakeDamage(groundTile.damageAmount);
        }
    }


    /// <summary>
    /// 敵を自動検出して、距離に応じて近距離 or 遠距離攻撃を実行する
    /// </summary>
    public void PerformAutoAttack()
    {
        // 最も近い敵を検索
        var target = FindNearestEnemy();

        if (target != null)
        {
            // 敵との距離を測定
            Transform targetTransform = ((MonoBehaviour)target).transform;
            float distance = Vector2.Distance(transform.position, targetTransform.position);

            // 範囲内なら近距離攻撃
            if (status.meleeRange > 0f && distance <= status.meleeRange)
            {
                Debug.Log("Executing MeleeAttack()");
                MeleeAttack(target);
                return;
            }
            // 遠距離攻撃範囲内なら遠距離攻撃
            else if (status.attackRadius > 0f && distance <= status.attackRadius)
            {
                Debug.Log("Executing RangedAttack()");
                RangedAttack(target);
                return;
            }

            Debug.Log("Target is out of attack range.");
        }

        // 敵がいない場合は空振り攻撃
        Debug.Log("No valid target. Executing empty MeleeAttack.");
        MeleeAttack(null);
    }


    /// <summary>
    /// シーン内の敵から最も近いものを返す
    /// </summary>
    private IDamageable FindNearestEnemy()
    {
        // "Enemy"タグの付いた全オブジェクトを取得
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        IDamageable nearest = null;
        float minDist = float.MaxValue;

        // 各敵との距離を計算し、最も近い敵を記録
        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);

            // 攻撃範囲内で最短距離の敵を選ぶ
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
    /// 近距離攻撃処理（攻撃アニメーション＋SE＋ダメージ適用）
    /// </summary>
    private void MeleeAttack(IDamageable target)
    {
        // 攻撃方向を決定（ターゲット方向 or デフォルト右向き）
        float direction = 1f;
        if (target != null)
        {
            Vector2 targetDir = ((MonoBehaviour)target).transform.position - transform.position;
            direction = Mathf.Sign(targetDir.x);
        }

        // 近距離攻撃アニメーションを再生
        animatorController?.PlayMeleeAttackAnimation(direction);

        // 攻撃SEを再生
        if (meleeAttackSE != null)
            AudioManager.Instance?.PlaySE(meleeAttackSE);

        // 実際にダメージを与える
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
    /// 遠距離攻撃処理（爆弾投げアニメーション＋SE再生）
    /// 実際の爆弾生成はアニメーションイベント側で行う
    /// </summary>
    private void RangedAttack(IDamageable target)
    {
        // 攻撃方向を決定
        float direction = 1f;
        if (target != null)
        {
            Vector2 targetDir = ((MonoBehaviour)target).transform.position - transform.position;
            direction = Mathf.Sign(targetDir.x);
        }

        // プレイヤーの向きを敵方向に合わせる（左右反転）
        if (direction != Mathf.Sign(transform.localScale.x))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        // 遠距離攻撃アニメーションを再生
        animatorController?.PlayRangedAttackAnimation(direction);

        // 遠距離攻撃SEを再生
        if (rangedAttackSE != null)
            AudioManager.Instance?.PlaySE(rangedAttackSE);
    }


    /// <summary>
    /// 実際に爆弾を生成して投げる処理（アニメーションイベントから呼び出す）
    /// </summary>
    public void ThrowBomb(float direction)
    {
        if (bombPrefab == null) return;

        // 爆弾プレハブを生成
        GameObject bombObject = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Bomb bomb = bombObject.GetComponent<Bomb>();

        // 爆弾スクリプトに発射処理を依頼
        if (bomb != null)
        {
            bomb.Launch(direction, throwForce, status.attack);
        }
    }


    /// <summary>
    /// 被ダメージ処理（IDamageable実装）
    /// </summary>
    public void TakeDamage(int damage)
    {
        // 無敵中・死亡済みなら無視
        if (isInvincible || isDead)
        {
            Debug.Log("Player is invincible. Damage ignored.");
            return;
        }

        // HPを減算
        currentHP -= damage;
        Debug.Log($"Player took {damage} damage. HP: {currentHP}");

        // 被ダメージSE再生
        if (damageSE != null)
            AudioManager.Instance?.PlaySE(damageSE);

        // 被ダメージアニメーション再生
        float direction = Mathf.Sign(transform.localScale.x);
        animatorController?.PlayDamageAnimation(direction);

        // HP UI更新
        healthUI?.UpdateHealth(currentHP);

        // HPが0以下 → 死亡処理
        if (currentHP <= 0)
        {
            OnDead();
        }
        else
        {
            // 無敵状態＋点滅演出
            StartCoroutine(InvincibleCoroutine());
        }
    }


    /// <summary>
    /// HP回復処理（UI更新付き）
    /// </summary>
    public void Heal(int amount)
    {
        currentHP += amount;
        Debug.Log($"Player healed by {amount}. HP: {currentHP}");
        healthUI?.UpdateHealth(currentHP);
    }


    /// <summary>
    /// プレイヤー死亡処理（ゲームオーバー等）
    /// </summary>
    private void OnDead()
    {
        if (isDead) return; // 二重呼び出し防止

        isDead = true;
        Debug.Log("Player died.");

        // ゲームマネージャーに通知
        GameManager.Instance.OnPlayerDead();
    }


    /// <summary>
    /// 無敵時間中に点滅する処理（演出用コルーチン）
    /// </summary>
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        bool visible = true;

        // 無敵時間が経過するまで繰り返す
        while (elapsed < invincibleTime)
        {
            // 表示 / 非表示を交互に切り替え
            visible = !visible;

            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;

            yield return new WaitForSeconds(BlinkInterval);
            elapsed += BlinkInterval;
        }

        // 点滅終了後に表示を戻す
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isInvincible = false;
    }
}
