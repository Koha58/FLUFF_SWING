using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// プレイヤーの攻撃処理・被ダメージ処理をまとめて管理するクラス。
/// ・近距離攻撃／遠距離攻撃の自動選択
/// ・IDamageable を実装して被ダメージ処理を一元管理
/// ・アニメーション／SE／UI との連携も担当
/// </summary>
public class PlayerAttack : MonoBehaviour, IDamageable
{
    #region === Inspector Settings ===

    [Header("▼ プレイヤーステータス関連")]
    [SerializeField] private CharacterStatus status; // 攻撃力・HP・攻撃範囲などの基本パラメータ

    [Header("▼ 連携スクリプト")]
    [SerializeField] private PlayerMove playerMove;                     // 足元タイルなどの移動系情報を持つ
    [SerializeField] private PlayerAnimatorController animatorController; // 攻撃・被弾アニメーション制御
    [SerializeField] private PlayerHealthUI healthUI;                   // HPバーUI

    [Header("▼ 攻撃関連")]
    [SerializeField] private GameObject bombPrefab;  // 遠距離攻撃で投げる爆弾プレハブ

    [Header("▼ ターゲットアイコン")]
    [SerializeField] private GameObject targetIconPrefab; // ターゲットアイコンのプレハブ

    [Header("▼ サウンドエフェクト")]
    [SerializeField] private AudioClip meleeAttackSE;   // 近距離攻撃SE
    [SerializeField] private AudioClip rangedAttackSE;  // 遠距離攻撃SE
    [SerializeField] private AudioClip damageSE;        // 被ダメージSE

    #endregion


    #region === Private Fields & Constants ===

    private int currentHP;                  // 現在HP
    private float invincibleTime = 3.0f;    // 被ダメージ後の無敵時間
    private const float BlinkInterval = 0.1f; // 無敵中の点滅速度
    private bool isInvincible = false;      // 無敵中フラグ
    private bool isDead = false;            // 死亡済みフラグ
    private SpriteRenderer spriteRenderer;  // 無敵点滅に使用
    private Vector2 pendingRangedTarget;    // 遠距離攻撃のターゲット位置（アニメーションイベント用）
    private GameObject currentTargetIcon; // 現在表示中のアイコンのインスタンスを保持するフィールド

    #endregion


    private void Start()
    {
        // 現在HPを最大HPで初期化
        currentHP = status.maxHP;

        // SpriteRenderer取得（点滅演出で使用）
        spriteRenderer = GetComponent<SpriteRenderer>();

        // HPUI初期化
        healthUI?.SetMaxHealth(currentHP);

        // ターゲットアイコンを初期化（非アクティブでインスタンス化）
        if (targetIconPrefab != null)
        {
            currentTargetIcon = Instantiate(targetIconPrefab, transform.position, Quaternion.identity, transform.parent);
            currentTargetIcon.SetActive(false);
        }
    }


    private void Update()
    {
        // 足元タイルを PlayerMove から取得
        var groundTile = playerMove.CurrentGroundTile;

        // 地面が「Hazard（ダメージ床）」なら自動的にダメージ
        if (groundTile != null && groundTile.tileType == CustomTile.TileType.Hazard)
        {
            TakeDamage(groundTile.damageAmount);
        }

        if (groundTile != null && groundTile.tileType == CustomTile.TileType.Fallout)
        {
            TakeDamage(groundTile.damageAmount * currentHP);
        }

        // ターゲットアイコンの表示・位置更新
        UpdateTargetIcon();
    }


    /// <summary>
    /// 敵を自動検出し、距離によって近距離攻撃 or 遠距離攻撃を実行
    /// </summary>
    public void PerformAutoAttack()
    {
        // アニメーション的に攻撃が許可されていないなら中断
        if (!animatorController.CanAcceptAttackInput()) return;

        // ▼ まずは近距離攻撃を優先チェック
        var nearest = FindNearestEnemy();
        if (nearest != null)
        {
            Transform t = ((MonoBehaviour)nearest).transform;
            float dist = Vector2.Distance(transform.position, t.position);

            if (dist <= status.meleeRange)
            {
                MeleeAttack(nearest);
                return;
            }
        }

        // ▼ 近距離圏外 → 向いている方向の敵に遠距離攻撃
        var rangedTarget = FindNearestEnemyInFacingDirection();
        if (rangedTarget != null)
        {
            RangedAttack(rangedTarget);
            return;
        }

        // 何も当たらなければ「空振り近接攻撃」
        MeleeAttack(null);
    }



    /// <summary>
    /// シーン内の「最も近い敵」を取得（攻撃可能距離内のみ）
    /// </summary>
    private IDamageable FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        IDamageable nearest = null;
        float minDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);

            // 攻撃可能距離以内で最も近い敵を記録
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
    /// プレイヤーが向いている方向に存在する最も近い敵を取得
    /// </summary>
    private IDamageable FindNearestEnemyInFacingDirection()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        IDamageable nearest = null;
        float minDist = float.MaxValue;

        // localScale.x の符号で左右判定（1:右、-1:左）
        float facing = Mathf.Sign(transform.localScale.x);

        foreach (var enemy in enemies)
        {
            Vector2 dir = enemy.transform.position - transform.position;

            // プレイヤーが向いている方向側の敵のみ対象
            if (Mathf.Sign(dir.x) != facing) continue;

            float dist = dir.magnitude;

            if (dist < minDist && dist <= status.attackRadius)
            {
                var d = enemy.GetComponent<IDamageable>();
                if (d != null)
                {
                    nearest = d;
                    minDist = dist;
                }
            }
        }
        return nearest;
    }

    /// <summary>
    /// 攻撃可能範囲内の最も近い敵の上にターゲットアイコンを表示する
    /// 敵がいなければ非表示にする
    /// </summary>
    private void UpdateTargetIcon() // ← 新規追加
    {
        if (currentTargetIcon == null) return;

        // 攻撃可能範囲内の最も近い敵を取得
        var nearest = FindNearestEnemy();

        if (nearest != null)
        {
            Transform targetTransform = ((MonoBehaviour)nearest).transform;
            float dist = Vector2.Distance(transform.position, targetTransform.position);

            // 近距離攻撃範囲外の場合は遠距離攻撃可能方向の敵をチェック
            // 攻撃可能範囲（attackRadius）でチェックしているので、MeleeRangeのチェックは不要だが、
            // AutoAttackの挙動に合わせるため、向いている方向の敵もチェックするロジックを統合
            bool isMeleeRange = dist <= status.meleeRange;
            var rangedTarget = isMeleeRange ? null : FindNearestEnemyInFacingDirection();

            // 近距離範囲内か、遠距離対象が見つかった場合のみアイコンを表示
            if (isMeleeRange || (rangedTarget != null && nearest.Equals(rangedTarget)))
            {
                // アイコンの位置をターゲット敵の頭上などに設定
                // ここではターゲットのY座標にオフセット(例: 1.0f)を加算しています
                Vector3 iconPosition = targetTransform.position + Vector3.up * 1.0f;

                currentTargetIcon.transform.position = iconPosition;
                if (!currentTargetIcon.activeSelf)
                {
                    currentTargetIcon.SetActive(true);
                }
            }
            else
            {
                // 攻撃範囲外なら非表示
                if (currentTargetIcon.activeSelf)
                {
                    currentTargetIcon.SetActive(false);
                }
            }
        }
        else
        {
            // 敵がいない場合も非表示
            if (currentTargetIcon.activeSelf)
            {
                currentTargetIcon.SetActive(false);
            }
        }
    }


    /// <summary>
    /// 近距離攻撃（アニメーション・SE・対象へのダメージ）
    /// </summary>
    private void MeleeAttack(IDamageable target)
    {
        // 攻撃方向を決定（ターゲットがいればその方向、いなければ右）
        float direction = 1f;
        if (target != null)
        {
            Vector2 targetDir = ((MonoBehaviour)target).transform.position - transform.position;
            direction = Mathf.Sign(targetDir.x);
        }

        // 近距離攻撃アニメーション再生
        animatorController?.PlayMeleeAttackAnimation(direction);

        // SE再生
        if (meleeAttackSE != null)
            AudioManager.Instance?.PlaySE(meleeAttackSE);

        // ダメージ処理
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
    /// 遠距離攻撃（アニメーション＆SE）  
    /// 爆弾の生成はアニメーションイベントで行う
    /// </summary>
    private void RangedAttack(IDamageable target)
    {
        Transform targetTransform = ((MonoBehaviour)target).transform;

        // 攻撃方向（ターゲットの左右）
        float direction = Mathf.Sign(targetTransform.position.x - transform.position.x);

        // 向きが違う時は反転
        if (direction != Mathf.Sign(transform.localScale.x))
        {
            var scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        // アニメーション再生
        animatorController?.PlayRangedAttackAnimation(direction);

        // SE再生
        AudioManager.Instance?.PlaySE(rangedAttackSE);

        // 爆弾生成時に使うターゲット座標を保存
        pendingRangedTarget = targetTransform.position;
    }


    /// <summary>
    /// 遠距離攻撃用の爆弾を生成して投げる（アニメーションイベントから呼ばれる）
    /// </summary>
    public void ThrowBomb(float direction)
    {
        if (bombPrefab == null) return;

        // 爆弾生成
        GameObject bombObject = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Bomb bomb = bombObject.GetComponent<Bomb>();

        // 爆弾にターゲット座標と攻撃力を渡す
        if (bomb != null)
        {
            bomb.LaunchToward(pendingRangedTarget, status.attack);
        }
    }


    /// <summary>
    /// 被ダメージ処理（IDamageable実装）
    /// </summary>
    public void TakeDamage(int damage)
    {
        // 無敵中 or すでに死亡している → 無視
        if (isInvincible || isDead)
        {
            Debug.Log("Player is invincible. Damage ignored.");
            return;
        }

        // HP減少
        currentHP -= damage;
        Debug.Log($"Player took {damage} damage. HP: {currentHP}");

        // 被ダメージSE
        if (damageSE != null)
            AudioManager.Instance?.PlaySE(damageSE);

        // ダメージアニメーション
        float direction = Mathf.Sign(transform.localScale.x);
        animatorController?.PlayDamageAnimation(direction);

        // HP UI更新
        healthUI?.UpdateHealth(currentHP);

        // HP0以下 → 死亡処理
        if (currentHP <= 0)
        {
            OnDead();
        }
        else
        {
            // 無敵＋点滅開始
            StartCoroutine(InvincibleCoroutine());
        }
    }


    /// <summary>
    /// HP回復処理
    /// </summary>
    public void Heal(int amount)
    {
        currentHP += amount;
        Debug.Log($"Player healed by {amount}. HP: {currentHP}");

        // HP UI更新
        healthUI?.UpdateHealth(currentHP);
    }


    /// <summary>
    /// プレイヤー死亡処理（ゲームオーバー通知など）
    /// </summary>
    private void OnDead()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player died.");

        // GameManager に通知
        GameManager.Instance.OnPlayerDead();
    }


    /// <summary>
    /// 被ダメージ後の無敵時間中に点滅させる演出処理
    /// </summary>
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        bool visible = true;

        // 無敵時間が終わるまで繰り返す
        while (elapsed < invincibleTime)
        {
            // 表示/非表示を切り替える
            visible = !visible;

            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;

            yield return new WaitForSeconds(BlinkInterval);
            elapsed += BlinkInterval;
        }

        // 無敵終了 → 表示を戻す
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isInvincible = false;
    }
}
