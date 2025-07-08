using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// �L�����N�^�[�̓���ɐ����o���e�L�X�g��\������R���g���[���[�N���X�B
/// �E�v���C���[�Ƃ̋����ɉ����Đ����o���̕\���E��\���������ؑ�
/// �E�e�L�X�g�̓^�C�s���O����1�������\��
/// �E�\���ʒu�̓L�����N�^�[�̓���ɌŒ肵�A��ɃJ����������
/// </summary>
public class BalloonTextController : MonoBehaviour
{
    // --- �萔��` ---
    // �����\�����鐁���o���̃��b�Z�[�W
    private const string DefaultInitialMessage = "�{�^�������C�t�Ɍ�������H";

    // �^�C�s���O�A�j���[�V������1�����\�����ԁi�b�j
    private const float DefaultTypingSpeed = 0.05f;

    // �v���C���[���琁���o����\�����鋗����臒l�i���[�g���j
    private const float DefaultShowDistance = 3.0f;

    // �����o����\������ۂ̃L�����N�^�[�̓���ւ̃I�t�Z�b�g�ʒu
    private static readonly Vector3 DefaultOffset = new Vector3(0, 1.5f, 0);

    // �����o���̃e�L�X�g�R���|�[�l���g
    private TextMeshProUGUI dialogueText;

    // �����o���̃L�����o�X�R���|�[�l���g
    private Canvas balloonCanvas;

    // �e�L�X�g��RectTransform�i����ʒu�����p�j
    private RectTransform textRectTransform;

    // �v���C���[��Transform�Q��
    private Transform player;

    // ���C���J�����Q��
    private Camera mainCamera;

    // �\�����������b�Z�[�W�S��
    private string fullText = DefaultInitialMessage;

    // �e�L�X�g�����T�C�Y�ۑ��i����ʒu�����̃��Z�b�g�p�j
    private Vector2 originalTextSize;

    // �^�C�s���O�A�j���[�V�����̃R���[�`���Q��
    private Coroutine typingCoroutine;

    // �����o�������ݕ\�������ǂ����̏�ԊǗ�
    private bool isDisplayed = false;

    // �^�C�s���O�A�j���[�V�����������������̃t���O
    private bool isFullyDisplayed = false;

    // �^�C�s���O�\���������������擾�p�v���p�e�B
    public bool IsFullyDisplayed => isFullyDisplayed;

    // �^�C�s���O�������ɌĂ΂��C�x���g
    public event System.Action OnFullyDisplayed;

    // 1����������̕\�����ԁi�b�j
    private float typingSpeed = DefaultTypingSpeed;

    // �v���C���[����̕\������臒l
    private float showDistance = DefaultShowDistance;

    // �����o���̕\���ʒu�I�t�Z�b�g�i�L��������j
    private Vector3 offset = DefaultOffset;

    /// <summary>
    /// ����������
    /// �E�R���|�[�l���g�擾�Ƒ��݃`�F�b�N
    /// �E�v���C���[�ƃJ�����̎Q�Ǝ擾
    /// �E�����o���̏�����\���ݒ�
    /// </summary>
    void Start()
    {
        // �����o���pCanvas�ƃe�L�X�g�ARectTransform���擾
        balloonCanvas = GetComponentInChildren<Canvas>();
        dialogueText = GetComponentInChildren<TextMeshProUGUI>();
        textRectTransform = dialogueText?.GetComponent<RectTransform>();

        // �K�{�R���|�[�l���g�������Ă��Ȃ���΃G���[�o�͂��A������~
        if (balloonCanvas == null || dialogueText == null || textRectTransform == null)
        {
            Debug.LogError("Canvas�܂���TextMeshProUGUI�܂���RectTransform��������܂���B");
            enabled = false;
            return;
        }

        // �e�L�X�g�̌��T�C�Y��ۑ��i����ʒu�����̃��Z�b�g�p�j
        originalTextSize = textRectTransform.sizeDelta;

        // �����\�����b�Z�[�W���Z�b�g
        fullText = DefaultInitialMessage;

        // �����o�����ŏ��͔�\���ɐݒ�
        balloonCanvas.enabled = false;

        // ���C���J�����̎Q�Ƃ��擾
        mainCamera = Camera.main;

        // Player�I�u�W�F�N�g���^�O�ŒT��Transform���擾
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player�I�u�W�F�N�g��������܂���B");
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// ���t���[���X�V����
    /// �E�v���C���[�Ƃ̋����𑪂萁���o���\����ON/OFF����
    /// �E�����o���̈ʒu���L�����N�^�[����ɌŒ肵�J������������������
    /// </summary>
    void Update()
    {
        // �v���C���[�����݂��Ȃ���Ώ������Ȃ�
        if (player == null) return;

        // �L�����ƃv���C���[�Ԃ̋������擾
        float distance = Vector3.Distance(transform.position, player.position);

        // �v���C���[���߂���ΐ����o���\���J�n�A�����Δ�\��
        if (distance < showDistance && !isDisplayed)
        {
            ShowBalloon();
        }
        else if (distance >= showDistance && isDisplayed)
        {
            HideBalloon();
        }

        // �����o�����\�����Ȃ�ʒu�X�V���ăJ����������
        if (isDisplayed)
        {
            // �L�����N�^�[�̓���ɐ����o����Canvas��z�u
            balloonCanvas.transform.position = transform.position + offset;

            // �����o������ɃJ�����̕����Ɍ�����i���ʂ��猩����悤�ɉ�]�j
            balloonCanvas.transform.LookAt(
                balloonCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// �����o����\�����A�^�C�s���O�A�j���[�V�������J�n����B
    /// ���ɃA�j���[�V�����������Ă������~���Ă���ĊJ����B
    /// </summary>
    private void ShowBalloon()
    {
        // �����o���\����Ԃ�ON�ɐݒ�
        isDisplayed = true;

        // Canvas��\��
        balloonCanvas.enabled = true;

        // �e�L�X�g����ɂ��ă^�C�s���O�J�n����
        dialogueText.text = "";

        // �^�C�s���O�����t���O�����Z�b�g
        isFullyDisplayed = false;

        // ���Ƀ^�C�s���O�R���[�`���������Ă������~
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // �^�C�s���O�R���[�`�����J�n
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// �����o���Ɏw�肵�����b�Z�[�W��\������B
    /// �e�L�X�g���̒�����e�L�X�g�ʒu�̔������I�t�Z�b�g���\�B
    /// </summary>
    /// <param name="message">�\�����������b�Z�[�W������</param>
    /// <param name="optionalWidth">�e�L�X�g�\�����i-1�ŕύX�Ȃ��j</param>
    /// <param name="customOffset">�e�L�X�g�̕\���ʒu�I�t�Z�b�g�i�C�Ӂj</param>
    public void ShowMessage(string message, float optionalWidth = -1f, Vector2? customOffset = null)
    {
        // �܂��e�L�X�g�̃T�C�Y�ƈʒu�������l�ɖ߂�
        if (textRectTransform != null)
        {
            textRectTransform.sizeDelta = originalTextSize;
            textRectTransform.anchoredPosition = Vector2.zero;
        }

        // ���b�Z�[�W���Z�b�g
        fullText = message;

        // �����w�肳��Ă���΃e�L�X�g����ύX
        if (optionalWidth > 0 && textRectTransform != null)
        {
            textRectTransform.sizeDelta = new Vector2(optionalWidth, originalTextSize.y);
        }

        // �ʒu�̃I�t�Z�b�g���w�肳��Ă���ΓK�p
        if (customOffset.HasValue && textRectTransform != null)
        {
            textRectTransform.anchoredPosition = customOffset.Value;
        }

        // �����o����\���i�^�C�s���O�J�n�j
        ShowBalloon();
    }

    /// <summary>
    /// �����o�����\���ɂ��ă^�C�s���O�A�j���[�V�������~����B
    /// �e�L�X�g�T�C�Y�����ɖ߂��������s���B
    /// </summary>
    private void HideBalloon()
    {
        // �����o���\����Ԃ�OFF�ɐݒ�
        isDisplayed = false;

        // �^�C�s���O�R���[�`���������Ă���Β�~
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // �e�L�X�g����ɂ���
        dialogueText.text = "";

        // Canvas���\���ɂ���
        balloonCanvas.enabled = false;

        // �^�C�s���O�����t���O�����Z�b�g
        isFullyDisplayed = false;

        // �e�L�X�g�̃T�C�Y�����̃T�C�Y�ɖ߂�
        if (textRectTransform != null)
        {
            textRectTransform.sizeDelta = originalTextSize;
        }
    }

    /// <summary>
    /// �������b�Z�[�W�ɖ߂��ĕ\���������B
    /// </summary>
    public void RestoreInitialMessage()
    {
        ShowMessage(DefaultInitialMessage);
    }

    /// <summary>
    /// �^�C�s���O�A�j���[�V�����̃R���[�`���B
    /// ���b�Z�[�W��1���������Ԃɕ\�����A�Ō�Ɋ����t���O�𗧂ĂăC�x���g�𔭍s�B
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator TypeText()
    {
        // 0�����ڂ���S�������܂�1�������\��
        for (int i = 0; i <= fullText.Length; i++)
        {
            // i�����ڂ܂Ő؂�o���ăe�L�X�g�\��
            dialogueText.text = fullText.Substring(0, i);

            // ���̕����܂ŏ����ҋ@
            yield return new WaitForSeconds(typingSpeed);
        }

        // �S�����\������
        isFullyDisplayed = true;

        // �����C�x���g�𔭉΁i�o�^������ΌĂ΂��j
        OnFullyDisplayed?.Invoke();
    }
}
