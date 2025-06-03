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
    #region �ˑ��X�N���v�g�E�\���v�f

    // ���C���[�A�N�V�����̏�ԁi�ڑ���ԂȂǁj���Ǘ�����X�N���v�g
    [SerializeField] private WireActionScript wireActionScript;

    // �A�j���[�V��������X�N���v�g
    [SerializeField] private PlayerAnimatorController animatorController;

    // �X�e�[�^�X�Ǘ��X�N���v�g
    [SerializeField] private CharacterBase characterData;

    // �v���C���[�̕��������𐧌䂷�� Rigidbody2D
    private Rigidbody2D rb;

    // ���̓A�N�V�����i"Move"�j
    private InputAction moveAction;

    #endregion


    #region �ړ��֘A

    // �n��ł̍��E�ړ��X�s�[�h
    private float moveSpeed;

    // �v���C���[�̍��E���͒l�i-1 �` 1�j
    private float moveInput;

    #endregion


    #region �ڒn����֘A

    [Header("Ground Check Settings")]

    // �n�ʃ`�F�b�N�p�̊�_�i�v���C���[�����j
    [SerializeField] private Transform groundCheck;

    // �n�ʂƔ��肷�郌�C���[�iLayerMask�j
    [SerializeField] private LayerMask groundLayer;

    // �ڒn����Ɏg�p����~�̔��a�iOverlapCircle�p�j
    private float groundCheckRadius = 0.5f;

    // ���݃v���C���[���n�ʂɐڒn���Ă��邩�ǂ���
    private bool isGrounded;

    // �O�̃t���[���ł̐ڒn���
    private bool wasGrounded = false;

    // �p�Ƀn�}�����ۂɎ����W�����v���邽�߂̏������
    private float jumpPower = 3.0f;

    #endregion


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Input System ���� "Move" �A�N�V�������擾
        moveAction = InputSystem.actions.FindAction("Move");
        moveAction?.Enable(); // ���͎�t��L����

        // characterData ���� moveSpeed���擾
        moveSpeed = characterData.moveSpeed;
    }

    private void Update()
    {
        // ���͂̎擾�iA/D�L�[�⍶�X�e�B�b�N�ɂ�鐅�������̓��́j
        moveInput = moveAction?.ReadValue<Vector2>().x ?? 0f;

        // �ڒn��������{
        isGrounded = CheckGrounded();

        // �ڒn��Ԃ̕ω������O�o��
        if (isGrounded != wasGrounded)
        {
            Debug.Log("�ڒn��Ԃ��ω�: isGrounded = " + isGrounded);
            wasGrounded = isGrounded;
        }

        // ���C���[�ɐڑ����͈ړ��A�j���[�V������~
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
    /// �ڒn��Ԃ𔻒肷�邽�߂̃��\�b�h
    /// Raycast�ŉ������ɐ����΂��A�n�ʂƂ̏Փ˂��m�F����
    /// </summary>
    /// <returns>�n�ʂɐڂ��Ă��邩�ǂ���</returns>
    private bool CheckGrounded()
    {
        Vector3 checkPos = groundCheck.position;

        // ��������Raycast���΂��A��苗�����Œn�ʃ��C���[�ɓ����邩����
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, groundCheckRadius, groundLayer);

        // �������Ă��āATilemap�����݂���ꍇ�A�n�ʂƌ��Ȃ�
        return hit.collider != null && hit.collider.GetComponent<Tilemap>() != null;
    }

    /// <summary>
    /// �������Z�̏����iFixedUpdate�͈��Ԋu�ŌĂ΂��j
    /// </summary>
    private void FixedUpdate()
    {
        // ���C���[�ɐڑ����͈�؂̈ړ��𖳌��ɂ���
        if (wireActionScript.IsConnected)
        {
            return;
        }

        // �ڒn���Ă��āA�����C���[�ɐڑ�����Ă��Ȃ��Ƃ��݈̂ړ��\
        if (isGrounded && !wireActionScript.IsConnected)
        {
            // ���E�ړ��𔽉f�iY�����̑��x�͕ێ��j
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        if (!isGrounded && Mathf.Abs(rb.linearVelocity.x) < 0.1f && moveInput != 0)
        {
            // �p�Ƀn�}���Ă�悤�ȂƂ��Ɏ����ŏ��W�����v
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, jumpPower);
        }

    }
}
