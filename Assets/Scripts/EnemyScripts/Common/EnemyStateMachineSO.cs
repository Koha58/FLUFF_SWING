using UnityEngine;

/// <summary>
/// 敵キャラクター用のステートマシン設定データ
/// ScriptableObjectとして作成し、敵の状態遷移に使うステートをまとめて保持する
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyStateMachine")]
public class EnemyStateMachineSO : ScriptableObject
{
    [Header("使用する状態のフラグ")]
    public bool usesMove;    // 移動状態を使うかどうか
    public bool usesAttack;  // 攻撃状態を使うかどうか
    public bool usesCut;     // ワイヤー切断状態を使うかどうか
    public bool usesDead;    // 死亡状態を使うかどうか

    [Header("状態オブジェクト")]
    public EnemyMoveStateSO moveState;     // 移動状態の設定データ
    public EnemyAttackStateSO attackState; // 攻撃状態の設定データ
    public EnemyCutWireStateSO cutState;    // ワイヤー切断状態の設定データ
    public EnemyDeadStateSO deadState;     // 死亡状態の設定データ
}
