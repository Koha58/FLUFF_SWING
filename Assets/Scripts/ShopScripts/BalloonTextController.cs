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
    // �����o�����̃e�L�X�g�R���|�[�l���g
    private TextMeshProUGUI dialogueText;
    // �\���������S���e�L�X�g
    private string fullText = "�{�^�������C�t�Ɍ�������H";
    // �^�C�s���O���x�i1����������̑҂����ԁj
    private float typingSpeed = 0.05f;

    // �v���C���[��Transform�i�����v�Z�p�j
    private Transform player;
    // �����o����\������ő勗��
    private float showDistance = 3.0f;

    // �L�����N�^�[����ւ̃I�t�Z�b�g�ʒu
    private Vector3 offset = new Vector3(0, 1.5f, 0);
    // ���C���J�����i�����o�����J���������Ɍ����邽�߁j
    private Camera mainCamera;

    // �����o���pCanvas�̎Q��
    private Canvas balloonCanvas;

    // �^�C�s���O�R���[�`���̎Q��
    private Coroutine typingCoroutine;

    // �����o���\�������ǂ����̃t���O
    private bool isDisplayed = false; 

    /// <summary>
    /// �����������B
    /// �q�I�u�W�F�N�g����Canvas��TextMeshProUGUI���擾���A
    /// �v���C���[��J�����̎Q�Ƃ��ݒ肵�܂��B
    /// �����o���͏�����\���ɂ��܂��B
    /// </summary>
    void Start()
    {
        // �q�I�u�W�F�N�g����Canvas���擾
        balloonCanvas = GetComponentInChildren<Canvas>();
        // �q�I�u�W�F�N�g����TextMeshProUGUI���擾
        dialogueText = GetComponentInChildren<TextMeshProUGUI>();

        // Canvas�����݂��Ȃ��ꍇ�̓G���[�\�����ď������~
        if (balloonCanvas == null)
        {
            Debug.LogError("�����o��Canvas��������܂���B�q�I�u�W�F�N�g��Canvas��ݒu���Ă��������B");
            enabled = false;
            return;
        }
        // TextMeshProUGUI�����݂��Ȃ��ꍇ�̓G���[�\�����ď������~
        if (dialogueText == null)
        {
            Debug.LogError("TextMeshProUGUI��������܂���B�q�I�u�W�F�N�g��TextMeshProUGUI��ݒu���Ă��������B");
            enabled = false;
            return;
        }

        // ������Ԃ͐����o�����\���ɂ���
        balloonCanvas.enabled = false;

        // ���C���J���������ݒ�Ȃ�V�[�����̃��C���J�������擾
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // �v���C���[�I�u�W�F�N�g���^�O�uPlayer�v����T���ăZ�b�g
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player�I�u�W�F�N�g��������܂���B�^�O�uPlayer�v��ݒ肵�Ă��������B");
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// ���t���[���Ăяo�����X�V�����B
    /// �v���C���[�Ƃ̋�������Ő����o���̕\��/��\����؂�ւ��A
    /// �\�����͐����o���̈ʒu���L�����N�^�[����ɌŒ肵�A
    /// �J������������������Billboard�������s���܂��B
    /// </summary>
    void Update()
    {
        // �v���C���[�ƃL�����N�^�[�̋������v�Z
        float distance = Vector3.Distance(transform.position, player.position);

        // �v���C���[���߂Â����琁���o����\��
        if (distance < showDistance && !isDisplayed)
        {
            ShowBalloon();
        }
        // �v���C���[�������������琁���o�����\��
        else if (distance >= showDistance && isDisplayed)
        {
            HideBalloon();
        }

        // �����o���\�����͖��t���[�������o���̈ʒu�Ɖ�]�𒲐�
        if (isDisplayed)
        {
            // �L�����N�^�[�̓���ioffset����j�ɐ����o��Canvas���ړ�
            balloonCanvas.transform.position = transform.position + offset;

            // �J�����ɏ�ɐ��ʂ������悤�ɉ�]�iBillboard���ʁj
            balloonCanvas.transform.LookAt(
                balloonCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// �����o����\�����A�e�L�X�g�̃^�C�s���O�\�����J�n���܂��B
    /// ���Ƀ^�C�s���O���Ȃ��~���Ă���ăX�^�[�g���܂��B
    /// </summary>
    private void ShowBalloon()
    {
        isDisplayed = true;            // �\�����t���O�𗧂Ă�
        balloonCanvas.enabled = true;  // Canvas��\����Ԃɂ���
        dialogueText.text = "";        // �e�L�X�g����ɏ�����

        // ���Ƀ^�C�s���O���Ȃ��~���Ă���V���ɊJ�n
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // �^�C�s���O���J�n
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// �����o�����\���ɂ��A�^�C�s���O�\�����~���܂��B
    /// </summary>
    private void HideBalloon()
    {
        isDisplayed = false;          // �\�����t���O��|��

        // �^�C�s���O���̃R���[�`��������Β�~
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = "";        // �e�L�X�g���N���A
        balloonCanvas.enabled = false; // Canvas���\���ɂ���
    }

    /// <summary>
    /// fullText��1�������^�C�s���O���ɕ\������R���[�`���B
    /// </summary>
    private IEnumerator TypeText()
    {
        // 0��������S�����܂ŏ��ɕ\��
        for (int i = 0; i <= fullText.Length; i++)
        {
            // �����������؂�o���ăe�L�X�g�ɃZ�b�g
            dialogueText.text = fullText.Substring(0, i);

            // typingSpeed�b�҂i1�����\�����鎞�ԁj
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
