﻿using UnityEngine;

/// <summary>
/// 敵キャラクターの制御クラス
/// ・ステータス管理
/// ・状態遷移(StateMachine)
/// ・ダメージ処理
/// ・プール返却
/// </summary>
public class EnemyController : MonoBehaviour, IDamageable
{
    #region === Constants ===
    private const float DEFAULT_GRAVITY = 1f;           // 通常Dynamic敵の重力
    private const float ZERO_VELOCITY = 0f;            // Rigidbody初期化用の速度
    private const int DEFAULT_DIRECTION = -1;          // 初期方向（左向き）
    #endregion

    #region === SerializeFields ===

    [Header("基本情報")]
    [SerializeField] private EnemyType enemyType;                       // 敵の種類
    [SerializeField] private CharacterBase characterData;               // ステータス参照元データ
    [SerializeField] private CharacterStatus status;                    // ステータス情報（HPなど）
    [SerializeField] private EnemyAnimationController animationController; // アニメーション制御コンポーネント
    [SerializeField] private EnemyStateMachineSO stateMachineSO;        // ステートマシン設定データ
    [SerializeField] private bool hasDeadAnimation = true;              // 死亡アニメーションの有無
    [SerializeField] private SpriteRenderer spriteRenderer;             // SpriteRenderer

    [Header("基本設定")]
    public bool keepDynamicBody = false; // Dynamicのままにする敵ならtrue

    [Header("サウンド")]
    [SerializeField] private AudioClip deathSE;  // 死亡時SE
    [SerializeField] private AudioClip cutSE;    // ワイヤーカット時SE
    [SerializeField] private AudioClip popSE;    // モグラ飛び出し/潜り時SE
    [SerializeField] private AudioClip rabbitSE; // ウサギ移動時SE

    #endregion

    #region === Private Fields ===

    private StateMachine<EnemyController> stateMachine; // 敵専用のステートマシン
    private int currentHP;                               // 現在のHP
    private float moveSpeed;                             // 移動速度
    private float patrolStartX;                          // パトロール開始位置X
    private int patrolDirection = DEFAULT_DIRECTION;     // パトロール方向（1:右, -1:左）
    private WireActionScript wireToCut;                 // ワイヤーを一時保持
    private int originalSortingOrder;                   // 元のSpriteRenderer.sortingOrder

    #endregion

    #region === Properties ===

    public EnemyType Type => enemyType;                       // 敵の種類
    public EnemyStateMachineSO StateMachineSO => stateMachineSO; // ステートマシン設定SO
    public bool HasDeadAnimation => hasDeadAnimation;        // 死亡アニメ有無
    public float MoveSpeed => moveSpeed;                     // 移動速度
    public float PatrolStartX { get => patrolStartX; set => patrolStartX = value; }
    public int PatrolDirection { get => patrolDirection; set => patrolDirection = value; }
    public bool IsMovementDisabledByAnimation { get; private set; } // アニメーション中の移動無効化
    public int Direction { get; set; } = DEFAULT_DIRECTION;          // 方向状態 (-1:左, 1:右)
    public int OriginalSortingOrder => originalSortingOrder;         // 元のsortingOrder

    #endregion

    #region === Unity Callbacks ===

    private void Awake()
    {
        // ステートマシン初期化
        stateMachine = new StateMachine<EnemyController>(this);

        // 移動速度をCharacterBaseから取得
        moveSpeed = characterData.moveSpeed;

        // SpriteRenderer が未設定の場合は子オブジェクトから取得
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 元のsortingOrderを保存
        if (spriteRenderer != null)
            originalSortingOrder = spriteRenderer.sortingOrder;
    }

    private void OnEnable()
    {
        // HPを最大値にリセット
        currentHP = status.maxHP;

        // ステートマシンが設定されていない場合はエラー
        if (stateMachineSO == null)
        {
            Debug.LogError("StateMachineSOが設定されていません！");
            return;
        }

        // 開始ステートを決定
        if (stateMachineSO.usesMove && stateMachineSO.moveState != null)
        {
            stateMachine.ChangeState(stateMachineSO.moveState); // 移動ステート開始
        }
        else if (stateMachineSO.attackState != null)
        {
            stateMachine.ChangeState(stateMachineSO.attackState); // 攻撃ステート開始
        }
        else
        {
            Debug.LogError("開始ステートが設定されていません！");
        }
    }

    private void Update()
    {
        // ステートマシンの毎フレーム更新
        stateMachine.Update(Time.deltaTime);
    }

    #endregion

    #region === Enemy Logic ===

    /// <summary>
    /// ダメージ処理
    /// Patrolタイプはアニメーション中でない場合は無効化
    /// </summary>
    public void TakeDamage(int damage)
    {
        // Patrolタイプでアニメーション中でない場合は無効化
        if (Type == EnemyType.Patrol && !IsMovementDisabledByAnimation)
            return;

        // HPを減少
        currentHP -= damage;

        // HPが0以下になったら死亡ステートに遷移
        if (currentHP <= 0)
        {
            // 死亡SEを再生
            AudioManager.Instance?.PlaySE(deathSE);

            // 死亡ステートに遷移
            stateMachine.ChangeState(stateMachineSO.deadState);
        }
    }

    public void AttackIfPossible()
    {
        // TODO: 攻撃ロジックをここに実装
    }

    /// <summary>
    /// 当たり判定処理
    /// Player, Wire, Flipタグを判定
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤー接触時
        if (collision.CompareTag("Player"))
        {
            // Patrolで、かつアニメ中でない場合はスキップ
            if (Type == EnemyType.Patrol && !IsMovementDisabledByAnimation)
                return;

            // Playerにダメージを与える
            IDamageable damageable = collision.GetComponent<IDamageable>();
            damageable?.TakeDamage(status.attack);
        }

        // ワイヤー接触時（Birdのみ）
        if (collision.CompareTag("Wire") && Type == EnemyType.Bird)
        {
            // ワイヤーのPlayerを取得
            var player = collision.GetComponentInParent<WireActionScript>();
            if (player != null)
            {
                wireToCut = player;                  // ワイヤーを保持
                stateMachine.ChangeState(stateMachineSO.cutState); // 攻撃ステートに遷移
            }
        }

        // Flipタグで方向反転
        if (collision.CompareTag("Flip"))
            ReverseDirection();
    }

    /// <summary>
    /// 方向反転処理
    /// </summary>
    public void ReverseDirection()
    {
        // 方向を反転
        Direction *= -1;

        // SpriteのFlipXを更新
        UpdateSpriteFlip();
    }

    /// <summary>
    /// SpriteRenderer のFlipXを方向に合わせる
    /// </summary>
    private void UpdateSpriteFlip()
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = (Direction == 1); // 右向きならflipX=true
    }

    /// <summary>
    /// 死亡後プールに返却
    /// </summary>
    public void HandleDead()
    {
        // EnemyPoolに返却して非アクティブ化
        EnemyPool.Instance.ReturnToPool(this);
    }

    /// <summary>
    /// Enemyをリセットして再利用可能にする
    /// Rigidbody, SpriteRenderer, Colliderを初期化
    /// </summary>
    public void ResetEnemy()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 移動を止める
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = ZERO_VELOCITY;

            if (keepDynamicBody)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = DEFAULT_GRAVITY; // Dynamicは重力有効
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = ZERO_VELOCITY;   // Kinematicは重力無効
            }
        }

        // 死亡時の上下反転を解除
        if (spriteRenderer != null)
            spriteRenderer.flipY = false;

        // 全コライダーを再有効化
        foreach (var c in GetComponents<Collider2D>())
            c.enabled = true;
    }

    #endregion

    #region === Animation Events ===

    /// <summary>
    /// アニメーションによる移動無効化ON
    /// </summary>
    public void DisableMovementByAnimation() => IsMovementDisabledByAnimation = true;

    /// <summary>
    /// アニメーションによる移動無効化OFF
    /// </summary>
    public void EnableMovementByAnimation() => IsMovementDisabledByAnimation = false;

    #endregion

    #region === State Control ===

    public EnemyAnimationController GetAnimationController() => animationController;

    /// <summary>
    /// 攻撃ステートに切り替え
    /// </summary>
    public void SwitchToAttack()
    {
        DisableMovementByAnimation();               // 攻撃中は移動不可
        stateMachine.ChangeState(stateMachineSO.attackState);
    }

    /// <summary>
    /// 攻撃アニメーション終了時に呼ばれる
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        EnableMovementByAnimation();                // 移動を再度有効化
        SwitchToMove();                             // 移動ステートに戻す

        // ワイヤーがあれば切断
        if (wireToCut != null)
        {
            wireToCut.CutWire();
            wireToCut = null; // 忘れずクリア
        }
    }

    /// <summary>
    /// 移動ステートに切り替え
    /// </summary>
    public void SwitchToMove() => stateMachine.ChangeState(stateMachineSO.moveState);

    #endregion

    #region === SE Play ===

    /// <summary>
    /// ワイヤーカット時のSEを再生する
    /// </summary>
    public void PlayCutSE()
    {
        // AudioManagerが存在すれば再生
        AudioManager.Instance?.PlaySE(cutSE);
    }

    /// <summary>
    /// モグラ飛び出し/潜り時のSEを再生する
    /// </summary>
    public void PlayPopSE()
    {
        // AudioManagerが存在すれば再生
        AudioManager.Instance?.PlaySE(popSE);
    }

    /// <summary>
    /// ウサギ移動時のSEを再生する
    /// </summary>
    public void PlayRabbitSE()
    {
        // AudioManagerが存在すれば再生
        AudioManager.Instance?.PlaySE(rabbitSE);
    }

    #endregion

}
