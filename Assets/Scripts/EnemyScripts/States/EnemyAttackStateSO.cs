using UnityEngine;

/// <summary>
/// �G�̍U����Ԃ��`����ScriptableObject
/// EnemyController�̍U�������ƍU���A�j���[�V�����Đ����Ǘ�����
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyAttackState")]
public class EnemyAttackStateSO : EnemyStateSO
{
    /// <summary>
    /// ��Ԃɓ������Ƃ��ɌĂ΂��B�U���A�j���[�V�������Đ�����B
    /// </summary>
    /// <param name="owner">��Ԃ̏��L�ҁi�G�̃R���g���[���[�j</param>
    public override void Enter(EnemyController owner) => owner.GetAnimationController().PlayAttackAnimation();

    /// <summary>
    /// ���t���[���Ă΂���Ԃ̍X�V�����B�U���\�Ȃ�U�������s����B
    /// </summary>
    /// <param name="owner">��Ԃ̏��L�ҁi�G�̃R���g���[���[�j</param>
    /// <param name="deltaTime">�O�t���[������̌o�ߎ���</param>
    public override void Tick(EnemyController owner, float deltaTime) => owner.AttackIfPossible();

    /// <summary>
    /// ��Ԃ𔲂���Ƃ��ɌĂ΂��B���ɏ����Ȃ��B
    /// </summary>
    /// <param name="owner">��Ԃ̏��L�ҁi�G�̃R���g���[���[�j</param>
    public override void Exit(EnemyController owner) { }
}
