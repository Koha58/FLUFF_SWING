using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackInput : MonoBehaviour
{
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private PlayerAnimatorController animatorController;

    private InputAction attackAction;

    private void Awake()
    {
        attackAction = InputSystem.actions.FindAction("Attack");

        if (attackAction != null)
        {
            attackAction.performed += ctx =>
            {
                if (playerMove == null || playerAttack == null || animatorController == null) return;

                // ✅ Animatorに問い合わせて攻撃可能か確認
                if (!animatorController.CanAttackNow())
                {
                    Debug.Log("攻撃不可状態（Landing, Attack中など）");
                    return;
                }

                // ✅ Landingアニメ中は完全に入力無視
                if (animatorController.IsPlayingLanding())
                {
                    Debug.Log("Landingアニメ再生中のため攻撃不可");
                    return;
                }

                // ✅ 接地していてLandingでなければ攻撃
                if (playerMove.IsGrounded)
                {
                    playerAttack.PerformAutoAttack();
                }
                else
                {
                    Debug.Log("空中またはワイヤー中のため攻撃不可");
                }
            };

            attackAction.Enable();
        }
        else
        {
            Debug.LogWarning("Attackアクションが見つかりません");
        }
    }


}
