using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// �v���C���[�̃��C���[����Ɋւ�����͂��Ǘ�����N���X�B
/// Input System��"ConnectWire"�i���N���b�N�j��"CutWire"�i�E�N���b�N�j�A�N�V�������Ď����A
/// �C�x���g��ʂ��đ��N���X�֒ʒm����B
/// </summary>
public class WireInputHandler : MonoBehaviour
{
    /// <summary>���C���[�ڑ��i���N���b�N�j�C�x���g</summary>
    public event Action OnLeftClick;

    /// <summary>���C���[�ؒf�i�E�N���b�N�j�C�x���g</summary>
    public event Action OnRightClick;

    // Input System�̃A�N�V�����Q�Ɓi���N���b�N�p�j
    private InputAction leftClickAction;

    // Input System�̃A�N�V�����Q�Ɓi�E�N���b�N�p�j
    private InputAction rightClickAction;

    /// <summary>
    /// �����������BInput System����A�N�V�������擾���A
    /// �R�[���o�b�N�o�^�ƗL�������s���B
    /// </summary>
    private void Awake()
    {
        // "ConnectWire"�A�N�V�����i���N���b�N�j��Input System����擾
        leftClickAction = InputSystem.actions.FindAction("ConnectWire");

        // "CutWire"�A�N�V�����i�E�N���b�N�j��Input System����擾
        rightClickAction = InputSystem.actions.FindAction("CutWire");

        // ���N���b�N�A�N�V�������擾�ł��Ă���΃C�x���g�o�^�ƗL����
        if (leftClickAction != null)
        {
            // �A�N�V���������s���ꂽ��OnLeftClick�C�x���g���Ăяo���inull�`�F�b�N�t���j
            leftClickAction.performed += ctx => OnLeftClick?.Invoke();

            // �A�N�V������L�������A���͎�t�J�n
            leftClickAction.Enable();
        }
        else
        {
            Debug.LogWarning("ConnectWire action not found in Input System.");
        }

        // �E�N���b�N�A�N�V�������擾�ł��Ă���΃C�x���g�o�^�ƗL����
        if (rightClickAction != null)
        {
            // �A�N�V���������s���ꂽ��OnRightClick�C�x���g���Ăяo���inull�`�F�b�N�t���j
            rightClickAction.performed += ctx => OnRightClick?.Invoke();

            // �A�N�V������L�������A���͎�t�J�n
            rightClickAction.Enable();
        }
        else
        {
            Debug.LogWarning("CutWire action not found in Input System.");
        }
    }
}
