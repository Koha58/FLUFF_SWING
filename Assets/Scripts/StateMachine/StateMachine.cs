/// <summary>
/// �ėp�I�ȃX�e�[�g�}�V���̃N���X
/// �W�F�l���b�N�^T�͏�Ԃ��Ǘ�����I�[�i�[�N���X�̌^
/// </summary>
/// <typeparam name="T">��Ԃ��Ǘ�����I�[�i�[�N���X�̌^</typeparam>
public class StateMachine<T>
{
    private T owner;                 // ���̃X�e�[�g�}�V�����Ǘ�����I�[�i�[�N���X�̃C���X�^���X
    private IState<T> currentState;  // ���݂̏�ԁiState�j

    /// <summary>
    /// �R���X�g���N�^�B�I�[�i�[�N���X�̃C���X�^���X���󂯎��
    /// </summary>
    /// <param name="owner">��Ԃ��Ǘ�����I�[�i�[�N���X�̃C���X�^���X</param>
    public StateMachine(T owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// ��Ԃ�؂�ւ���
    /// </summary>
    /// <param name="newState">�V�����J�ڂ�����</param>
    public void ChangeState(IState<T> newState)
    {
        // ���݂̏�Ԃ��甲���鏈�����Ă�
        currentState?.Exit(owner);

        // �V������Ԃɐ؂�ւ�
        currentState = newState;

        // �V������Ԃ̏������������Ă�
        currentState?.Enter(owner);
    }

    /// <summary>
    /// ���t���[���Ă΂��X�V����
    /// ���݂̏�Ԃ�Tick���Ă�
    /// </summary>
    /// <param name="deltaTime">�O�t���[������̌o�ߎ��ԁi�b�j</param>
    public void Update(float deltaTime)
    {
        currentState?.Tick(owner, deltaTime);
    }
}
