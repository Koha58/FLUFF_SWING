using UnityEngine;

/// <summary>
/// �G�L�����N�^�[�̃p�g���[���ړ��X�e�[�g�B
/// ���͈͓������E�ɉ����ړ����郍�W�b�N�����B
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyMove/PatrolMoveState")]
public class PatrolMoveStateSO : EnemyMoveStateSO
{
    /// <summary>
    /// �p�g���[������͈͂̕��i���E�̈ړ������j
    /// </summary>
    public float patrolRange = 3f;

    /// <summary>
    /// �X�e�[�g�J�n���ɏ����ʒu�ƈړ�������ݒ肷��B
    /// </summary>
    /// <param name="owner">�X�e�[�g�����G�L�����N�^�[</param>
    public override void Enter(EnemyController owner)
    {
        base.Enter(owner);

        // ���݈ʒu���p�g���[���J�n�ʒu�Ƃ��ċL�^
        owner.PatrolStartX = owner.transform.position.x;

        // �����̈ړ����������ɐݒ�
        owner.PatrolDirection = -1;
    }

    /// <summary>
    /// ���t���[���Ă΂��p�g���[���ړ������B
    /// �͈͂𒴂���������𔽓]���A�X�v���C�g�����]����B
    /// </summary>
    /// <param name="owner">�X�e�[�g�����G�L�����N�^�[</param>
    /// <param name="deltaTime">�o�ߎ���</param>
    public override void Tick(EnemyController owner, float deltaTime)
    {
        // �A�j���[�V�����C�x���g�ňړ����֎~����Ă���ꍇ�͈ړ����Ȃ�
        if (owner.IsMovementDisabledByAnimation) return;

        // ���݂̕����Ɍ������Ĉړ�
        owner.transform.Translate(Vector2.right * owner.PatrolDirection * owner.MoveSpeed * deltaTime);

        // �p�g���[���͈͂𒴂���������𔽓]
        if (Mathf.Abs(owner.transform.position.x - owner.PatrolStartX) >= patrolRange)
        {
            owner.PatrolDirection *= -1;

            // �X�v���C�g�����E���]
            Flip(owner);
        }
    }

    /// <summary>
    /// �X�e�[�g�I�����̏����B
    /// ���N���X�� Exit ���Ăяo���B
    /// </summary>
    /// <param name="owner">�X�e�[�g�����G�L�����N�^�[</param>
    public override void Exit(EnemyController owner)
    {
        base.Exit(owner);
    }

    /// <summary>
    /// �G�L�����N�^�[�̃X�v���C�g�����E���]����B
    /// </summary>
    /// <param name="owner">�X�e�[�g�����G�L�����N�^�[</param>
    private void Flip(EnemyController owner)
    {
        Vector3 scale = owner.transform.localScale;
        scale.x *= -1;
        owner.transform.localScale = scale;
    }
}
