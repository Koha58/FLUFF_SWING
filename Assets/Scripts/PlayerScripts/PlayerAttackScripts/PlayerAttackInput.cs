using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの攻撃入力を管理するクラス。
/// Input Systemの"Attack"アクションを監視し、攻撃コマンドをPlayerAttackへ伝達する。
/// 地面に接地している場合のみ攻撃が実行される。
/// </summary>
public class PlayerAttackInput : MonoBehaviour
{
    /// <summary>攻撃処理を担当する PlayerAttack コンポーネント</summary>
    [SerializeField] private PlayerAttack playerAttack;

    /// <summary>プレイヤーの接地状態を確認するための PlayerMove コンポーネント</summary>
    [SerializeField] private PlayerMove playerMove;

    /// <summary>攻撃入力アクション（Input Systemの"Attack"）</summary>
    private InputAction attackAction;

    [SerializeField] private PlayerAnimatorController animatorController; public PlayerAnimatorController AnimatorController => animatorController;

    /// <summary>
    /// 初期化処理（Input Systemからアクションを取得してイベント登録）
    /// </summary>
    private void Awake()
    {
        // Input Systemから"Attack"アクションを取得
        attackAction = InputSystem.actions.FindAction("Attack");

        if (attackAction != null)
        {
            // "Attack"入力がされたときのコールバックを登録
            attackAction.performed += ctx =>
            {
                if (playerMove == null || playerAttack == null || animatorController == null) return;

                // 接地している場合のみ攻撃処理を実行
                if (playerMove != null && playerMove.IsGrounded && animatorController.CurrentState != PlayerAnimatorController.PlayerState.Landing)
                {
                    playerAttack.PerformAutoAttack();
                }
                else
                {
                    Debug.Log("空中またはワイヤー中のため攻撃不可");
                }
            };

            // アクションを有効化
            attackAction.Enable();
        }
        else
        {
            Debug.LogWarning("Attackアクションが見つかりません");
        }
    }
}
