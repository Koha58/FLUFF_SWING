using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの攻撃入力を管理するクラス。
/// Input Systemの"Attack"アクションを監視し、攻撃コマンドをPlayerAttackへ伝達する。
/// </summary>
public class PlayerAttackInput : MonoBehaviour
{
    // 攻撃処理を担当するPlayerAttackコンポーネントの参照
    [SerializeField] private PlayerAttack playerAttack;

    // Input Systemの"Attack"アクション
    private InputAction attackAction;

    private void Awake()
    {
        // Input Systemから"Attack"アクションを取得
        attackAction = InputSystem.actions.FindAction("Attack");
        if (attackAction != null)
        {
            // "Attack"アクションが実行されたときにPlayerAttackの攻撃処理を呼び出す
            attackAction.performed += ctx => playerAttack.PerformAutoAttack();

            // 入力受付を有効化
            attackAction.Enable();
        }
        else
        {
            Debug.LogWarning("playerAttack is null");
        }
    }
}
