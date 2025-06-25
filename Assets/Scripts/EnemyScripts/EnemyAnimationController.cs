using UnityEngine;

/// <summary>
/// �G�L�����N�^�[�̃A�j���[�V��������N���X
/// Animator�̃p�����[�^���n�b�V�������ĊǗ����A�ړ��E�U���E���S�̃A�j���[�V�������Đ��E��~����
/// </summary>
public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;   // Animator�R���|�[�l���g�Q��

    private int moveParamHash;    // �ړ��A�j���[�V�����p�p�����[�^�̃n�b�V���l
    private int attackParamHash;  // �U���A�j���[�V�����p�p�����[�^�̃n�b�V���l
    private int deadParamHash;    // ���S�A�j���[�V�����p�p�����[�^�̃n�b�V���l

    [SerializeField] private bool usesMove = false;    // �ړ��A�j���[�V�������g�p���邩
    [SerializeField] private bool usesAttack = false;  // �U���A�j���[�V�������g�p���邩
    [SerializeField] private bool usesDead = false;    // ���S�A�j���[�V�������g�p���邩

    private bool canUseMove = false; // Attack ���Ă΂ꂽ�� true �ɂ���

    private void Awake()
    {
        // �g�p����A�j���[�V�����ɑΉ������p�����[�^�����n�b�V�����i�����A�N�Z�X�p�j
        if (usesMove)
            moveParamHash = Animator.StringToHash("IsMove");
        if (usesAttack)
            attackParamHash = Animator.StringToHash("IsAttack");
        if (usesDead)
            deadParamHash = Animator.StringToHash("IsDead");
    }

    /// <summary>
    /// �S�ẴA�j���[�V������Ԃ����Z�b�g����
    /// </summary>
    private void ResetAnimationStates()
    {
        if (usesMove && moveParamHash != 0)
            animator.SetBool(moveParamHash, false);
        if (usesAttack && attackParamHash != 0)
            animator.SetBool(attackParamHash, false);
        // Dead��Trigger�Ȃ̂Ŗ��������Ȃ�
    }

    /// <summary>
    /// �ړ��A�j���[�V�������J�n����
    /// </summary>
    public void PlayMoveAnimation()
    {
        // Attack�Ă΂��܂Ŗ���
        if (!usesMove || !canUseMove) return;

        ResetAnimationStates();

        if (usesMove)
            animator.SetBool(moveParamHash, true);
    }

    /// <summary>
    /// �ړ��A�j���[�V�������~����
    /// </summary>
    public void StopMoveAnimation()
    {
        if (usesMove)
            animator.SetBool(moveParamHash, false);
    }

    /// <summary>
    /// �U���A�j���[�V�������J�n����
    /// </summary>
    public void PlayAttackAnimation()
    {
        if (!usesAttack) return;

        ResetAnimationStates();

        if (usesAttack)
            animator.SetBool(attackParamHash, true);

        // Attack���Ă񂾂̂ňȍ~Move��L���ɂ���
        canUseMove = true;
    }

    /// <summary>
    /// �U���A�j���[�V�������~����
    /// </summary>
    public void StopAttackAnimation()
    {
        if (usesAttack)
            animator.SetBool(attackParamHash, false);
    }

    /// <summary>
    /// ���S�A�j���[�V�������Đ�����iTrigger���Z�b�g�j
    /// </summary>
    public void PlayDeadAnimation()
    {
        ResetAnimationStates();

        if (usesDead)
            animator.SetTrigger(deadParamHash);
    }
}