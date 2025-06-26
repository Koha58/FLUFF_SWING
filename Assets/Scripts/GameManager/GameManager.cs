using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の進行（ゲーム開始・ゴール・ゲームオーバーなど）を管理するクラス。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool isGameEnded = false;

    private void Awake()
    {
        // シングルトンのインスタンスを初期化（既に存在していれば破棄）
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // シーンを跨いでも保持したい場合は以下を有効化
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// ゴール到達時に呼び出す。接触したプレイヤーの向きをもとにアニメ再生。
    /// </summary>
    // GameManager.cs 側
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
            Debug.LogWarning("PlayerAnimatorControllerが見つかりません。");
        }

        Invoke(nameof(ShowClearScreen), 1.0f);
    }


    private void ShowClearScreen()
    {
        // 例：Scene遷移やUI表示
        Debug.Log("Showing clear screen...");
        // SceneManager.LoadScene("ClearScene");
    }

    /// <summary>
    /// プレイヤー死亡時の処理
    /// </summary>
    public void OnPlayerDead()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        Debug.Log("Game Over!");

        // TODO: GameOver演出やリトライボタン表示
        Invoke(nameof(ShowGameOverScreen), 1.0f);
    }

    private void ShowGameOverScreen()
    {
        Debug.Log("Showing Game Over screen...");
        // SceneManager.LoadScene("GameOverScene");
    }
}
