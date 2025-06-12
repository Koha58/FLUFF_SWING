using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// �v���C���[�̍U�����͂��Ǘ�����N���X�B
/// Input System��"Attack"�A�N�V�������Ď����A�U���R�}���h��PlayerAttack�֓`�B����B
/// </summary>
public class PlayerAttackInput : MonoBehaviour
{
    // �U��������S������PlayerAttack�R���|�[�l���g�̎Q��
    [SerializeField] private PlayerAttack playerAttack;

    // Input System��"Attack"�A�N�V����
    private InputAction attackAction;

    private void Awake()
    {
        // Input System����"Attack"�A�N�V�������擾
        attackAction = InputSystem.actions.FindAction("Attack");
        if (attackAction != null)
        {
            // "Attack"�A�N�V���������s���ꂽ�Ƃ���PlayerAttack�̍U���������Ăяo��
            attackAction.performed += ctx => playerAttack.PerformAutoAttack();

            // ���͎�t��L����
            attackAction.Enable();
        }
        else
        {
            Debug.LogWarning("playerAttack is null");
        }
    }
}
