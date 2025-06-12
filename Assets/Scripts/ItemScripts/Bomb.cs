using UnityEngine;

/// <summary>
/// �������锚�e�̋������Ǘ�����N���X�B
/// - ������ꂽ��A�������Z�Ŕ��
/// - �n�`��G�ɓ�����Ɣ�������
/// - �����G�t�F�N�g���Đ����A���g��j�󂷂�
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bomb : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("�����G�t�F�N�g")]
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("�����ݒ�")]
    private float explosionRadius = 2f;    // �����͈�
    private int explosionDamage;       // �����_���[�W
    [SerializeField] private LayerMask damageableLayers;     // �_���[�W����Ώۃ��C���[

    private bool hasExploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(float direction, float force, int damage)
    {
        rb.linearVelocity = new Vector2(force * direction, force * 0.5f);
        explosionDamage = damage;
    }

    /// <summary>
    /// �Փ˔���i�n�`�E�G�Ȃǂɓ��������甚���j
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;  // ���ɔ����ς݂Ȃ疳��

        Explode();
    }

    /// <summary>
    /// ��������
    /// - �G�t�F�N�g����
    /// - �͈͓��̃_���[�W����
    /// - ���e�I�u�W�F�N�g�j��
    /// </summary>
    private void Explode()
    {
        hasExploded = true;

        // �����G�t�F�N�g�̐���
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // �͈͓��̃_���[�W����
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(explosionDamage);
            }
        }

        // ���g�̍폜
        Destroy(gameObject);
    }

    /// <summary>
    /// �G�f�B�^��Ŕ����͈͂������iGizmo�\���j
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
