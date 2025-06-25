using UnityEngine;

/// <summary>
/// �G�L�����N�^�[�p�̃X�e�[�g�}�V���ݒ�f�[�^
/// ScriptableObject�Ƃ��č쐬���A�G�̏�ԑJ�ڂɎg���X�e�[�g���܂Ƃ߂ĕێ�����
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyStateMachine")]
public class EnemyStateMachineSO : ScriptableObject
{
    [Header("�g�p�����Ԃ̃t���O")]
    public bool usesMove;    // �ړ���Ԃ��g�����ǂ���
    public bool usesAttack;  // �U����Ԃ��g�����ǂ���
    public bool usesCut;     // ���C���[�ؒf��Ԃ��g�����ǂ���
    public bool usesDead;    // ���S��Ԃ��g�����ǂ���

    [Header("��ԃI�u�W�F�N�g")]
    public EnemyMoveStateSO moveState;     // �ړ���Ԃ̐ݒ�f�[�^
    public EnemyAttackStateSO attackState; // �U����Ԃ̐ݒ�f�[�^
    public EnemyCutWireStateSO cutState;    // ���C���[�ؒf��Ԃ̐ݒ�f�[�^
    public EnemyDeadStateSO deadState;     // ���S��Ԃ̐ݒ�f�[�^
}
