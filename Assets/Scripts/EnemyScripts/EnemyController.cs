using UnityEditor.U2D.Animation;
using UnityEngine;

/// <summary>
/// �G�L�����N�^�[�̐���N���X
/// �G�̏�ԊǗ�(StateMachine)��HP�Ǘ��A�_���[�W�����Ȃǂ��s��
/// </summary>
public class EnemyController : MonoBehaviour, IDamageable
{
    // �X�e�[�^�X�Ǘ��X�N���v�g
    [SerializeField] private CharacterBase characterData;
    [SerializeField] private CharacterStatus status;                         // �G�̃X�e�[�^�X���iHP�Ȃǁj
    [SerializeField] private EnemyAnimationController animationController;   // �A�j���[�V��������p�R���|�[�l���g
    [SerializeField] private EnemyStateMachineSO stateMachineSO;             // �G�̏�ԊǗ��f�[�^(ScriptableObject)
    public EnemyStateMachineSO StateMachineSO => stateMachineSO;
    [SerializeField] private bool hasDeadAnimation = true;                   // ���S�A�j���[�V�����̗L���t���O

    private StateMachine<EnemyController> stateMachine;                      // �G��p�̃X�e�[�g�}�V��
    private int currentHP;                                                    // ���݂�HP
    private float moveSpeed;                                                 // �n��ł̍��E�ړ��X�s�[�h
    public float MoveSpeed => moveSpeed; // �� �ǉ�

    private float patrolStartX;
    private int patrolDirection = -1;

    public float PatrolStartX { get => patrolStartX; set => patrolStartX = value; }
    public int PatrolDirection { get => patrolDirection; set => patrolDirection = value; }

    public bool HasDeadAnimation => hasDeadAnimation;

    /// <summary>
    /// �A�j���[�V�����C�x���g���͈ړ��𖳌���
    /// </summary>
    public bool IsMovementDisabledByAnimation { get; private set; }

    private void Awake()
    {
        // �X�e�[�g�}�V�����������i���g��ΏۂɁj
        stateMachine = new StateMachine<EnemyController>(this);

        // characterData ���� moveSpeed���擾
        moveSpeed = characterData.moveSpeed;
    }

    private void OnEnable()
    {
        // HP���ő�Ƀ��Z�b�g
        currentHP = status.maxHP;

        // �X�e�[�g�}�V��SO���ݒ肳��Ă��邩�`�F�b�N
        if (stateMachineSO == null)
        {
            Debug.LogError("stateMachineSO���Z�b�g����Ă��܂���I");
            return;
        }

        // �G�̎�ނɂ���ĊJ�n�X�e�[�g��؂�ւ���
        // Bird�̂悤�Ɉړ�����J�n����ꍇ��moveState����J�n
        if (stateMachineSO.usesMove && stateMachineSO.moveState != null)
        {
            stateMachine.ChangeState(stateMachineSO.moveState);
        }
        // ����ȊO�͍U����Ԃ���J�n
        else if (stateMachineSO.attackState != null)
        {
            stateMachine.ChangeState(stateMachineSO.attackState);
        }
        else
        {
            Debug.LogError("�J�nState���Z�b�g����Ă��܂���I");
        }
    }

    private void Update()
    {
        // ���t���[���X�e�[�g�}�V����Update���Ăяo���i��ԑJ�ڂ⏈�������s�j
        stateMachine.Update(Time.deltaTime);
    }

    /// <summary>
    /// �_���[�W���󂯂鏈��
    /// HP�����炵�A0�ȉ��Ȃ玀�S�X�e�[�g�ɑJ�ڂ���
    /// </summary>
    /// <param name="damage">�󂯂�_���[�W��</param>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {currentHP}");

        if (currentHP <= 0)
        {
            // HP0�ȉ��Ŏ��S�X�e�[�g�֑J��
            stateMachine.ChangeState(stateMachineSO.deadState);
        }
    }

    // �ȉ��͓G�̍s�����W�b�N�p�̃_�~�[���\�b�h
    public void AttackIfPossible() { /* �U�����W�b�N */ }

    /// <summary>
    /// ���S����������ɌĂ΂�郁�\�b�h
    /// �v�[���ɓG�I�u�W�F�N�g��ԋp����
    /// </summary>
    public void HandleDead()
    {
        EnemyPool.Instance.ReturnToPool(this);
    }

    /// <summary>
    /// AnimationEvent ����Ăяo��
    /// </summary>
    public void DisableMovementByAnimation()
    {
        IsMovementDisabledByAnimation = true;
    }

    public void EnableMovementByAnimation()
    {
        IsMovementDisabledByAnimation = false;
    }

    /// <summary>
    /// �A�j���[�V��������R���|�[�l���g���擾����
    /// </summary>
    /// <returns>EnemyAnimationController</returns>
    public EnemyAnimationController GetAnimationController() => animationController;

    /// <summary>
    /// �U����Ԃɐ؂�ւ���
    /// </summary>
    public void SwitchToAttack() => stateMachine.ChangeState(stateMachineSO.attackState);

    /// <summary>
    /// �ړ���Ԃɐ؂�ւ���
    /// </summary>
    public void SwitchToMove() => stateMachine.ChangeState(stateMachineSO.moveState);
}