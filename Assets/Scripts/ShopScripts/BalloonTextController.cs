using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// �L�����N�^�[�̓���ɐ����o���e�L�X�g��\������R���g���[���[�N���X
/// �v���C���[�Ƃ̋����ɉ����Đ����o���̕\���E��\����؂�ւ��A
/// �e�L�X�g�̓^�C�s���O����1�������\�����܂��B
/// </summary>
public class BalloonTextController : MonoBehaviour
{
    /// <summary>�����o�����̃e�L�X�g�\���R���|�[�l���g</summary>
    private TextMeshProUGUI dialogueText;

    /// <summary>�\������S���e�L�X�g</summary>
    private string fullText = "�{�^�������C�t�Ɍ�������H";

    /// <summary>1�����\���̊Ԋu�i�b�j</summary>
    private float typingSpeed = 0.05f;

    /// <summary>�v���C���[��Transform�i�ʒu�擾�p�j</summary>
    private Transform player;

    /// <summary>�����o���\���̍ő勗���i3���[�g���j</summary>
    private float showDistance = 3.0f;

    /// <summary>�����o���̕\���ʒu�I�t�Z�b�g�i���゠����j</summary>
    private Vector3 offset = new Vector3(0, 1.5f, 0);

    /// <summary>���C���J�����Q�Ɓi�����o���̉�]����p�j</summary>
    private Camera mainCamera;

    /// <summary>�����o��Canvas�R���|�[�l���g</summary>
    private Canvas balloonCanvas;

    /// <summary>�e�L�X�g�^�C�s���O������Coroutine�Ǘ�</summary>
    private Coroutine typingCoroutine;

    /// <summary>�����o�������ݕ\�������ǂ����̃t���O</summary>
    private bool isDisplayed = false;

    /// <summary>�e�L�X�g���S���\�����ꂽ���ǂ����̃t���O</summary>
    private bool isFullyDisplayed = false;

    /// <summary>�S���\�����ꂽ���ǂ����̌��J�v���p�e�B</summary>
    public bool IsFullyDisplayed => isFullyDisplayed;

    /// <summary>�e�L�X�g�S���\���������ɌĂ΂��C�x���g</summary>
    public event System.Action OnFullyDisplayed;


    void Start()
    {
        // Canvas�R���|�[�l���g���擾�i�q�I�u�W�F�N�g����j
        balloonCanvas = GetComponentInChildren<Canvas>();

        // TextMeshProUGUI�R���|�[�l���g���擾�i�q�I�u�W�F�N�g����j
        dialogueText = GetComponentInChildren<TextMeshProUGUI>();

        // Canvas�����݂��Ȃ��ꍇ�̓G���[���o���ď�����~
        if (balloonCanvas == null)
        {
            Debug.LogError("�����o��Canvas��������܂���B");
            enabled = false;
            return;
        }

        // TextMeshProUGUI�����݂��Ȃ��ꍇ�̓G���[���o���ď�����~
        if (dialogueText == null)
        {
            Debug.LogError("TextMeshProUGUI��������܂���B");
            enabled = false;
            return;
        }

        // �ŏ��͐����o�����\���ɂ���
        balloonCanvas.enabled = false;

        // ���C���J�����̎Q�Ƃ��擾
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // �^�O�uPlayer�v���t�����I�u�W�F�N�g��Transform���擾
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player�I�u�W�F�N�g��������܂���B�^�O�uPlayer�v��ݒ肵�Ă��������B");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        // �v���C���[�Ƃ��̃I�u�W�F�N�g�̋������v�Z
        float distance = Vector3.Distance(transform.position, player.position);

        // �������ł܂��\�����Ă��Ȃ���Ε\���J�n
        if (distance < showDistance && !isDisplayed)
        {
            ShowBalloon();
        }
        // �����O�ɂȂ�\�����Ȃ��\���ɂ���
        else if (distance >= showDistance && isDisplayed)
        {
            HideBalloon();
        }

        // �\�����͐����o���̈ʒu�𓪏�ɐݒ肵�A
        // �J�����̕����������悤�ɉ�]�𒲐�����
        if (isDisplayed)
        {
            balloonCanvas.transform.position = transform.position + offset;

            // �J���������ɏ�ɐ��ʂ������悤�ɉ�]
            balloonCanvas.transform.LookAt(
                balloonCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// �����o����\�����A�e�L�X�g�̃^�C�s���O���J�n����
    /// </summary>
    private void ShowBalloon()
    {
        isDisplayed = true;
        balloonCanvas.enabled = true;
        dialogueText.text = "";
        isFullyDisplayed = false;

        // �����O��̃^�C�s���O����������Β�~����
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // �V�����^�C�s���O�������J�n
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// �����o�����\���ɂ��A�^�C�s���O�������~����
    /// </summary>
    private void HideBalloon()
    {
        isDisplayed = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = "";
        balloonCanvas.enabled = false;
        isFullyDisplayed = false;
    }

    /// <summary>
    /// �e�L�X�g��1�������\�����Ă����R���[�`��
    /// �\��������AOnFullyDisplayed�C�x���g���Ăяo��
    /// </summary>
    private IEnumerator TypeText()
    {
        // 0�����ڂ���S���������[�v
        for (int i = 0; i <= fullText.Length; i++)
        {
            // 0�����ڂ���i�����ڂ܂ł�؂�o���ĕ\��
            dialogueText.text = fullText.Substring(0, i);

            // �w��b���ҋ@
            yield return new WaitForSeconds(typingSpeed);
        }

        // �S���\�������t���O�𗧂Ă�
        isFullyDisplayed = true;

        // �\�������̒ʒm�C�x���g���Ăяo���inull�`�F�b�N�t���j
        OnFullyDisplayed?.Invoke();
    }

}
