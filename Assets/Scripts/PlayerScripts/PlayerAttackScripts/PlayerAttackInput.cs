using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// �v���C���[�̍U�����͂��Ǘ�����N���X�B
/// Input System��"Attack"�A�N�V�������Ď����A�U���R�}���h��PlayerAttack�֓`�B����B
/// �n�ʂɐڒn���Ă���ꍇ�̂ݍU�������s�����B
/// </summary>
public class PlayerAttackInput : MonoBehaviour
{
    /// <summary>�U��������S������ PlayerAttack �R���|�[�l���g</summary>
    [SerializeField] private PlayerAttack playerAttack;

    /// <summary>�v���C���[�̐ڒn��Ԃ��m�F���邽�߂� PlayerMove �R���|�[�l���g</summary>
    [SerializeField] private PlayerMove playerMove;

    /// <summary>�U�����̓A�N�V�����iInput System��"Attack"�j</summary>
    private InputAction attackAction;

    [SerializeField] private PlayerAnimatorController animatorController; public PlayerAnimatorController AnimatorController => animatorController;

    /// <summary>
    /// �����������iInput System����A�N�V�������擾���ăC�x���g�o�^�j
    /// </summary>
    private void Awake()
    {
        // Input System����"Attack"�A�N�V�������擾
        attackAction = InputSystem.actions.FindAction("Attack");

        if (attackAction != null)
        {
            // "Attack"���͂����ꂽ�Ƃ��̃R�[���o�b�N��o�^
            attackAction.performed += ctx =>
            {
                if (playerMove == null || playerAttack == null || animatorController == null) return;

                // �ڒn���Ă���ꍇ�̂ݍU�����������s
                if (playerMove != null && playerMove.IsGrounded && animatorController.CurrentState != PlayerAnimatorController.PlayerState.Landing)
                {
                    playerAttack.PerformAutoAttack();
                }
                else
                {
                    Debug.Log("�󒆂܂��̓��C���[���̂��ߍU���s��");
                }
            };

            // �A�N�V������L����
            attackAction.Enable();
        }
        else
        {
            Debug.LogWarning("Attack�A�N�V������������܂���");
        }
    }
}
