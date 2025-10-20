using UnityEngine;

/// <summary>
/// 敵キャラクターの制御クラス
/// ・ステータス管理
/// ・状態遷移(StateMachine)
/// ・ダメージ処理
/// ・プール返却
/// </summary>
public class EnemyController : MonoBehaviour, IDamageable
{
    #region === SerializeFields ===

    /// <summary>敵の種類</summary>
    [SerializeField] private EnemyType enemyType;

    /// <summary>ステータス参照元データ</summary>
    [SerializeField] private CharacterBase characterData;

    /// <summary>ステータス情報（HPなど）</summary>
    [SerializeField] private CharacterStatus status;

    /// <summary>アニメーション制御コンポーネント</summary>
    [SerializeField] private EnemyAnimationController animationController;

    /// <summary>ステートマシン設定データ</summary>
    [SerializeField] private EnemyStateMachineSO stateMachineSO;

    /// <summary>死亡アニメーションの有無</summary>
    [SerializeField] private bool hasDeadAnimation = true;

    /// <summary>SpriteRenderer</summary>
    [SerializeField] private SpriteRenderer spriteRenderer;

    /// <summary>
    /// 死亡時のSE
    /// </summary>
    [SerializeField] private AudioClip deathSE;

    /// <summary>
    /// ワイヤーカット時のSE
    /// </summary>
    [SerializeField] private AudioClip cutSE;

    #endregion

    #region === Private Fields ===

    /// <summary>敵専用のステートマシン</summary>
    private StateMachine<EnemyController> stateMachine;

    /// <summary>現在のHP</summary>
    private int currentHP;

    /// <summary>キャラクターの移動速度</summary>
    private float moveSpeed;

    /// <summary>パトロール開始位置X座標</summary>
    private float patrolStartX;

    /// <summary>パトロール方向（1:右, -1:左）</summary>
    private int patrolDirection = -1;

    private WireActionScript wireToCut; // ★ Wireを一時保持する

    #endregion

    #region === Properties ===

    /// <summary>敵の種類を外部に公開</summary>
    public EnemyType Type => enemyType;

    /// <summary>ステートマシン設定SOを外部に公開</summary>
    public EnemyStateMachineSO StateMachineSO => stateMachineSO;

    /// <summary>死亡アニメーションの有無</summary>
    public bool HasDeadAnimation => hasDeadAnimation;

    /// <summary>移動速度</summary>
    public float MoveSpeed => moveSpeed;

    /// <summary>パトロール開始位置X</summary>
    public float PatrolStartX
    {
        get => patrolStartX;
        set => patrolStartX = value;
    }

    /// <summary>パトロール方向</summary>
    public int PatrolDirection
    {
        get => patrolDirection;
        set => patrolDirection = value;
    }

    /// <summary>アニメーション中の移動無効化</summary>
    public bool IsMovementDisabledByAnimation { get; private set; }

    /// <summary>
    /// 方向状態を外部からアクセスさせる
    /// </summary>
    public int Direction { get; set; } = -1; // 左向きで開始

    #endregion

    #region === Unity Callbacks ===

    /// <summary>
    /// ステートマシン初期化 & ステータス設定
    /// </summary>
    private void Awake()
    {
        // ステートマシンを自身を対象に初期化
        stateMachine = new StateMachine<EnemyController>(this);

        // キャラクターデータから移動速度を取得
        moveSpeed = characterData.moveSpeed;

        // SpriteRendererがない場合は作成する
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    /// <summary>
    /// 有効化時にHP初期化 & 開始ステートを設定
    /// </summary>
    private void OnEnable()
    {
        // HPを最大値にリセット
        currentHP = status.maxHP;

        // ステートマシン設定がない場合はエラー
        if (stateMachineSO == null)
        {
            Debug.LogError("StateMachineSOが設定されていません！");
            return;
        }

        // 移動ステートを使う場合は移動ステートで開始
        if (stateMachineSO.usesMove && stateMachineSO.moveState != null)
        {
            stateMachine.ChangeState(stateMachineSO.moveState);
        }
        // そうでなければ攻撃ステートで開始
        else if (stateMachineSO.attackState != null)
        {
            stateMachine.ChangeState(stateMachineSO.attackState);
        }
        // 開始ステートが無ければエラー
        else
        {
            Debug.LogError("開始ステートが設定されていません！");
        }
    }

    /// <summary>
    /// ステートマシン更新
    /// </summary>
    private void Update()
    {
        // ステートマシンの毎フレーム更新を呼び出す
        stateMachine.Update(Time.deltaTime);
    }

    #endregion

    #region === Enemy Logic ===

    /// <summary>
    /// ダメージを受ける
    /// Patrolタイプはアニメーション中は無敵
    /// </summary>
    public void TakeDamage(int damage)
    {
        Debug.Log($"TakeDamage called. Type={Type}, IsMovementDisabledByAnimation={IsMovementDisabledByAnimation}");

        // Patrolタイプで、潜りアニメーション中でない場合はダメージ無効
        if (Type == EnemyType.Patrol && !IsMovementDisabledByAnimation)
            return;

        // HPを減少
        currentHP -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {currentHP}");

        // HPが0以下になったら死亡ステートへ遷移
        if (currentHP <= 0)
        {
            // 死亡SEを再生
            AudioManager.Instance?.PlaySE(deathSE);
            stateMachine.ChangeState(stateMachineSO.deadState);
        }
    }

    /// <summary>
    /// 攻撃可能なら攻撃を実行（未実装）
    /// </summary>
    public void AttackIfPossible()
    {
        // TODO: 攻撃ロジック実装予定
    }

    /// <summary>
    /// Playerとの接触処理
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Patrolで、かつ潜り中は無効化
            if (Type == EnemyType.Patrol && !IsMovementDisabledByAnimation)
            {
                Debug.Log("Patrol enemy is disabled by animation. No damage dealt.");
                return; // スキップ
            }

            // それ以外はダメージを与える
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(status.attack);
                Debug.Log($"Enemy attacked Player for {status.attack} damage");
            }
        }

        if (collision.CompareTag("Wire"))
        {
            // Birdにワイヤーが接触したとき
            if (Type == EnemyType.Bird)
            {
                // ワイヤーの Player を取得して CutWire を呼ぶ
                var player = collision.GetComponentInParent<WireActionScript>();

                if (player != null)
                {
                    wireToCut = player;  // ← Wireを覚えておく！

                    // 攻撃ステートに遷移
                    stateMachine.ChangeState(stateMachineSO.cutState);
                }
            }
        }

        // "Flip"タグに触れたら反転
        if (collision.CompareTag("Flip"))
        {
            ReverseDirection();
        }
    }

    /// <summary>
    /// 方向と Sprite を反転する
    /// </summary>
    public void ReverseDirection()
    {
        Direction *= -1;
        UpdateSpriteFlip();
        Debug.Log($"[EnemyController] 方向反転: {Direction}");
    }

    /// <summary>
    /// SpriteRenderer の FlipX を Direction に合わせて設定
    /// </summary>
    private void UpdateSpriteFlip()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (Direction == 1);
        }
    }

    /// <summary>
    /// 死亡後、プールへ返却
    /// </summary>
    public void HandleDead()
    {
        // EnemyPoolに返却して非アクティブ化
        EnemyPool.Instance.ReturnToPool(this);
    }

    #endregion

    #region === Animation Events ===

    /// <summary>
    /// アニメーションイベント：移動無効化ON
    /// </summary>
    public void DisableMovementByAnimation()
    {
        // 移動無効フラグをON
        IsMovementDisabledByAnimation = true;
    }

    /// <summary>
    /// アニメーションイベント：移動無効化OFF
    /// </summary>
    public void EnableMovementByAnimation()
    {
        // 移動無効フラグをOFF
        IsMovementDisabledByAnimation = false;
    }

    #endregion

    #region === State Control ===

    /// <summary>
    /// アニメーション制御を取得
    /// </summary>
    public EnemyAnimationController GetAnimationController()
    {
        return animationController;
    }

    /// <summary>
    /// 攻撃ステートに切り替え
    /// </summary>
    public void SwitchToAttack()
    {
        // 攻撃中は移動無効
        DisableMovementByAnimation();

        // 攻撃ステートに遷移
        stateMachine.ChangeState(stateMachineSO.attackState);
    }

    /// <summary>
    /// 攻撃アニメーション終了時に呼ばれる
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        EnableMovementByAnimation();  // 移動無効を解除
        SwitchToMove();               // ステートを移動に戻す
                                      // Wireがあれば切る
        if (wireToCut != null)
        {
            wireToCut.CutWire();
            wireToCut = null;  // 忘れずクリア！
        }
    }

    /// <summary>
    /// 移動ステートに切り替え
    /// </summary>
    public void SwitchToMove()
    {
        // 移動ステートに遷移
        stateMachine.ChangeState(stateMachineSO.moveState);
    }

    #endregion

    /// <summary>
    /// ワイヤーカットSE再生
    /// </summary>
    public void PlayCutSE()
    {
        AudioManager.Instance?.PlaySE(cutSE);
    }
}
