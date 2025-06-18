/// <summary>
/// 敵キャラクターの「移動ステート」を表す抽象基底クラス。
/// 状態マシン内で共通の移動アニメーション制御を実装する。
/// </summary>
public abstract class EnemyMoveStateSO : EnemyStateSO
{
    /// <summary>
    /// 移動ステートに入った際に呼ばれる処理。
    /// 移動アニメーションを再生する。
    /// </summary>
    /// <param name="owner">ステートを持つ敵キャラクター</param>
    public override void Enter(EnemyController owner)
    {
        // 移動アニメーションを再生する
        owner.GetAnimationController().PlayMoveAnimation();
    }

    /// <summary>
    /// 移動ステートから抜ける際に呼ばれる処理。
    /// 移動アニメーションを停止する。
    /// </summary>
    /// <param name="owner">ステートを持つ敵キャラクター</param>
    public override void Exit(EnemyController owner)
    {
        // 移動アニメーションを停止する
        owner.GetAnimationController().StopMoveAnimation();
    }
}
