using UnityEngine;

[CreateAssetMenu(menuName = "State/EnemyMove/PatrolMoveState")]
public class PatrolMoveStateSO : EnemyMoveStateSO
{
    public float patrolRange = 3f;

    public override void Enter(EnemyController owner)
    {
        base.Enter(owner);
        owner.PatrolStartX = owner.transform.position.x;
        owner.PatrolDirection = -1;
    }

    public override void Tick(EnemyController owner, float deltaTime)
    {
        if (owner.IsMovementDisabledByAnimation) return;

        owner.transform.Translate(Vector2.right * owner.PatrolDirection * owner.MoveSpeed * deltaTime);

        if (Mathf.Abs(owner.transform.position.x - owner.PatrolStartX) >= patrolRange)
        {
            owner.PatrolDirection *= -1;
            Flip(owner);
        }
    }

    public override void Exit(EnemyController owner)
    {
        base.Exit(owner);
    }

    private void Flip(EnemyController owner)
    {
        Vector3 scale = owner.transform.localScale;
        scale.x *= -1;
        owner.transform.localScale = scale;
    }
}
