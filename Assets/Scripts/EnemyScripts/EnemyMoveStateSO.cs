using UnityEngine;

/// <summary>
/// �G�̈ړ���Ԃ��`����ScriptableObject
/// EnemyController�̈ړ������ƈړ��A�j���[�V�����Đ����Ǘ�����
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyMoveState")]
public class EnemyMoveStateSO : EnemyStateSO
{
    /// <summary>
    /// ��Ԃɓ������Ƃ��ɌĂ΂��B�ړ��A�j���[�V�������Đ�����B
    /// </summary>
    /// <param name="owner">��Ԃ̏��L�ҁi�G�̃R���g���[���[�j</param>
    public override void Enter(EnemyController owner)
    {
        base.Enter(owner); // ���O�o�������ꍇ�͌ĂԁB�s�v�Ȃ�ȗ��\
        owner.GetAnimationController().PlayMoveAnimation();
    }

    /// <summary>
    /// ���t���[���Ă΂���Ԃ̍X�V�����B�G�̈ړ��������s���B
    /// </summary>
    /// <param name="owner">��Ԃ̏��L�ҁi�G�̃R���g���[���[�j</param>
    /// <param name="deltaTime">�O�t���[������̌o�ߎ���</param>
    public override void Tick(EnemyController owner, float deltaTime)
    {
        owner.Move();
    }

    /// <summary>
    /// ��Ԃ𔲂���Ƃ��ɌĂ΂��i�K�v�ɉ����ď�����ǉ��\�j
    /// </summary>
    /// <param name="owner">��Ԃ̏��L�ҁi�G�̃R���g���[���[�j</param>
    public override void Exit(EnemyController owner)
    {
        base.Exit(owner); // ���O�o�������ꍇ�͌ĂԁB�s�v�Ȃ�ȗ��\
    }
}
