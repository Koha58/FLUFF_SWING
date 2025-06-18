using UnityEngine;

/// <summary>
/// 敵の攻撃状態を定義するScriptableObject
/// EnemyControllerの攻撃処理と攻撃アニメーション再生を管理する
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyAttackState")]
public class EnemyAttackStateSO : EnemyStateSO
{
    /// <summary>
    /// 状態に入ったときに呼ばれる。攻撃アニメーションを再生する。
    /// </summary>
    /// <param name="owner">状態の所有者（敵のコントローラー）</param>
    public override void Enter(EnemyController owner) => owner.GetAnimationController().PlayAttackAnimation();

    /// <summary>
    /// 毎フレーム呼ばれる状態の更新処理。攻撃可能なら攻撃を実行する。
    /// </summary>
    /// <param name="owner">状態の所有者（敵のコントローラー）</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public override void Tick(EnemyController owner, float deltaTime) => owner.AttackIfPossible();

    /// <summary>
    /// 状態を抜けるときに呼ばれる。特に処理なし。
    /// </summary>
    /// <param name="owner">状態の所有者（敵のコントローラー）</param>
    public override void Exit(EnemyController owner) { }
}
