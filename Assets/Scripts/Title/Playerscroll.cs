using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]

public class Plyerscrool : MonoBehaviour
{
    public float runSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    #region SE管理

    // --------------------------------------------------------------
    // 【SE管理】
    // 着地時に再生
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
        // 常に右に走る
        rb.linearVelocity = new Vector2(runSpeed, rb.linearVelocity.y);

        // アニメーターに速度を渡す
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("scroll", horizontalSpeed);

        // スプライト反転（元の絵が左向きだから右に進むときはflipX = true）
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