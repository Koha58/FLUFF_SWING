using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// �Q�[���S�̂̏�ԁi�Q�[���N���A�A�Q�[���I�[�o�[�Ȃǁj�ƁA
/// ����ɔ���UI�\���E���Ԑ���E���͏������Ǘ�����N���X�B
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Inspector Settings

    /// <summary>�Q�[���I�����UI��\������܂ł̑ҋ@���ԁi�b�j</summary>
    [SerializeField]
    private float resultDelay = 2.0f;

    /// <summary>UI�\����ɃQ�[�����ꎞ��~����܂ł̒x�����ԁi�b�j</summary>
    [SerializeField]
    private float pauseDelayAfterResult = 0.5f;

    /// <summary>�ʏ�̃Q�[���i�s���x�iTime.timeScale = 1�j</summary>
    private readonly float normalTimeScale = 1.0f;

    /// <summary>�ꎞ��~���̃Q�[���i�s���x�iTime.timeScale = 0�j</summary>
    private readonly float pausedTimeScale = 0.0f;

    [Header("SE�ݒ�")]
    [SerializeField]
    private AudioSource seAudioSource; // SE���Đ�����AudioSource

    [SerializeField]
    private AudioClip clearSE;         // �X�e�[�W�N���A����SE

    #endregion

    #region State Management

    /// <summary>�V���O���g���C���X�^���X</summary>
    public static GameManager Instance { get; private set; }

    /// <summary>�Q�[�����I�����Ă��邩�ǂ���</summary>
    private bool isGameEnded = false;

    #endregion

    #region Unity Lifecycle

    /// <summary>�|�[�Y�p��Input�A�N�V����</summary>
    private InputAction pauseAction;

    /// <summary>���݃|�[�Y��Ԃ��ǂ���</summary>
    private bool isPaused = false;

    /// <summary>
    /// �����������F�V���O���g���̐ݒ��Input�A�N�V�����̎擾�ATimeScale�̏��������s���B
    /// </summary>
    private void Awake()
    {
        // �Q�[���J�n���̃^�C���X�P�[����ʏ�ɐݒ�
        Time.timeScale = normalTimeScale;

        // Input System ���� "Pause" �A�N�V�������擾
        pauseAction = InputSystem.actions.FindAction("Pause");
        if (pauseAction == null)
        {
            Debug.LogError("Pause�A�N�V������������܂���BInputActions�̐ݒ���m�F���Ă��������B");
        }

        // �V���O���g���C���X�^���X�̐ݒ�
        Instance = this;
    }

    #endregion

    #region External Event Handlers

    /// <summary>
    /// �v���C���[���S�[���ɓ��B�����ۂɌĂяo�����B
    /// UI�\���E�A�j���[�V�����Đ��E�Q�[���ꎞ��~�������s���B
    /// </summary>
    /// <param name="playerTransform">�S�[�������v���C���[��Transform</param>
    public void OnGoalReached(Transform playerTransform)
    {
        // ���łɏI�����Ă���Ζ���
        if (isGameEnded) return;

        // �Q�[���I���t���O�𗧂Ă�
        isGameEnded = true;
        Debug.Log("Goal reached! Stage Clear!");

        // �v���C���[�̌����ɉ������S�[���A�j���[�V�������Đ�
        var playerController = playerTransform.GetComponent<PlayerAnimatorController>();
        if (playerController != null)
        {
            float direction = playerTransform.localScale.x >= 0 ? 1f : -1f;
            playerController.PlayGoalAnimation(direction);
        }

        // �w��b����Ƀ��U���gUI��\��
        Invoke(nameof(NotifyClear), resultDelay);
    }

    /// <summary>
    /// �v���C���[�����S�����Ƃ��ɌĂяo�����B
    /// </summary>
    public void OnPlayerDead()
    {
        // ���łɏI�����Ă���Ζ���
        if (isGameEnded) return;

        // �Q�[���I���t���O�𗧂ĂāA�Q�[���I�[�o�[������x���Ăяo��
        isGameEnded = true;
        Invoke(nameof(NotifyGameOver), resultDelay);
    }

    /// <summary>�X�e�[�W�N���A�ʒm�����i�����p�j</summary>
    private void NotifyClear()
    {
        ShowResult(GameResult.Clear);
    }

    /// <summary>�Q�[���I�[�o�[�ʒm�����i�����p�j</summary>
    private void NotifyGameOver()
    {
        ShowResult(GameResult.GameOver);
    }

    /// <summary>
    /// �Q�[�����ʂɉ��������U���gUI�\���ƈꎞ��~���s���B
    /// </summary>
    /// <param name="result">�N���A or �Q�[���I�[�o�[</param>
    private void ShowResult(GameResult result)
    {
        // ���ʂɉ�����UI��\��
        GameResultUIController.Instance.ShowResult(result);

        // �Q�[���N���A���̂�SE���Đ�
        if (result == GameResult.Clear && seAudioSource != null && clearSE != null)
        {
            seAudioSource.PlayOneShot(clearSE);
        }

        // UI�\����Ɉꎞ��~������x�����s
        Invoke(nameof(PauseGame), pauseDelayAfterResult);
    }

    #endregion

    #region Time Control

    /// <summary>
    /// �L��������Pause�A�N�V������o�^�B
    /// </summary>
    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.Enable();
            pauseAction.performed += OnPausePerformed;
        }
    }

    /// <summary>
    /// ����������Pause�A�N�V�����������B
    /// </summary>
    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed;
            pauseAction.Disable();
        }
    }

    /// <summary>
    /// �|�[�Y���͂��s��ꂽ�Ƃ��̏����B
    /// �Q�[���̈ꎞ��~/�ĊJ��UI�\��������s���B
    /// </summary>
    /// <param name="context">���̓C�x���g�R���e�L�X�g</param>
    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // �Q�[���I����̓|�[�Y�ł��Ȃ��悤�ɂ���
        if (isGameEnded) return;

        if (isPaused)
        {
            // �ĊJ�����F�^�C���X�P�[����߂��A�|�[�YUI�����
            ResumeGame();
            PauseMenuUIController.Instance?.ClosePauseMenu();
        }
        else
        {
            // �ꎞ��~�����F�^�C���X�P�[����0�ɂ��A�|�[�YUI���J��
            PauseGame();
            PauseMenuUIController.Instance?.OpenPauseMenu();
        }

        // �|�[�Y��Ԃ̃t���O���g�O��
        isPaused = !isPaused;
    }

    /// <summary>
    /// �|�[�YUI����Resume���ꂽ�ۂɌĂяo���B
    /// �^�C���X�P�[���ƃt���O�����Z�b�g����B
    /// </summary>
    public void ResumeFromPauseMenu()
    {
        // �Q�[���I����͉������Ȃ�
        if (isGameEnded) return;

        // �ĊJ����
        ResumeGame();
        isPaused = false;
    }

    /// <summary>
    /// �Q�[�����ꎞ��~�iTime.timeScale = 0�j�ɂ���B
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = pausedTimeScale;
    }

    /// <summary>
    /// �Q�[���̈ꎞ��~�������iTime.timeScale = 1�j�ɖ߂��B
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = normalTimeScale;
    }

    #endregion
}
