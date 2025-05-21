using UnityEngine;

/// <summary>
/// プレイヤーのアニメーションを制御するクラス。
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;

    // 一時的に移動フラグを保持（滑らかな切り替え用）
    private bool isMoving = false;
    private float moveThreshold = 0.05f;
    private float moveDelayTime = 0.1f; // 0.1秒以内の停止は無視する
    private float moveStopTimer = 0f;

    private bool isSwinging = false;
    private bool justGrappled = false;
    private float grappleTransitionTime = 0.3f;
    private float grappleTimer = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (justGrappled)
        {
            grappleTimer -= Time.deltaTime;
            if (grappleTimer <= 0f)
            {
                justGrappled = false;
                animator.SetBool("justGrappled", false);
            }
        }
    }

    /// <summary>
    /// 移動アニメーションの更新（左右移動）
    /// </summary>
    public void UpdateMoveAnimation(float moveInput)
    {
        if (Mathf.Abs(moveInput) > moveThreshold)
        {
            // 動いているので即反映
            isMoving = true;
            moveStopTimer = 0f;
        }
        else
        {
            // 停止し始めたときの遅延処理
            moveStopTimer += Time.deltaTime;
            if (moveStopTimer > moveDelayTime)
            {
                isMoving = false;
            }
        }

        animator.SetBool("isRunning", isMoving);
        FlipSprite(moveInput);
    }

    /// <summary>
    /// ワイヤーアクション
    /// </summary>
    public void PlayGrappleSwingAnimation()
    {
        justGrappled = true;
        isSwinging = true;
        grappleTimer = grappleTransitionTime;

        animator.SetBool("isJumping", true);
        animator.SetBool("isSwinging", true);
    }

    public void StopSwingAnimation()
    {
        isSwinging = false;
        animator.SetBool("isSwinging", false);
        animator.SetBool("isStaying", true);
    }

    /// <summary>
    /// プレイヤーの向きを移動方向に合わせて左右反転
    /// X+方向が左向きのスプライトを考慮
    /// </summary>
    private void FlipSprite(float moveInput)
    {
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Sign(moveInput) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}
