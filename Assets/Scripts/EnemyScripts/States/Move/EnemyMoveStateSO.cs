public abstract class EnemyMoveStateSO : EnemyStateSO
{
    public override void Enter(EnemyController owner)
    {
        owner.GetAnimationController().PlayMoveAnimation();
    }

    public override void Exit(EnemyController owner)
    {
        owner.GetAnimationController().StopMoveAnimation();
    }
}
