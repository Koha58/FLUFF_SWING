using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// �Q�[���̏�ԁi�N���A�E�Q�[���I�[�o�[�Ȃǁj��UI�\���A���Ԑ�����Ǘ�����N���X
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Inspector Settings

    // -------------------------------
    // UI�֘A�I�u�W�F�N�g�Ǝ��Ԑ���ݒ�
    // �Q�[�����ʁi�N���A or �Q�[���I�[�o�[�j�̕\���⎞�Ԓ�~�����Ɏg�p
    // -------------------------------

    // ���ʑS�̂𕢂��p�l��
    [SerializeField] private GameObject resultPanel;

    // �X�e�[�W�N���A���ɕ\������UI
    [SerializeField] private GameObject clearUI;

    // �Q�[���I�[�o�[���ɕ\������UI
    [SerializeField] private GameObject gameOverUI;

    // �Q�[���I�����UI��\������܂ł̑ҋ@���ԁi�b�j
    private float resultDelay = 2.0f;

    // UI�\����ɃQ�[�����ꎞ��~����܂ł̒x�����ԁi�b�j
    private float pauseDelayAfterResult = 0.5f;

    // �ʏ�̃Q�[���i�s���x�iTime.timeScale = 1�j
    private float normalTimeScale = 1.0f;

    // �ꎞ��~���̃Q�[���i�s���x�iTime.timeScale = 0�j
    private float pausedTimeScale = 0.0f;


    #endregion

    #region State Management

    /// <summary>�V���O���g���̃C���X�^���X</summary>
    public static GameManager Instance { get; private set; }

    /// <summary>�Q�[���I���ς݂��ǂ����̃t���O</summary>
    private bool isGameEnded = false;

    #endregion

    #region Unity Lifecycle

    /// <summary>�U�����̓A�N�V�����iInput System��"Attack"�j</summary>
    private InputAction pauseAction;

    private bool isPaused = false;

    /// <summary>
    /// �����������B�V���O���g���̐ݒ��TimeScale�̏��������s���B
    /// </summary>
    private void Awake()
    {
        // �Q�[���J�n���͒ʏ푬�x�œ��삷��悤�ɐݒ�
        Time.timeScale = normalTimeScale;

        // Input System����"Pause"�A�N�V�������擾
        pauseAction = InputSystem.actions.FindAction("Pause");

        // ���łɑ��̃C���X�^���X�����݂���ꍇ�͏d��������邽�ߎ��g��j��
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // �V���O���g���C���X�^���X��ݒ�
        Instance = this;
    }

    #endregion

    #region External Event Handlers

    /// <summary>
    /// �v���C���[���S�[���ɓ��B�������̏���
    /// ���łɃQ�[���I����ԂȂ珈���𖳎�����
    /// </summary>
    /// <param name="playerTransform">�S�[�������v���C���[��Transform</param>
    public void OnGoalReached(Transform playerTransform)
    {
        // ���łɃQ�[���I�����Ă���Ή������Ȃ�
        if (isGameEnded) return;

        // �Q�[���I���t���O�𗧂Ă�
        isGameEnded = true;

        Debug.Log("Goal reached! Stage Clear!");

        // �v���C���[�̌����𔻒肵�S�[���p�A�j���[�V�������Đ�
        var playerController = playerTransform.GetComponent<PlayerAnimatorController>();
        if (playerController != null)
        {
            float direction = playerTransform.localScale.x >= 0 ? 1f : -1f;
            playerController.PlayGoalAnimation(direction);
        }

        // �w��x�����Ԍ�ɃN���A��ʂ�\�����鏈�����Ă�
        Invoke(nameof(ShowClearScreen), resultDelay);
    }

    /// <summary>
    /// �v���C���[�����S�������̏���
    /// ���łɃQ�[���I����ԂȂ珈���𖳎�����
    /// </summary>
    public void OnPlayerDead()
    {
        // ���łɃQ�[���I�����Ă���Ή������Ȃ�
        if (isGameEnded) return;

        // �Q�[���I���t���O�𗧂Ă�
        isGameEnded = true;

        Debug.Log("Game Over!");

        // �w��x�����Ԍ�ɃQ�[���I�[�o�[��ʂ�\�����鏈�����Ă�
        Invoke(nameof(ShowGameOverScreen), resultDelay);
    }

    #endregion

    #region UI & Screen Management

    /// <summary>
    /// �N���A��ʂ�\�����鏈��
    /// </summary>
    private void ShowClearScreen()
    {
        Debug.Log("Showing clear screen...");

        // �N���AUI�̂ݕ\�����A���͔�\���ɐ؂�ւ�
        ShowResultUI(clearUI);

        // UI�\����ɃQ�[�����ꎞ��~���邽�߂̌Ăяo����x��������
        Invoke(nameof(PauseGame), pauseDelayAfterResult);
    }

    /// <summary>
    /// �Q�[���I�[�o�[��ʂ�\�����鏈��
    /// </summary>
    private void ShowGameOverScreen()
    {
        Debug.Log("Showing Game Over screen...");

        // �Q�[���I�[�o�[UI�̂ݕ\�����A���͔�\���ɐ؂�ւ�
        ShowResultUI(gameOverUI);

        // UI�\����ɃQ�[�����ꎞ��~���邽�߂̌Ăяo����x��������
        Invoke(nameof(PauseGame), pauseDelayAfterResult);
    }

    /// <summary>
    /// ���ʉ�ʗpUI�̐؂�ւ�����
    /// </summary>
    /// <param name="targetUI">�\������UI�I�u�W�F�N�g</param>
    private void ShowResultUI(GameObject targetUI)
    {
        // ���ʃp�l�����̂͏�ɕ\������
        resultPanel.SetActive(true);

        // �SUI����U��\���ɂ���
        clearUI.SetActive(false);
        gameOverUI.SetActive(false);

        // ������UI������\����Ԃɂ���
        targetUI.SetActive(true);
    }

    #endregion

    #region Time Control

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.Enable();
            pauseAction.performed += OnPausePerformed;
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed;
            pauseAction.Disable();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (isGameEnded) return;

        if (isPaused)
        {
            ResumeGame();
            // �����Ń|�[�YUI���\���Ɂi�ʃN���X�ł��j
            PauseMenuUIController.Instance?.ClosePauseMenu();
        }
        else
        {
            PauseGame();
            // �����Ń|�[�YUI��\���i�ʃN���X�ł��j
            PauseMenuUIController.Instance?.OpenPauseMenu();
        }

        isPaused = !isPaused;
    }


    /// <summary>
    /// �Q�[�����ꎞ��~����iTime.timeScale��0�ɐݒ�j
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = pausedTimeScale;
    }

    /// <summary>
    /// �Q�[���̈ꎞ��~����������iTime.timeScale��ʏ�l�ɖ߂��j
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = normalTimeScale;
    }

    #endregion
}
