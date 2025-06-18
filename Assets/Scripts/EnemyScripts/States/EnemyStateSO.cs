using UnityEngine;

/// <summary>
/// Enemy専用のステート基底クラス
/// StateSO<EnemyController>を継承し、Enemyの状態管理で共通して行いたい処理をまとめる
/// </summary>
public abstract class EnemyStateSO : StateSO<EnemyController>
{
    /// <summary>
    /// 状態に入ったときに呼ばれるメソッド
    /// Enemy共通でログ出力などの共通処理をここで実装可能
    /// </summary>
    /// <param name="owner">状態の所有者（EnemyController）</param>
    public override void Enter(EnemyController owner)
    {
        Debug.Log($"Entering state: {name} for {owner.name}");
    }

    /// <summary>
    /// 状態を抜けるときに呼ばれるメソッド
    /// Enemy共通でログ出力などの共通処理をここで実装可能
    /// </summary>
    /// <param name="owner">状態の所有者（EnemyController）</param>
    public override void Exit(EnemyController owner)
    {
        Debug.Log($"Exiting state: {name} for {owner.name}");
    }
}
