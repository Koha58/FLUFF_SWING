using UnityEngine;

/// <summary>
/// �v���C���[�̍U����������є�_���[�W�������Ǘ�����N���X�B
/// - �ł��߂��G�������Ŕ��肵�A�ߋ��������������ōU�����@��؂�ւ���B
/// - IDamageable�C���^�[�t�F�[�X���������A�U�����󂯂鑤�̏������s���B
/// </summary>
public class PlayerAttack : MonoBehaviour, IDamageable
{
    /// <summary>�L�����N�^�[�̃X�e�[�^�X���i�U���͂�HP�A�U���͈͂Ȃǁj</summary>
    [SerializeField] private CharacterStatus status;

    // �A�j���[�V��������X�N���v�g
    [SerializeField] private PlayerAnimatorController animatorController;

    [SerializeField] private GameObject bombPrefab;   // ���e��Prefab�iInspector�ŃZ�b�g�j
    private float throwForce = 10f;  // ���e�̓������

    /// <summary>���݂�HP</summary>
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

        if (target != null)
        {
            Transform targetTransform = ((MonoBehaviour)target).transform;
            float distance = Vector2.Distance(transform.position, targetTransform.position);

            if (status.meleeRange > 0f && distance <= status.meleeRange)
            {
                Debug.Log("Executing MeleeAttack()");
                MeleeAttack(target);
                return;
            }
            else if (status.attackRadius > 0f && distance <= status.attackRadius)
            {
                Debug.Log("Executing RangedAttack()");
                RangedAttack(target);
                return;
            }

            Debug.Log("Target is out of attack range.");
        }

        // �G�����Ȃ��A�܂��͔͈͊O�������ꍇ�ł���U��ߐڍU��
        Debug.Log("No valid target. Executing empty MeleeAttack.");
        MeleeAttack(null);
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
            if (dist < minDist && dist <= status.attackRadius)
            {
                var damageable = enemy.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    nearest = damageable;
                    minDist = dist;
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// �ߋ����U�������s����
    /// </summary>
    private void MeleeAttack(IDamageable target)
    {
        // �^�[�Q�b�g������Ε������v�Z�A���Ȃ���ΉE�����ŉ���i�܂��͌��݂̌����j
        float direction = 1f;
        if (target != null)
        {
            Vector2 targetDir = ((MonoBehaviour)target).transform.position - transform.position;
            direction = Mathf.Sign(targetDir.x);
        }

        // �A�j���[�V�����Đ�
        animatorController?.PlayMeleeAttackAnimation(direction);

        // �U����������Ώۂ�����ꍇ�̂݃_���[�W����
        if (target != null)
        {
            target.TakeDamage(status.attack);
            Debug.Log("Performed melee attack on target.");
        }
        else
        {
            Debug.Log("Performed empty melee attack.");
        }
    }

    /// <summary>
    /// �������U�������s����
    /// </summary>
    private void RangedAttack(IDamageable target)
    {
        // �^�[�Q�b�g������Ε������v�Z�A���Ȃ���ΉE�����ŉ���i�܂��͌��݂̌����j
        float direction = 1f;
        if (target != null)
        {
            Vector2 targetDir = ((MonoBehaviour)target).transform.position - transform.position;
            direction = Mathf.Sign(targetDir.x);
        }

        // �A�j���[�V�����Đ�
        animatorController?.PlayRangedAttackAnimation(direction);
    }

    public void ThrowBomb(float direction)
    {
        if (bombPrefab == null) return;

        GameObject bombObject = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Bomb bomb = bombObject.GetComponent<Bomb>();

        if (bomb != null)
        {
            bomb.Launch(direction, throwForce, status.attack);  // �U���͂��n��
        }
    }

    /// <summary>
    /// �_���[�W���󂯂鏈���iIDamageable�C���^�[�t�F�[�X�����j
    /// </summary>
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
        // �Q�[���I�[�o�[�����Ȃ�
    }
}
