using UnityEngine;

[CreateAssetMenu(menuName = "State/EnemyCutWireState")]
public class EnemyCutWireStateSO : EnemyStateSO
{
    public override void Enter(EnemyController owner)
    {
        owner.GetAnimationController().PlayAttackAnimation();
    }

    public override void Tick(EnemyController owner, float deltaTime)
    {
        // Cut は Tick ではなくアニメ終了で行うのでここは空
    }

    public override void Exit(EnemyController owner)
    {
        // アニメ停止は OnAttackAnimationEnd で済む
    }
}
