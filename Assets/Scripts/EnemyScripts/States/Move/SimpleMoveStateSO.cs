using UnityEngine;

[CreateAssetMenu(menuName = "State/EnemyMove/SimpleMoveState")]
public class SimpleMoveStateSO : EnemyMoveStateSO
{
    public override void Tick(EnemyController owner, float deltaTime)
    {
        if (owner.IsMovementDisabledByAnimation)
        {
            return; // �A�j���C�x���g���͈ړ����Ȃ�
        }

        owner.transform.Translate(Vector2.left * owner.MoveSpeed * deltaTime);
    }

}
