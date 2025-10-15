using UnityEngine;

/// <summary>
/// 敵の死亡状態を定義するScriptableObject。
/// 死亡アニメーションの再生と、終了後のプール返却を管理する。
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyDeadState")]
public class EnemyDeadStateSO : EnemyStateSO
{
    [Header("死亡処理設定")]
    [Tooltip("死亡アニメーション再生後に待機する秒数（アニメーションがない場合は無視）")]
    [SerializeField] private float waitAfterDeathAnimation = 1.5f;

    /// <summary>
    /// 死亡状態に入ったときの処理。
    /// ・死亡アニメーションを再生し、終了を待ってからプールへ返却。
    /// ・アニメーションがない場合は即座にプール返却。
    /// </summary>
    public override void Enter(EnemyController owner)
    {
        base.Enter(owner);

        if (owner.GetAnimationController() && owner.StateMachineSO.usesDead)
        {
            // 死亡アニメーションを再生
            owner.GetAnimationController().PlayDeadAnimation();

            // 再生後、一定時間待ってからプール返却
            owner.StartCoroutine(WaitAndHandleDead(owner));
        }
        else
        {
            // アニメーションなし → 即返却
            owner.HandleDead();
        }
    }

    /// <summary>
    /// 死亡アニメーション終了後にプール返却するコルーチン。
    /// </summary>
    private System.Collections.IEnumerator WaitAndHandleDead(EnemyController owner)
    {
        Animator animator = owner.GetAnimationController()?.GetComponent<Animator>();

        if (animator != null)
        {
            // 現在再生中アニメーションの情報を取得
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            yield return new WaitForSeconds(waitAfterDeathAnimation);
        }
        else
        {
            // Animatorがない場合は1フレームだけ待機
            yield return null;
        }

        // 待機後にプールへ返却
        owner.HandleDead();
    }

    public override void Tick(EnemyController owner, float deltaTime) { }

    public override void Exit(EnemyController owner) { }
}
