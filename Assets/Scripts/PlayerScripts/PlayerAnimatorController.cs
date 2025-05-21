using UnityEngine;

/// <summary>
/// �v���C���[�̃A�j���[�V�����𐧌䂷��N���X�B
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;

    // �ꎞ�I�Ɉړ��t���O��ێ��i���炩�Ȑ؂�ւ��p�j
    private bool isMoving = false;
    private float moveThreshold = 0.05f;
    private float moveDelayTime = 0.1f; // 0.1�b�ȓ��̒�~�͖�������
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
    /// �ړ��A�j���[�V�����̍X�V�i���E�ړ��j
    /// </summary>
    public void UpdateMoveAnimation(float moveInput)
    {
        if (Mathf.Abs(moveInput) > moveThreshold)
        {
            // �����Ă���̂ő����f
            isMoving = true;
            moveStopTimer = 0f;
        }
        else
        {
            // ��~���n�߂��Ƃ��̒x������
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
    /// ���C���[�A�N�V����
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
    /// �v���C���[�̌������ړ������ɍ��킹�č��E���]
    /// X+�������������̃X�v���C�g���l��
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
