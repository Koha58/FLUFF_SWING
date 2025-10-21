using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]

public class Plyerscrool : MonoBehaviour
{
    public float runSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    #region SE�Ǘ�

    // --------------------------------------------------------------
    // �ySE�Ǘ��z
    // ���n���ɍĐ�
    // --------------------------------------------------------------
    [SerializeField] private AudioClip[] footstepSEs;
    private int lastFootstepIndex = -1;

    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    void FixedUpdate()
    {
        // ��ɉE�ɑ���
        rb.linearVelocity = new Vector2(runSpeed, rb.linearVelocity.y);

        // �A�j���[�^�[�ɑ��x��n��
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("scroll", horizontalSpeed);

        // �X�v���C�g���]�i���̊G��������������E�ɐi�ނƂ���flipX = true�j
        spriteRenderer.flipX = rb.linearVelocity.x < 0;

    }

    public void PlayFootstepSE()
    {
        if (footstepSEs == null || footstepSEs.Length == 0) return;

        int index;
        do
        {
            index = Random.Range(0, footstepSEs.Length);
        } while (index == lastFootstepIndex && footstepSEs.Length > 1);

        lastFootstepIndex = index;
        AudioManager.Instance?.PlaySE(footstepSEs[index]);
    }
}