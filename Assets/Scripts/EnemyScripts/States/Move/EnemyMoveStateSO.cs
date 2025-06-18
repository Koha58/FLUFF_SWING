/// <summary>
/// �G�L�����N�^�[�́u�ړ��X�e�[�g�v��\�����ۊ��N���X�B
/// ��ԃ}�V�����ŋ��ʂ̈ړ��A�j���[�V�����������������B
/// </summary>
public abstract class EnemyMoveStateSO : EnemyStateSO
{
    /// <summary>
    /// �ړ��X�e�[�g�ɓ������ۂɌĂ΂�鏈���B
    /// �ړ��A�j���[�V�������Đ�����B
    /// </summary>
    /// <param name="owner">�X�e�[�g�����G�L�����N�^�[</param>
    public override void Enter(EnemyController owner)
    {
        // �ړ��A�j���[�V�������Đ�����
        owner.GetAnimationController().PlayMoveAnimation();
    }

    /// <summary>
    /// �ړ��X�e�[�g���甲����ۂɌĂ΂�鏈���B
    /// �ړ��A�j���[�V�������~����B
    /// </summary>
    /// <param name="owner">�X�e�[�g�����G�L�����N�^�[</param>
    public override void Exit(EnemyController owner)
    {
        // �ړ��A�j���[�V�������~����
        owner.GetAnimationController().StopMoveAnimation();
    }
}
