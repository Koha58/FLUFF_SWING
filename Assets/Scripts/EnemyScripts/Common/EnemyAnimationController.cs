using UnityEngine;

/// <summary>
/// 敵キャラクターのアニメーション制御クラス
/// Animatorのパラメータをハッシュ化して管理し、移動・攻撃・死亡のアニメーションを再生・停止する
/// </summary>
public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;   // Animatorコンポーネント参照

    private int moveParamHash;    // 移動アニメーション用パラメータのハッシュ値
    private int attackParamHash;  // 攻撃アニメーション用パラメータのハッシュ値
    private int deadParamHash;    // 死亡アニメーション用パラメータのハッシュ値

    [SerializeField] private bool usesMove = false;    // 移動アニメーションを使用するか
    [SerializeField] private bool usesAttack = false;  // 攻撃アニメーションを使用するか
    [SerializeField] private bool usesDead = false;    // 死亡アニメーションを使用するか

    private bool canUseMove = false; // Attack が呼ばれたら true にする

    private void Awake()
    {
        // 使用するアニメーションに対応したパラメータ名をハッシュ化（高速アクセス用）
        if (usesMove)
            moveParamHash = Animator.StringToHash("IsMove");
        if (usesAttack)
            attackParamHash = Animator.StringToHash("IsAttack");
        if (usesDead)
            deadParamHash = Animator.StringToHash("IsDead");
    }

    /// <summary>
    /// 全てのアニメーション状態をリセットする
    /// </summary>
    private void ResetAnimationStates()
    {
        if (usesMove && moveParamHash != 0)
            animator.SetBool(moveParamHash, false);
        if (usesAttack && attackParamHash != 0)
            animator.SetBool(attackParamHash, false);
        // DeadはTriggerなので無効化しない
    }

    /// <summary>
    /// 移動アニメーションを開始する
    /// </summary>
    public void PlayMoveAnimation()
    {
        // Attack呼ばれるまで無効
        if (!usesMove || !canUseMove) return;

        ResetAnimationStates();

        if (usesMove)
            animator.SetBool(moveParamHash, true);
    }

    /// <summary>
    /// 移動アニメーションを停止する
    /// </summary>
    public void StopMoveAnimation()
    {
        if (usesMove)
            animator.SetBool(moveParamHash, false);
    }

    /// <summary>
    /// 攻撃アニメーションを開始する
    /// </summary>
    public void PlayAttackAnimation()
    {
        if (!usesAttack) return;

        ResetAnimationStates();

        if (usesAttack)
            animator.SetBool(attackParamHash, true);

        // Attackを呼んだので以降Moveを有効にする
        canUseMove = true;
    }

    /// <summary>
    /// 攻撃アニメーションを停止する
    /// </summary>
    public void StopAttackAnimation()
    {
        if (usesAttack)
            animator.SetBool(attackParamHash, false);
    }

    /// <summary>
    /// 死亡アニメーションを再生する（Triggerをセット）
    /// </summary>
    public void PlayDeadAnimation()
    {
        ResetAnimationStates();

        if (usesDead)
            animator.SetTrigger(deadParamHash);
    }
}