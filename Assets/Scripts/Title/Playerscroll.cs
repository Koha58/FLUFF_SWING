using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class Plyerscrool : MonoBehaviour
{
    public float runSpeed = 5f;

    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // �O�����Ɏ����ړ�
        Vector3 move = transform.right * runSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, 0f);

        // �������̑������A�j���[�^�[�ɔ��f
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        animator.SetFloat("scroll", horizontalSpeed);
    }
}