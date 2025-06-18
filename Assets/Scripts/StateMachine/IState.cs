/// <summary>
/// ��ԃp�^�[���p�̏�ԃC���^�[�t�F�[�X
/// �W�F�l���b�N�^T�͏�Ԃ����I�[�i�[�N���X�̌^
/// </summary>
/// <typeparam name="T">��Ԃ��Ǘ�����I�[�i�[�N���X�̌^</typeparam>
public interface IState<T>
{
    /// <summary>
    /// ��Ԃɓ��������̏���������
    /// </summary>
    /// <param name="owner">��Ԃ̃I�[�i�[�I�u�W�F�N�g</param>
    void Enter(T owner);

    /// <summary>
    /// ���t���[���Ă΂���Ԃ̍X�V����
    /// </summary>
    /// <param name="owner">��Ԃ̃I�[�i�[�I�u�W�F�N�g</param>
    /// <param name="deltaTime">�O�t���[������̌o�ߎ��ԁi�b�j</param>
    void Tick(T owner, float deltaTime);

    /// <summary>
    /// ��Ԃ��甲���鎞�̏I������
    /// </summary>
    /// <param name="owner">��Ԃ̃I�[�i�[�I�u�W�F�N�g</param>
    void Exit(T owner);
}
