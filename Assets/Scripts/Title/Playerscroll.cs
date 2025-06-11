using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]

public class Plyerscrool : MonoBehaviour
{
    public float runSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

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
        spriteRenderer.flipX = rb.linearVelocity.x > 0;

    }
}