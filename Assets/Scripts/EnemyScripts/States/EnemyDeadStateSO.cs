using UnityEngine;

/// <summary>
/// 敵の死亡状態を定義するScriptableObject
/// 敵の死亡時のアニメーション再生やプールへの返却処理を管理する
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyDeadState")]
public class EnemyDeadStateSO : EnemyStateSO
{
    /// <summary>
    /// 状態に入ったときに呼ばれる。
    /// 死亡アニメーションが使用可能なら再生し、死亡処理を行う。
    /// アニメーションが使えない場合は即座に死亡処理を行う。
    /// </summary>
    /// <param name="owner">状態の所有者（敵のコントローラー）</param>
    public override void Enter(EnemyController owner)
    {
        base.Enter(owner);

        if (owner.GetAnimationController() && owner.StateMachineSO.usesDead)
        {
            // 死亡アニメーションを再生
            owner.GetAnimationController().PlayDeadAnimation();
            // 死亡処理（プールへ戻すなど）
            owner.HandleDead();
        }
        else
        {
            // アニメーションなしで即死亡処理
            owner.HandleDead();
        }
    }

    /// <summary>
    /// 死亡状態の更新処理（特に何もしない）
    /// </summary>
    /// <param name="owner">状態の所有者</param>
    /// <param name="deltaTime">経過時間</param>
    public override void Tick(EnemyController owner, float deltaTime) { }

    /// <summary>
    /// 状態から抜けるときの処理（特になし）
    /// </summary>
    /// <param name="owner">状態の所有者</param>
    public override void Exit(EnemyController owner) { }
}
