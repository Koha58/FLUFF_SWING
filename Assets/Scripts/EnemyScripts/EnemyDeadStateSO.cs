using UnityEngine;

/// <summary>
/// �G�̎��S��Ԃ��`����ScriptableObject
/// �G�̎��S���̃A�j���[�V�����Đ���v�[���ւ̕ԋp�������Ǘ�����
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyDeadState")]
public class EnemyDeadStateSO : EnemyStateSO
{
    /// <summary>
    /// ��Ԃɓ������Ƃ��ɌĂ΂��B
    /// ���S�A�j���[�V�������g�p�\�Ȃ�Đ����A���S�������s���B
    /// �A�j���[�V�������g���Ȃ��ꍇ�͑����Ɏ��S�������s���B
    /// </summary>
    /// <param name="owner">��Ԃ̏��L�ҁi�G�̃R���g���[���[�j</param>
    public override void Enter(EnemyController owner)
    {
        base.Enter(owner);

        if (owner.GetAnimationController() && owner.StateMachineSO.usesDead)
        {
            // ���S�A�j���[�V�������Đ�
            owner.GetAnimationController().PlayDeadAnimation();
            // ���S�����i�v�[���֖߂��Ȃǁj
            owner.HandleDead();
        }
        else
        {
            // �A�j���[�V�����Ȃ��ő����S����
            owner.HandleDead();
        }
    }

    /// <summary>
    /// ���S��Ԃ̍X�V�����i���ɉ������Ȃ��j
    /// </summary>
    /// <param name="owner">��Ԃ̏��L��</param>
    /// <param name="deltaTime">�o�ߎ���</param>
    public override void Tick(EnemyController owner, float deltaTime) { }

    /// <summary>
    /// ��Ԃ��甲����Ƃ��̏����i���ɂȂ��j
    /// </summary>
    /// <param name="owner">��Ԃ̏��L��</param>
    public override void Exit(EnemyController owner) { }
}
