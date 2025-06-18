using UnityEngine;

/// <summary>
/// �G�L�����N�^�[�̒P���ȍ��E�ړ��X�e�[�g�B
/// EnemyMoveStateSO ���p�����A�ړ����W�b�N�� Tick �Œ�`����B
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyMove/SimpleMoveState")]
public class SimpleMoveStateSO : EnemyMoveStateSO
{
    /// <summary>
    /// ���t���[���Ă΂��ړ������B
    /// �A�j���[�V�����C�x���g�ňړ����֎~����Ă��Ȃ���΁A
    /// �������Ɉ�葬�x�ňړ�����B
    /// </summary>
    /// <param name="owner">�X�e�[�g�����G�L�����N�^�[</param>
    /// <param name="deltaTime">�o�ߎ���</param>
    public override void Tick(EnemyController owner, float deltaTime)
    {
        // �A�j���[�V�����C�x���g���͈ړ����s��Ȃ�
        if (owner.IsMovementDisabledByAnimation)
        {
            return;
        }

        // �������ֈ�葬�x�ňړ�����
        owner.transform.Translate(Vector2.left * owner.MoveSpeed * deltaTime);
    }
}
