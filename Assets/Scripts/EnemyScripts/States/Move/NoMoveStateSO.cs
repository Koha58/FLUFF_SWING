using UnityEngine;

/// <summary>
/// �G�L�����N�^�[�́u�ړ����Ȃ��X�e�[�g�v�B
/// EnemyMoveStateSO ���p�����邪�A Tick ���ňړ��������s��Ȃ��B
/// �{�X��Œ�C��Ȃǂ́u���̏�ōU���݂̂��s���G�v�Ɏg�p����B
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyMove/NoMoveState")]
public class NoMoveStateSO : EnemyMoveStateSO
{
    /// <summary>
    /// ���t���[���Ă΂�邪�A�ړ������͍s��Ȃ��B
    /// </summary>
    /// <param name="owner">�X�e�[�g�����G�L�����N�^�[</param>
    /// <param name="deltaTime">�o�ߎ���</param>
    public override void Tick(EnemyController owner, float deltaTime)
    {
        // �ړ������Ȃ��i�Î~��ԁj
    }
}
