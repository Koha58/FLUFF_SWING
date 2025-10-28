using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの攻撃入力を処理するクラス。
/// Input System（新入力システム）を使用しており、
/// 攻撃アクション入力を検知して PlayerAttack / PlayerAnimatorController に伝える。
/// </summary>
public class PlayerAttackInput : MonoBehaviour
{
    [Header("参照コンポーネント")]
    [SerializeField] private PlayerAttack playerAttack;               // 実際の攻撃ロジック（近接・遠距離攻撃処理など）
    [SerializeField] private PlayerMove playerMove;                   // プレイヤーの移動・接地判定を参照
    [SerializeField] private PlayerAnimatorController animatorController; // 攻撃可否やアニメーション制御を参照

    private InputAction attackAction; // Input SystemのAttackアクション

    private void Awake()
    {
        // Input SystemのActionMapから「Attack」アクションを取得
        attackAction = InputSystem.actions.FindAction("Attack");

        if (attackAction != null)
        {
            // Attack入力（ボタン押下）が発生したときに呼ばれるコールバックを登録
            attackAction.performed += ctx =>
            {
                // 必須参照が欠けていたら何もしない
                if (playerMove == null || playerAttack == null || animatorController == null) return;

                // ✅ 攻撃可能かどうかをAnimator側でチェック
                // （攻撃中・ダメージ中・ゴール中などは攻撃禁止）
                if (!animatorController.CanAttackNow())
                {
                    Debug.Log("攻撃不可状態（Damage, Goal, 攻撃中など）");
                    return;
                }

                // ✅ Landingアニメーション再生中は攻撃入力を完全に無視
                // → 着地モーション中はアニメーション整合性を保つため攻撃禁止
                if (animatorController.IsPlayingLanding())
                {
                    Debug.Log("Landingアニメ再生中のため攻撃不可");
                    return;
                }

                // ✅ 接地していて、かつLandingでなければ攻撃可能
                // （空中・ワイヤー中・スイング中などは攻撃を封じる）
                if (playerMove.IsGrounded)
                {
                    // PlayerAttackクラスの自動攻撃処理を実行
                    // → 状況に応じて近接/遠距離攻撃を自動選択する
                    playerAttack.PerformAutoAttack();
                }
                else
                {
                    Debug.Log("空中またはワイヤー中のため攻撃不可");
                }
            };

            // Attackアクションを有効化
            attackAction.Enable();
        }
        else
        {
            Debug.LogWarning("Attackアクションが見つかりません。InputActionsにAttackが定義されているか確認してください。");
        }
    }
}
