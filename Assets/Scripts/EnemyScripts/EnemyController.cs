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

    /// <summary>SpriteRenderer</summary>
    [SerializeField] private SpriteRenderer spriteRenderer;

    /// <summary>
    /// ���S����SE
    /// </summary>
    [SerializeField] private AudioClip deathSE;

    /// <summary>
    /// ���C���[�J�b�g����SE
    /// </summary>
    [SerializeField] private AudioClip cutSE;

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

    private WireActionScript wireToCut; // �� Wire���ꎞ�ێ�����

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

    /// <summary>
    /// ������Ԃ��O������A�N�Z�X������
    /// </summary>
    public int Direction { get; set; } = -1; // �������ŊJ�n

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

        // SpriteRenderer���Ȃ��ꍇ�͍쐬����
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
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
            // ���SSE���Đ�
            AudioManager.Instance?.PlaySE(deathSE);
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

    /// <summary>
    /// Player�Ƃ̐ڐG����
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Patrol�ŁA�����蒆�͖�����
            if (Type == EnemyType.Patrol && !IsMovementDisabledByAnimation)
            {
                Debug.Log("Patrol enemy is disabled by animation. No damage dealt.");
                return; // �X�L�b�v
            }

            // ����ȊO�̓_���[�W��^����
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(status.attack);
                Debug.Log($"Enemy attacked Player for {status.attack} damage");
            }
        }

        if (collision.CompareTag("Wire"))
        {
            // Bird�Ƀ��C���[���ڐG�����Ƃ�
            if (Type == EnemyType.Bird)
            {
                // ���C���[�� Player ���擾���� CutWire ���Ă�
                var player = collision.GetComponentInParent<WireActionScript>();

                if (player != null)
                {
                    wireToCut = player;  // �� Wire���o���Ă����I

                    // �U���X�e�[�g�ɑJ��
                    stateMachine.ChangeState(stateMachineSO.cutState);
                }
            }
        }

        // "Flip"�^�O�ɐG�ꂽ�甽�]
        if (collision.CompareTag("Flip"))
        {
            ReverseDirection();
        }
    }

    /// <summary>
    /// ������ Sprite �𔽓]����
    /// </summary>
    public void ReverseDirection()
    {
        Direction *= -1;
        UpdateSpriteFlip();
        Debug.Log($"[EnemyController] �������]: {Direction}");
    }

    /// <summary>
    /// SpriteRenderer �� FlipX �� Direction �ɍ��킹�Đݒ�
    /// </summary>
    private void UpdateSpriteFlip()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (Direction == 1);
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
        // �U�����͈ړ�����
        DisableMovementByAnimation();

        // �U���X�e�[�g�ɑJ��
        stateMachine.ChangeState(stateMachineSO.attackState);
    }

    /// <summary>
    /// �U���A�j���[�V�����I�����ɌĂ΂��
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        EnableMovementByAnimation();  // �ړ�����������
        SwitchToMove();               // �X�e�[�g���ړ��ɖ߂�
                                      // Wire������ΐ؂�
        if (wireToCut != null)
        {
            wireToCut.CutWire();
            wireToCut = null;  // �Y�ꂸ�N���A�I
        }
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

    /// <summary>
    /// ���C���[�J�b�gSE�Đ�
    /// </summary>
    public void PlayCutSE()
    {
        AudioManager.Instance?.PlaySE(cutSE);
    }
}
