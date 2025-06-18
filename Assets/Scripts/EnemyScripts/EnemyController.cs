using UnityEditor.U2D.Animation;
using UnityEngine;

/// <summary>
/// 敵キャラクターの制御クラス
/// 敵の状態管理(StateMachine)やHP管理、ダメージ処理などを行う
/// </summary>
public class EnemyController : MonoBehaviour, IDamageable
{
    // ステータス管理スクリプト
    [SerializeField] private CharacterBase characterData;
    [SerializeField] private CharacterStatus status;                         // 敵のステータス情報（HPなど）
    [SerializeField] private EnemyAnimationController animationController;   // アニメーション制御用コンポーネント
    [SerializeField] private EnemyStateMachineSO stateMachineSO;             // 敵の状態管理データ(ScriptableObject)
    public EnemyStateMachineSO StateMachineSO => stateMachineSO;
    [SerializeField] private bool hasDeadAnimation = true;                   // 死亡アニメーションの有無フラグ

    private StateMachine<EnemyController> stateMachine;                      // 敵専用のステートマシン
    private int currentHP;                                                    // 現在のHP
    private float moveSpeed;                                                 // 地上での左右移動スピード
    public float MoveSpeed => moveSpeed; // ← 追加

    private float patrolStartX;
    private int patrolDirection = -1;

    public float PatrolStartX { get => patrolStartX; set => patrolStartX = value; }
    public int PatrolDirection { get => patrolDirection; set => patrolDirection = value; }

    public bool HasDeadAnimation => hasDeadAnimation;

    /// <summary>
    /// アニメーションイベント中は移動を無効化
    /// </summary>
    public bool IsMovementDisabledByAnimation { get; private set; }

    private void Awake()
    {
        // ステートマシンを初期化（自身を対象に）
        stateMachine = new StateMachine<EnemyController>(this);

        // characterData から moveSpeedを取得
        moveSpeed = characterData.moveSpeed;
    }

    private void OnEnable()
    {
        // HPを最大にリセット
        currentHP = status.maxHP;

        // ステートマシンSOが設定されているかチェック
        if (stateMachineSO == null)
        {
            Debug.LogError("stateMachineSOがセットされていません！");
            return;
        }

        // 敵の種類によって開始ステートを切り替える
        // Birdのように移動から開始する場合はmoveStateから開始
        if (stateMachineSO.usesMove && stateMachineSO.moveState != null)
        {
            stateMachine.ChangeState(stateMachineSO.moveState);
        }
        // それ以外は攻撃状態から開始
        else if (stateMachineSO.attackState != null)
        {
            stateMachine.ChangeState(stateMachineSO.attackState);
        }
        else
        {
            Debug.LogError("開始Stateがセットされていません！");
        }
    }

    private void Update()
    {
        // 毎フレームステートマシンのUpdateを呼び出す（状態遷移や処理を実行）
        stateMachine.Update(Time.deltaTime);
    }

    /// <summary>
    /// ダメージを受ける処理
    /// HPを減らし、0以下なら死亡ステートに遷移する
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {currentHP}");

        if (currentHP <= 0)
        {
            // HP0以下で死亡ステートへ遷移
            stateMachine.ChangeState(stateMachineSO.deadState);
        }
    }

    // 以下は敵の行動ロジック用のダミーメソッド
    public void AttackIfPossible() { /* 攻撃ロジック */ }

    /// <summary>
    /// 死亡処理完了後に呼ばれるメソッド
    /// プールに敵オブジェクトを返却する
    /// </summary>
    public void HandleDead()
    {
        EnemyPool.Instance.ReturnToPool(this);
    }

    /// <summary>
    /// AnimationEvent から呼び出す
    /// </summary>
    public void DisableMovementByAnimation()
    {
        IsMovementDisabledByAnimation = true;
    }

    public void EnableMovementByAnimation()
    {
        IsMovementDisabledByAnimation = false;
    }

    /// <summary>
    /// アニメーション制御コンポーネントを取得する
    /// </summary>
    /// <returns>EnemyAnimationController</returns>
    public EnemyAnimationController GetAnimationController() => animationController;

    /// <summary>
    /// 攻撃状態に切り替える
    /// </summary>
    public void SwitchToAttack() => stateMachine.ChangeState(stateMachineSO.attackState);

    /// <summary>
    /// 移動状態に切り替える
    /// </summary>
    public void SwitchToMove() => stateMachine.ChangeState(stateMachineSO.moveState);
}