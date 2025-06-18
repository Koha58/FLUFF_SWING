using UnityEngine;

/// <summary>
/// �G�����e�𓊂����p�N���X
/// - Animator�̃C�x���g����Ă΂��
/// </summary>
public class EnemyThrowAttack : MonoBehaviour
{
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private float throwForce = 7f;

    private Transform player;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
    }

    /// <summary>
    /// �A�j���[�V�����C�x���g����Ă΂�Ĕ��e�𐶐��E������
    /// </summary>
    public void Throw()
    {
        if (bombPrefab == null || player == null) return;

        GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Bomb bombScript = bomb.GetComponent<Bomb>();

        if (bombScript != null)
        {
            // �G����v���C���[�����֔�΂�
            Vector2 dir = (player.position - transform.position).normalized;
            bombScript.Launch(dir.x, throwForce, 20); // 20 = �_���[�W��
        }
    }
}
