using UnityEditor.U2D.Animation;
using UnityEngine;

/// <summary>
/// �G�L�����N�^�[�̐���N���X
/// �E�X�e�[�^�X�Ǘ�
/// �E��ԑJ��(StateMachine)
/// �E�_���[�W����
/// �E�v�[���ԋp
/// </summary>
public class EnemyController : MonoBehaviour, IDamageable
{
    #region === SerializeFields ===

    /// <summary>�G�̎��</summary>
    [SerializeField] private EnemyType enemyType;

    /// <summary>�X�e�[�^�X�Q�ƌ��f�[�^</summary>
    [SerializeField] private CharacterBase characterData;

    /// <summary>�X�e�[�^�X���iHP�Ȃǁj</summary>
    [SerializeField] private CharacterStatus status;

    /// <summary>�A�j���[�V��������R���|�[�l���g</summary>
    [SerializeField] private EnemyAnimationController animationController;

    /// <summary>�X�e�[�g�}�V���ݒ�f�[�^</summary>
    [SerializeField] private EnemyStateMachineSO stateMachineSO;

    /// <summary>���S�A�j���[�V�����̗L��</summary>
    [SerializeField] private bool hasDeadAnimation = true;

    #endregion

    #region === Private Fields ===

    /// <summary>�G��p�̃X�e�[�g�}�V��</summary>
    private StateMachine<EnemyController> stateMachine;

    /// <summary>���݂�HP</summary>
    private int currentHP;

    /// <summary>�L�����N�^�[�̈ړ����x</summary>
    private float moveSpeed;

    /// <summary>�p�g���[���J�n�ʒuX���W</summary>
    private float patrolStartX;

    /// <summary>�p�g���[�������i1:�E, -1:���j</summary>
    private int patrolDirection = -1;

    #endregion

    #region === Properties ===

    /// <summary>�G�̎�ނ��O���Ɍ��J</summary>
    public EnemyType Type => enemyType;

    /// <summary>�X�e�[�g�}�V���ݒ�SO���O���Ɍ��J</summary>
    public EnemyStateMachineSO StateMachineSO => stateMachineSO;

    /// <summary>���S�A�j���[�V�����̗L��</summary>
    public bool HasDeadAnimation => hasDeadAnimation;

    /// <summary>�ړ����x</summary>
    public float MoveSpeed => moveSpeed;

    /// <summary>�p�g���[���J�n�ʒuX</summary>
    public float PatrolStartX
    {
        get => patrolStartX;
        set => patrolStartX = value;
    }

    /// <summary>�p�g���[������</summary>
    public int PatrolDirection
    {
        get => patrolDirection;
        set => patrolDirection = value;
    }

    /// <summary>�A�j���[�V�������̈ړ�������</summary>
    public bool IsMovementDisabledByAnimation { get; private set; }

    #endregion

    #region === Unity Callbacks ===

    /// <summary>
    /// �X�e�[�g�}�V�������� & �X�e�[�^�X�ݒ�
    /// </summary>
    private void Awake()
    {
        // �X�e�[�g�}�V�������g��Ώۂɏ�����
        stateMachine = new StateMachine<EnemyController>(this);

        // �L�����N�^�[�f�[�^����ړ����x���擾
        moveSpeed = characterData.moveSpeed;
    }

    /// <summary>
    /// �L��������HP������ & �J�n�X�e�[�g��ݒ�
    /// </summary>
    private void OnEnable()
    {
        // HP���ő�l�Ƀ��Z�b�g
        currentHP = status.maxHP;

        // �X�e�[�g�}�V���ݒ肪�Ȃ��ꍇ�̓G���[
        if (stateMachineSO == null)
        {
            Debug.LogError("StateMachineSO���ݒ肳��Ă��܂���I");
            return;
        }

        // �ړ��X�e�[�g���g���ꍇ�͈ړ��X�e�[�g�ŊJ�n
        if (stateMachineSO.usesMove && stateMachineSO.moveState != null)
        {
            stateMachine.ChangeState(stateMachineSO.moveState);
        }
        // �����łȂ���΍U���X�e�[�g�ŊJ�n
        else if (stateMachineSO.attackState != null)
        {
            stateMachine.ChangeState(stateMachineSO.attackState);
        }
        // �J�n�X�e�[�g��������΃G���[
        else
        {
            Debug.LogError("�J�n�X�e�[�g���ݒ肳��Ă��܂���I");
        }
    }

    /// <summary>
    /// �X�e�[�g�}�V���X�V
    /// </summary>
    private void Update()
    {
        // �X�e�[�g�}�V���̖��t���[���X�V���Ăяo��
        stateMachine.Update(Time.deltaTime);
    }

    #endregion

    #region === Enemy Logic ===

    /// <summary>
    /// �_���[�W���󂯂�
    /// Patrol�^�C�v�̓A�j���[�V�������͖��G
    /// </summary>
    public void TakeDamage(int damage)
    {
        Debug.Log($"TakeDamage called. Type={Type}, IsMovementDisabledByAnimation={IsMovementDisabledByAnimation}");

        // Patrol�^�C�v�ŁA����A�j���[�V�������łȂ��ꍇ�̓_���[�W����
        if (Type == EnemyType.Patrol && !IsMovementDisabledByAnimation)
            return;

        // HP������
        currentHP -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {currentHP}");

        // HP��0�ȉ��ɂȂ����玀�S�X�e�[�g�֑J��
        if (currentHP <= 0)
        {
            stateMachine.ChangeState(stateMachineSO.deadState);
        }
    }

    /// <summary>
    /// �U���\�Ȃ�U�������s�i�������j
    /// </summary>
    public void AttackIfPossible()
    {
        // TODO: �U�����W�b�N�����\��
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player�^�O�ƐڐG������
        if (collision.CompareTag("Player"))
        {
            // IDamageable ���擾���ă_���[�W��^����
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(status.attack); // Enemy �̍U���͂�n��
                Debug.Log($"Enemy attacked Player for {status.attack} damage");
            }
        }
    }


    /// <summary>
    /// ���S��A�v�[���֕ԋp
    /// </summary>
    public void HandleDead()
    {
        // EnemyPool�ɕԋp���Ĕ�A�N�e�B�u��
        EnemyPool.Instance.ReturnToPool(this);
    }

    #endregion

    #region === Animation Events ===

    /// <summary>
    /// �A�j���[�V�����C�x���g�F�ړ�������ON
    /// </summary>
    public void DisableMovementByAnimation()
    {
        // �ړ������t���O��ON
        IsMovementDisabledByAnimation = true;
    }

    /// <summary>
    /// �A�j���[�V�����C�x���g�F�ړ�������OFF
    /// </summary>
    public void EnableMovementByAnimation()
    {
        // �ړ������t���O��OFF
        IsMovementDisabledByAnimation = false;
    }

    #endregion

    #region === State Control ===

    /// <summary>
    /// �A�j���[�V����������擾
    /// </summary>
    public EnemyAnimationController GetAnimationController()
    {
        return animationController;
    }

    /// <summary>
    /// �U���X�e�[�g�ɐ؂�ւ�
    /// </summary>
    public void SwitchToAttack()
    {
        // �U���X�e�[�g�ɑJ��
        stateMachine.ChangeState(stateMachineSO.attackState);
    }

    /// <summary>
    /// �ړ��X�e�[�g�ɐ؂�ւ�
    /// </summary>
    public void SwitchToMove()
    {
        // �ړ��X�e�[�g�ɑJ��
        stateMachine.ChangeState(stateMachineSO.moveState);
    }

    #endregion
}
