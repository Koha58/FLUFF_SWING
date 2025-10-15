using UnityEngine;

/// <summary>
/// �G�̎��S��Ԃ��`����ScriptableObject�B
/// ���S�A�j���[�V�����̍Đ��ƁA�I����̃v�[���ԋp���Ǘ�����B
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyDeadState")]
public class EnemyDeadStateSO : EnemyStateSO
{
    [Header("���S�����ݒ�")]
    [Tooltip("���S�A�j���[�V�����Đ���ɑҋ@����b���i�A�j���[�V�������Ȃ��ꍇ�͖����j")]
    [SerializeField] private float waitAfterDeathAnimation = 1.5f;

    /// <summary>
    /// ���S��Ԃɓ������Ƃ��̏����B
    /// �E���S�A�j���[�V�������Đ����A�I����҂��Ă���v�[���֕ԋp�B
    /// �E�A�j���[�V�������Ȃ��ꍇ�͑����Ƀv�[���ԋp�B
    /// </summary>
    public override void Enter(EnemyController owner)
    {
        base.Enter(owner);

        if (owner.GetAnimationController() && owner.StateMachineSO.usesDead)
        {
            // ���S�A�j���[�V�������Đ�
            owner.GetAnimationController().PlayDeadAnimation();

            // �Đ���A��莞�ԑ҂��Ă���v�[���ԋp
            owner.StartCoroutine(WaitAndHandleDead(owner));
        }
        else
        {
            // �A�j���[�V�����Ȃ� �� ���ԋp
            owner.HandleDead();
        }
    }

    /// <summary>
    /// ���S�A�j���[�V�����I����Ƀv�[���ԋp����R���[�`���B
    /// </summary>
    private System.Collections.IEnumerator WaitAndHandleDead(EnemyController owner)
    {
        Animator animator = owner.GetAnimationController()?.GetComponent<Animator>();

        if (animator != null)
        {
            // ���ݍĐ����A�j���[�V�����̏����擾
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            yield return new WaitForSeconds(waitAfterDeathAnimation);
        }
        else
        {
            // Animator���Ȃ��ꍇ��1�t���[�������ҋ@
            yield return null;
        }

        // �ҋ@��Ƀv�[���֕ԋp
        owner.HandleDead();
    }

    public override void Tick(EnemyController owner, float deltaTime) { }

    public override void Exit(EnemyController owner) { }
}
