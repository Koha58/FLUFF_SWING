using System.Collections;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// �L�����N�^�[�̓���ɐ����o���e�L�X�g��\������R���g���[���[�N���X�B
/// - �v���C���[�Ƃ̋����ɉ����Ď����I�ɕ\���^��\����؂�ւ���
/// - �����o�����̃e�L�X�g�̓^�C�s���O����1�������\�������
/// - �����o���̓L�����N�^�[�̓���ɌŒ肳��A��ɃJ�����̕�������
/// </summary>
public class BalloonTextController : MonoBehaviour
{
    //===============================
    // �萔��`
    //===============================

    // �����\�����鐁���o�����b�Z�[�W
    private const string DEFAULT_INITIAL_MESSAGE = "�{�^�������C�t�Ɍ�������H";

    // 1������\������̂ɂ����鎞�ԁi�b�j
    private const float DEFAULT_TYPING_SPEED = 0.05f;

    // �v���C���[�����̋����ȓ��ɋ߂Â��Ɛ����o�����\�������i���[�g���j
    private const float DEFAULT_SHOW_DISTANCE = 3.0f;

    // �����o���̕\���ʒu���L�����N�^�[�̓���ɂ��炷�I�t�Z�b�g
    private static readonly Vector3 DEFAULT_BALLOON_OFFSET = new Vector3(0f, 1.5f, 0f);

    // �^�C�s���O���̐ݒ�
    private static readonly string[] TYPING_SE_NAMES = { "Typing1", "Typing2", "Typing3" };
    private const float TYPING_SE_COOLDOWN = 1.0f;         // SE��炷�ŏ��Ԋu�i�A�Ŗh�~�j
    private const float TYPING_SE_PLAY_PROBABILITY = 0.7f;  // SE����m���i1.0 = ��ɖ�j

    //===============================
    // �t�B�[���h�ϐ�
    //===============================
    private TextMeshProUGUI dialogueText;     // �����o�����̃e�L�X�g
    private Canvas balloonCanvas;             // �����o����\������L�����o�X
    private RectTransform textRectTransform;  // �e�L�X�g��RectTransform�i�T�C�Y�ύX�p�j
    private Transform player;                 // �v���C���[��Transform
    private Camera mainCamera;                // ���C���J����

    private string fullText = DEFAULT_INITIAL_MESSAGE; // ���ݕ\�����郁�b�Z�[�W�S��
    private Vector2 originalTextSize;                 // �e�L�X�g�̏����T�C�Y��ۑ�
    private Coroutine typingCoroutine;                // �^�C�s���O�A�j���[�V�����p�R���[�`���Q��

    private bool isDisplayed = false;        // ���ݐ����o�����\������
    private bool isFullyDisplayed = false;   // �e�L�X�g���Ō�܂ŕ\�����ꂽ���ǂ���

    private float typingSpeed = DEFAULT_TYPING_SPEED; // ���݂̃^�C�s���O���x
    private float showDistance = DEFAULT_SHOW_DISTANCE; // �\����؂�ւ��鋗��
    private Vector3 offset = DEFAULT_BALLOON_OFFSET;     // �\���ʒu�̃I�t�Z�b�g

    private float lastTypeSETime = 0f;  // �Ō�Ƀ^�C�s���OSE��炵�����Ԃ��L�^

    //===============================
    // �v���p�e�B�E�C�x���g
    //===============================
    public bool IsFullyDisplayed => isFullyDisplayed; // �O������Q�Ɖ\�ȕ\�������t���O
    public event System.Action OnFullyDisplayed;      // �^�C�s���O�������̃C�x���g

    //===============================
    // Unity�W���R�[���o�b�N
    //===============================
    private void Start()
    {
        // --- �K�v�ȃR���|�[�l���g�̎擾 ---
        balloonCanvas = GetComponentInChildren<Canvas>();
        dialogueText = GetComponentInChildren<TextMeshProUGUI>();
        textRectTransform = dialogueText?.GetComponent<RectTransform>();

        // �K�{�v�f��������Ȃ���Γ�����~
        if (balloonCanvas == null || dialogueText == null || textRectTransform == null)
        {
            Debug.LogError("Canvas�܂���TextMeshProUGUI�܂���RectTransform��������܂���B");
            enabled = false;
            return;
        }

        // �e�L�X�g�̏����T�C�Y��ێ��i��Ń��Z�b�g�p�j
        originalTextSize = textRectTransform.sizeDelta;

        // ������Ԃ͔�\��
        fullText = DEFAULT_INITIAL_MESSAGE;
        balloonCanvas.enabled = false;

        // --- �v���C���[�ƃJ�����̎擾 ---
        mainCamera = Camera.main;
        player = GameObject.FindWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player�I�u�W�F�N�g��������܂���B");
            enabled = false;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // �v���C���[�Ƃ̋����𑪒�
        float distance = Vector3.Distance(transform.position, player.position);

        // ��苗���ȓ��Ȃ琁���o����\��
        if (distance < showDistance && !isDisplayed)
        {
            ShowBalloon();
        }
        // ���ꂽ���\����
        else if (distance >= showDistance && isDisplayed)
        {
            HideBalloon();
        }

        // �����o�����\�����Ȃ�A����ɒǏ]���J�����ɐ���
        if (isDisplayed)
        {
            balloonCanvas.transform.position = transform.position + offset;
            balloonCanvas.transform.LookAt(
                balloonCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    //===============================
    // �����o�����䏈��
    //===============================

    /// <summary>
    /// �����o����\�����āA�e�L�X�g�̃^�C�s���O�A�j���[�V�������J�n����B
    /// ���ɕ\�����ł���΁A�R���[�`������U�~�߂ă��X�^�[�g�B
    /// </summary>
    private void ShowBalloon()
    {
        isDisplayed = true;
        balloonCanvas.enabled = true;
        dialogueText.text = "";
        isFullyDisplayed = false;

        // �O��̃^�C�s���O�������Ă����ꍇ�͒�~
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // �^�C�s���O�J�n
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// �����o�����\���ɂ��āA�A�j���[�V�������~�B
    /// </summary>
    private void HideBalloon()
    {
        isDisplayed = false;

        // �^�C�s���O���Ȃ璆�f
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // �e�L�X�g��Canvas�����Z�b�g
        dialogueText.text = "";
        balloonCanvas.enabled = false;
        isFullyDisplayed = false;
        textRectTransform.sizeDelta = originalTextSize;
    }

    /// <summary>
    /// �w�肵�����b�Z�[�W�𐁂��o���ɕ\������B
    /// �e�L�X�g����ʒu�I�t�Z�b�g���I�v�V�����Ŏw��\�B
    /// </summary>
    public void ShowMessage(string message, float optionalWidth = -1f, Vector2? customOffset = null)
    {
        // �T�C�Y�E�ʒu��������
        if (textRectTransform != null)
        {
            textRectTransform.sizeDelta = originalTextSize;
            textRectTransform.anchoredPosition = Vector2.zero;
        }

        // �\�����b�Z�[�W�X�V
        fullText = message;

        // ���w�肪����ꍇ�͕ύX
        if (optionalWidth > 0 && textRectTransform != null)
            textRectTransform.sizeDelta = new Vector2(optionalWidth, originalTextSize.y);

        // �ʒu�I�t�Z�b�g���w�肳��Ă���ΓK�p
        if (customOffset.HasValue && textRectTransform != null)
            textRectTransform.anchoredPosition = customOffset.Value;

        // �\���J�n
        ShowBalloon();
    }

    /// <summary>
    /// �������b�Z�[�W�i�f�t�H���g���j�ɖ߂��B
    /// </summary>
    public void RestoreInitialMessage()
    {
        ShowMessage(DEFAULT_INITIAL_MESSAGE);
    }

    //===============================
    // �^�C�s���O�A�j���[�V��������
    //===============================

    /// <summary>
    /// �e�L�X�g��1�������\������R���[�`���B
    /// �������ƂɌ��ʉ���炵�A�S�\����ɃC�x���g�𔭍s�B
    /// </summary>
    private IEnumerator TypeText()
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            // �����������؂�o���ĕ\���i��: "�{", "�{�^", "�{�^��"...�j
            dialogueText.text = fullText.Substring(0, i);

            if (i < fullText.Length)
            {
                char c = fullText[i];

                // �X�y�[�X�ł͂Ȃ� && ��莞�Ԍo�� && �m���`�F�b�N�ɍ��i������SE�Đ�
                bool canPlaySE = !char.IsWhiteSpace(c)
                                 && Time.time - lastTypeSETime > TYPING_SE_COOLDOWN
                                 && UnityEngine.Random.value < TYPING_SE_PLAY_PROBABILITY;

                if (canPlaySE)
                {
                    // �����_���ȃ^�C�v����I��ōĐ�
                    string seName = TYPING_SE_NAMES[UnityEngine.Random.Range(0, TYPING_SE_NAMES.Length)];
                    AudioManager.Instance.PlaySE(seName);
                    lastTypeSETime = Time.time;
                }
            }

            // ���̕����܂ł̑ҋ@�i�^�C�v���x�ɉ����āj
            yield return new WaitForSeconds(typingSpeed);
        }

        // �S���\������
        isFullyDisplayed = true;
        OnFullyDisplayed?.Invoke();
    }
}
