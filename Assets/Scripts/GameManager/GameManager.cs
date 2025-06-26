using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �Q�[���S�̂̐i�s�i�Q�[���J�n�E�S�[���E�Q�[���I�[�o�[�Ȃǁj���Ǘ�����N���X�B
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool isGameEnded = false;

    private void Awake()
    {
        // �V���O���g���̃C���X�^���X���������i���ɑ��݂��Ă���Δj���j
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // �V�[�����ׂ��ł��ێ��������ꍇ�͈ȉ���L����
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// �S�[�����B���ɌĂяo���B�ڐG�����v���C���[�̌��������ƂɃA�j���Đ��B
    /// </summary>
    // GameManager.cs ��
    public void OnGoalReached(Transform playerTransform)
    {
        if (isGameEnded) return;
        isGameEnded = true;

        Debug.Log("Goal reached! Stage Clear!");

        var playerController = playerTransform.GetComponent<PlayerAnimatorController>();
        if (playerController != null)
        {
            float direction = playerTransform.localScale.x >= 0 ? 1f : -1f;
            playerController.PlayGoalAnimation(direction);
        }
        else
        {
            Debug.LogWarning("PlayerAnimatorController��������܂���B");
        }

        Invoke(nameof(ShowClearScreen), 1.0f);
    }


    private void ShowClearScreen()
    {
        // ��FScene�J�ڂ�UI�\��
        Debug.Log("Showing clear screen...");
        // SceneManager.LoadScene("ClearScene");
    }

    /// <summary>
    /// �v���C���[���S���̏���
    /// </summary>
    public void OnPlayerDead()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        Debug.Log("Game Over!");

        // TODO: GameOver���o�⃊�g���C�{�^���\��
        Invoke(nameof(ShowGameOverScreen), 1.0f);
    }

    private void ShowGameOverScreen()
    {
        Debug.Log("Showing Game Over screen...");
        // SceneManager.LoadScene("GameOverScene");
    }
}
