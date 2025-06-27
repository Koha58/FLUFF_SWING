using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �Q�[���̏�ԁi�N���A�E�Q�[���I�[�o�[�Ȃǁj��UI�\���A���Ԑ�����Ǘ�����N���X
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Inspector Settings

    [Header("UI References")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject clearUI;
    [SerializeField] private GameObject gameOverUI;

    private float resultDelay = 2.0f;
    private float pauseDelayAfterResult = 0.5f;

    private float normalTimeScale = 1.0f;
    private float pausedTimeScale = 0.0f;

    #endregion

    #region State Management

    /// <summary>�V���O���g���̃C���X�^���X</summary>
    public static GameManager Instance { get; private set; }

    /// <summary>�Q�[���I���ς݂��ǂ����̃t���O</summary>
    private bool isGameEnded = false;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// �����������B�V���O���g���̐ݒ��TimeScale�̏��������s���B
    /// </summary>
    private void Awake()
    {
        // �Q�[���J�n���͒ʏ푬�x�œ��삷��悤�ɐݒ�
        Time.timeScale = normalTimeScale;

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
