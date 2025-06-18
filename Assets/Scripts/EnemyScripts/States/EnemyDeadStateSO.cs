using UnityEngine;

/// <summary>
/// �G�̎��S��Ԃ��`����ScriptableObject
/// �G�̎��S���̃A�j���[�V�����Đ���v�[���ւ̕ԋp�������Ǘ�����
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyDeadState")]
public class EnemyDeadStateSO : EnemyStateSO
{
    /// <summary>
    /// ���S��Ԃɓ������Ƃ��̏���
    /// �E���S�A�j���[�V����������ꍇ�͍Đ����A�A�j���[�V�����I���܂ő҂��Ă���v�[���֕ԋp
    /// �E�A�j���[�V�������Ȃ��ꍇ�͑����Ƀv�[���ԋp����
    /// </summary>
    /// <param name="owner">��Ԃ̏��L�ҁi�G�̃R���g���[���[�j</param>
    public override void Enter(EnemyController owner)
    {
        base.Enter(owner);

        if (owner.GetAnimationController() && owner.StateMachineSO.usesDead)
        {
            // ���S�A�j���[�V�������Đ�
            owner.GetAnimationController().PlayDeadAnimation();
            // �R���[�`�����g���ăA�j���[�V�����I���܂őҋ@���A���̌�v�[���ɕԋp
            owner.StartCoroutine(WaitAndHandleDead(owner));
        }
        else
        {
            // �A�j���[�V�����Ȃ��Ȃ瑦���Ƀv�[���֕ԋp
            owner.HandleDead();
        }
    }

    /// <summary>
    /// ���S�A�j���[�V�����I���܂őҋ@���A�v�[���ɕԋp����R���[�`��
    /// </summary>
    /// <param name="owner">�G�R���g���[���[</param>
    /// <returns>IEnumerator</returns>
    private System.Collections.IEnumerator WaitAndHandleDead(EnemyController owner)
    {
        // Animator�R���|�[�l���g���擾
        Animator animator = owner.GetAnimationController().GetComponent<Animator>();
        if (animator != null)
        {
            // ���݂̃A�j���[�V������Ԃ��擾
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // �A�j���[�V�����̒������擾�i�b�j
            float animationLength = stateInfo.length;

            // ���S�A�j���[�V�����̒������擾���đҋ@������@�̗�i�Œ�b���ł��j
            // ���󋵂ɂ���Ă͎��S�A�j���[�V���������w�肵�Đ��m�Ȓ������擾�����ق����ǂ�
            yield return new WaitForSeconds(animationLength);

            // �������͌Œ�b���҂�
            // yield return new WaitForSeconds(1.0f);
        }
        else
        {
            // Animator���擾�ł��Ȃ����1�t���[���҂����ɂ���
            yield return null;
        }

        // �ҋ@��A�v�[���ɕԋp
        owner.HandleDead();
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
