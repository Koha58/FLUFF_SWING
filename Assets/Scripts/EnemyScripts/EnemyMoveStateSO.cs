using UnityEngine;

/// <summary>
/// 敵の移動状態を定義するScriptableObject
/// EnemyControllerの移動処理と移動アニメーション再生を管理する
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyMoveState")]
public class EnemyMoveStateSO : EnemyStateSO
{
    /// <summary>
    /// 状態に入ったときに呼ばれる。移動アニメーションを再生する。
    /// </summary>
    /// <param name="owner">状態の所有者（敵のコントローラー）</param>
    public override void Enter(EnemyController owner)
    {
        base.Enter(owner); // ログ出したい場合は呼ぶ。不要なら省略可能
        owner.GetAnimationController().PlayMoveAnimation();
    }

    /// <summary>
    /// 毎フレーム呼ばれる状態の更新処理。敵の移動処理を行う。
    /// </summary>
    /// <param name="owner">状態の所有者（敵のコントローラー）</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public override void Tick(EnemyController owner, float deltaTime)
    {
        owner.Move();
    }

    /// <summary>
    /// 状態を抜けるときに呼ばれる（必要に応じて処理を追加可能）
    /// </summary>
    /// <param name="owner">状態の所有者（敵のコントローラー）</param>
    public override void Exit(EnemyController owner)
    {
        base.Exit(owner); // ログ出したい場合は呼ぶ。不要なら省略可能
    }
}
