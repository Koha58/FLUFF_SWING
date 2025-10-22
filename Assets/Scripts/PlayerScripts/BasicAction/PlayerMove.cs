using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/// <summary>
/// �v���C���[�̍��E�ړ�����ѐڒn������Ǘ�����N���X�B
/// - �n�ʂɂ���Ƃ��݈̂ړ��\
/// - ���C���[�ڑ����͈ړ��𖳌���
/// - �n�ʂƂ̐ڐG����� Raycast �ɂ����{
/// </summary>
[RequireComponent(typeof(Rigidbody2D))] // Rigidbody2D ���A�^�b�`����Ă��Ȃ��ꍇ�A�����Œǉ������
public class PlayerMove : MonoBehaviour
{
    #region === Inspector�ݒ�E�ˑ��R���|�[�l���g ===

    /// <summary>���C���[�A�N�V�����̏�Ԃ��Ǘ�����X�N���v�g</summary>
    [SerializeField] private WireActionScript wireActionScript;

    /// <summary>�A�j���[�V��������p�X�N���v�g</summary>
    [SerializeField] private PlayerAnimatorController animatorController;

    /// <summary>�v���C���[�X�e�[�^�X�f�[�^�i�ړ����x�Ȃǁj</summary>
    [SerializeField] private CharacterBase characterData;

    /// <summary>�n�ʔ���p��Transform�i�v���C���[�̑����j</summary>
    [SerializeField] private Transform groundCheck;

    /// <summary>�n�ʔ���Ŕ���ΏۂƂ��郌�C���[</summary>
    [SerializeField] private LayerMask groundLayer;

    #endregion


    #region === �����t�B�[���h ===

    /// <summary>�v���C���[�̕��������𐧌䂷�� Rigidbody2D</summary>
    private Rigidbody2D rb;

    /// <summary>�ړ����͒l�i-1�`1�j</summary>
    private float moveInput;

    /// <summary>�n��ł̈ړ����x�icharacterData����擾�j</summary>
    private float moveSpeed;

    /// <summary>�ڒn����p�̔��a�iOverlapCircle�Ȃǂ̔���͈́j</summary>
    private float groundCheckRadius = 0.5f;

    /// <summary>���݃v���C���[���n�ʂɐڒn���Ă��邩�̃t���O</summary>
    private bool isGrounded;

    /// <summary>�O�t���[���ł̐ڒn��ԁi��ԕω��̌��m�Ɏg�p�j</summary>
    private bool wasGrounded = false;

    /// <summary>�p�Ƀn�}�����ۂɎ����W�����v���邽�߂̏������</summary>
    private float jumpPower = 3.0f;

    /// <summary>���ݐڐG���Ă���n�ʂ̃J�X�^���^�C���i�ڒn���莞�ɍX�V�j</summary>
    private CustomTile currentGroundTile;

    /// <summary>�ړ����̓A�N�V�����iInput System��"Move"�j</summary>
    private InputAction moveAction;

    #endregion


    #region === Unity�C�x���g���\�b�h ===

    /// <summary>
    /// �R���|�[�l���g����������
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Input System����Move�A�N�V�������擾���ėL����
        moveAction = InputSystem.actions.FindAction("Move");
        moveAction?.Enable();

        // �X�e�[�^�X����ړ����x���擾
        moveSpeed = characterData.moveSpeed;
    }

    /// <summary>
    /// ���t���[���̓��͎擾�Ɛڒn����A�A�j���[�V��������
    /// </summary>
    private void Update()
    {
        // ���������̈ړ����͂��擾�i-1����1�j
        moveInput = moveAction?.ReadValue<Vector2>().x ?? 0f;

        // �ڒn��������{�iRaycast�ő������`�F�b�N�j
        isGrounded = CheckGrounded();

        // �ڒn��Ԃ��ω������烍�O���o���i�f�o�b�O�p�j
        if (isGrounded != wasGrounded)
        {
            Debug.Log("�ڒn��Ԃ��ω�: isGrounded = " + isGrounded);
            wasGrounded = isGrounded;
        }

        // ���C���[�ڑ����͈ړ��A�j���[�V������~�A�����łȂ���Γ��͂ɉ����čX�V
        if (wireActionScript.IsConnected)
        {
            animatorController?.ResetMoveAnimation();
        }
        else
        {
            animatorController?.UpdateMoveAnimation(moveInput);
        }
    }

    /// <summary>
    /// �������Z�X�V�i���Ԋu�ŌĂ΂��j
    /// �ړ�������W�����v�̕⏕�������Ŏ��s
    /// </summary>
    private void FixedUpdate()
    {
        // �_���[�W�A�j���Đ����܂��̓��C���[�ڑ����͈ړ��s�ɂ���
        if (animatorController.IsDamagePlaying || wireActionScript.IsConnected)
        {
            return;
        }

        if (isGrounded && !wireActionScript.IsConnected)
        {
            // linearVelocity���g���đ��x��ݒ�iY�����͕ێ��j
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        if (!isGrounded && Mathf.Abs(rb.linearVelocity.x) < 0.1f && moveInput != 0)
        {
            // �p�n�}��΍�̎����W�����v
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, jumpPower);
        }
    }


    #endregion


    #region === �ڒn����֘A���\�b�h ===

    /// <summary>
    /// Raycast���g���ăv���C���[�����̒n�ʂ𔻒肵�A�n�ʂɐڂ��Ă��邩��Ԃ��B
    /// �܂��ڐG���Ă���J�X�^���^�C�����X�V����B
    /// </summary>
    /// <returns>�n�ʂɐڒn���Ă����true�A�����łȂ����false</returns>
    private bool CheckGrounded()
    {
        Vector3 checkPos = groundCheck.position;

        // ��������Raycast���΂��Ēn�ʃ��C���[�ɓ����邩����
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, groundCheckRadius, groundLayer);

        if (hit.collider != null)
        {
            // Tilemap���擾���A���������|�C���g�̃^�C�������擾����
            Tilemap tilemap = hit.collider.GetComponent<Tilemap>();

            if (tilemap != null)
            {
                // ���[���h���W���^�C�����W�ɕϊ�
                Vector3Int cell = tilemap.WorldToCell(hit.point);

                // �^�C�����擾���ACustomTile���ǂ�������
                TileBase tile = tilemap.GetTile(cell);
                if (tile is CustomTile customTile)
                {
                    currentGroundTile = customTile;
                }
                else
                {
                    currentGroundTile = null;
                }

                return true;
            }
        }

        currentGroundTile = null;
        return false;
    }

    /// <summary>
    /// �n�ʂ܂ł̋�����Ԃ��iRaycast���ʂ̋����j
    /// </summary>
    public float DistanceToGround
    {
        get
        {
            Vector3 checkPos = groundCheck.position;
            RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, groundCheckRadius * 2f, groundLayer);
            return hit.collider != null ? hit.distance : Mathf.Infinity;
        }
    }

    /// <summary>
    /// �u�قڐڒn�v����B������臒l�ȓ��Ȃ�true
    /// </summary>
    public bool IsAlmostGrounded(float threshold = 0.08f)
    {
        // Grounded��true�Ȃ���true��Ԃ��i�t���[���x���΍�j
        if (isGrounded) return true;
        return DistanceToGround < threshold;
    }

    #endregion


    #region === �v���p�e�B�i�O���Q�Ɨp�j ===

    /// <summary>
    /// ���ݐڐG���Ă���^�C���̎�ށi�n�ʂ̎�ނ𔻕ʉ\�j
    /// null�̏ꍇ�͒n�ʂȂ�
    /// </summary>
    public CustomTile.TileType? CurrentGroundType => currentGroundTile?.tileType;

    /// <summary>
    /// ���ݐڐG���Ă���^�C���̃C���X�^���X
    /// </summary>
    public CustomTile CurrentGroundTile => currentGroundTile;

    /// <summary>
    /// �v���C���[�����ݒn�ʂɐڒn���Ă��邩�ǂ���
    /// </summary>
    public bool IsGrounded => isGrounded;

    #endregion
}
