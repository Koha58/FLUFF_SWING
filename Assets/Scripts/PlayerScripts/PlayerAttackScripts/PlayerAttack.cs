using UnityEngine;

/// <summary>
/// �v���C���[�̍U����������є�_���[�W�������Ǘ�����N���X�B
/// - �ł��߂��G�������Ŕ��肵�A�ߋ��������������ōU�����@��؂�ւ���B
/// - IDamageable�C���^�[�t�F�[�X���������A�U�����󂯂鑤�̏������s���B
/// </summary>
public class PlayerAttack : MonoBehaviour, IDamageable
{
    // �L�����N�^�[�̃X�e�[�^�X���i�U���͂�ő�HP�Ȃǁj
    [SerializeField] private CharacterStatus status;

    // �ߋ����U�����\�Ȕ͈́i�P�ʁF���[���h�P�ʁj
    [SerializeField] private float meleeRange = 1.5f;

    // �U���\�ȍő�͈́i���͈͓̔��̓G���^�[�Q�b�g�ɂ���j
    [SerializeField] private float attackRadius = 5f;

    // ���݂�HP
    private int currentHP;

    private void Start()
    {
        // ����HP���X�e�[�^�X�̍ő�HP�ŏ�����
        currentHP = status.maxHP;
    }

    /// <summary>
    /// �ł��߂��G��T���A�����ŋߋ������������U����I�����Ď��s����
    /// </summary>
    public void PerformAutoAttack()
    {
        var target = FindNearestEnemy();
        if (target == null) return;

        // �G��Transform���擾
        Transform targetTransform = ((MonoBehaviour)target).transform;
        float distance = Vector2.Distance(transform.position, targetTransform.position);

        // �����ɂ���čU�����@��؂�ւ���
        if (distance <= meleeRange)
        {
            MeleeAttack(target);
        }
        else
        {
            RangedAttack(target);
        }
    }

    /// <summary>
    /// �U���\�͈͓��ōł��߂��G���������ĕԂ�
    /// </summary>
    /// <returns>�ł��߂��G��IDamageable�R���|�[�l���g�A�G�����Ȃ����null</returns>
    private IDamageable FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        IDamageable nearest = null;
        float minDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist && dist <= attackRadius)
            {
                nearest = enemy.GetComponent<IDamageable>();
                minDist = dist;
            }
        }

        return nearest;
    }

    /// <summary>
    /// �ߋ����U�������s����
    /// </summary>
    /// <param name="target">�U���Ώ�</param>
    private void MeleeAttack(IDamageable target)
    {
        target.TakeDamage(status.attack);
        Debug.Log("Performed melee attack.");
    }

    /// <summary>
    /// �������U�������s����
    /// </summary>
    /// <param name="target">�U���Ώ�</param>
    private void RangedAttack(IDamageable target)
    {
        target.TakeDamage(status.attack);
        Debug.Log("Performed ranged attack.");
    }

    /// <summary>
    /// �_���[�W���󂯂鏈���iIDamageable�C���^�[�t�F�[�X�����j
    /// </summary>
    /// <param name="damage">�󂯂�_���[�W��</param>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"Player took {damage} damage. HP: {currentHP}");

        if (currentHP <= 0)
        {
            OnDead();
        }
    }

    /// <summary>
    /// HP��0�ȉ��ɂȂ������̏���
    /// </summary>
    private void OnDead()
    {
        Debug.Log("Player died.");
        // �Q�[���I�[�o�[�����Ȃǒǉ��\
    }
}
