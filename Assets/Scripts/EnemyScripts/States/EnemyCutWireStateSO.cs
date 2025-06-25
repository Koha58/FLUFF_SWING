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
        // Cut �� Tick �ł͂Ȃ��A�j���I���ōs���̂ł����͋�
    }

    public override void Exit(EnemyController owner)
    {
        // �A�j����~�� OnAttackAnimationEnd �ōς�
    }
}
