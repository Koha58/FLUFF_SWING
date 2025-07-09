using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームの状態（クリア・ゲームオーバーなど）とUI表示、時間制御を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Inspector Settings

    // -------------------------------
    // UI関連オブジェクトと時間制御設定
    // ゲーム結果（クリア or ゲームオーバー）の表示や時間停止処理に使用
    // -------------------------------

    // 結果全体を覆うパネル
    [SerializeField] private GameObject resultPanel;

    // ステージクリア時に表示するUI
    [SerializeField] private GameObject clearUI;

    // ゲームオーバー時に表示するUI
    [SerializeField] private GameObject gameOverUI;

    // ゲーム終了後にUIを表示するまでの待機時間（秒）
    private float resultDelay = 2.0f;

    // UI表示後にゲームを一時停止するまでの遅延時間（秒）
    private float pauseDelayAfterResult = 0.5f;

    // 通常のゲーム進行速度（Time.timeScale = 1）
    private float normalTimeScale = 1.0f;

    // 一時停止時のゲーム進行速度（Time.timeScale = 0）
    private float pausedTimeScale = 0.0f;


    #endregion

    #region State Management

    /// <summary>シングルトンのインスタンス</summary>
    public static GameManager Instance { get; private set; }

    /// <summary>ゲーム終了済みかどうかのフラグ</summary>
    private bool isGameEnded = false;

    #endregion

    #region Unity Lifecycle

    /// <summary>攻撃入力アクション（Input Systemの"Attack"）</summary>
    private InputAction pauseAction;

    private bool isPaused = false;

    /// <summary>
    /// 初期化処理。シングルトンの設定とTimeScaleの初期化を行う。
    /// </summary>
    private void Awake()
    {
        // ゲーム開始時は通常速度で動作するように設定
        Time.timeScale = normalTimeScale;

        // Input Systemから"Pause"アクションを取得
        pauseAction = InputSystem.actions.FindAction("Pause");

        // すでに他のインスタンスが存在する場合は重複を避けるため自身を破棄
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // シングルトンインスタンスを設定
        Instance = this;
    }

    #endregion

    #region External Event Handlers

    /// <summary>
    /// プレイヤーがゴールに到達した時の処理
    /// すでにゲーム終了状態なら処理を無視する
    /// </summary>
    /// <param name="playerTransform">ゴールしたプレイヤーのTransform</param>
    public void OnGoalReached(Transform playerTransform)
    {
        // すでにゲーム終了していれば何もしない
        if (isGameEnded) return;

        // ゲーム終了フラグを立てる
        isGameEnded = true;

        Debug.Log("Goal reached! Stage Clear!");

        // プレイヤーの向きを判定しゴール用アニメーションを再生
        var playerController = playerTransform.GetComponent<PlayerAnimatorController>();
        if (playerController != null)
        {
            float direction = playerTransform.localScale.x >= 0 ? 1f : -1f;
            playerController.PlayGoalAnimation(direction);
        }

        // 指定遅延時間後にクリア画面を表示する処理を呼ぶ
        Invoke(nameof(ShowClearScreen), resultDelay);
    }

    /// <summary>
    /// プレイヤーが死亡した時の処理
    /// すでにゲーム終了状態なら処理を無視する
    /// </summary>
    public void OnPlayerDead()
    {
        // すでにゲーム終了していれば何もしない
        if (isGameEnded) return;

        // ゲーム終了フラグを立てる
        isGameEnded = true;

        Debug.Log("Game Over!");

        // 指定遅延時間後にゲームオーバー画面を表示する処理を呼ぶ
        Invoke(nameof(ShowGameOverScreen), resultDelay);
    }

    #endregion

    #region UI & Screen Management

    /// <summary>
    /// クリア画面を表示する処理
    /// </summary>
    private void ShowClearScreen()
    {
        Debug.Log("Showing clear screen...");

        // クリアUIのみ表示し、他は非表示に切り替え
        ShowResultUI(clearUI);

        // UI表示後にゲームを一時停止するための呼び出しを遅延させる
        Invoke(nameof(PauseGame), pauseDelayAfterResult);
    }

    /// <summary>
    /// ゲームオーバー画面を表示する処理
    /// </summary>
    private void ShowGameOverScreen()
    {
        Debug.Log("Showing Game Over screen...");

        // ゲームオーバーUIのみ表示し、他は非表示に切り替え
        ShowResultUI(gameOverUI);

        // UI表示後にゲームを一時停止するための呼び出しを遅延させる
        Invoke(nameof(PauseGame), pauseDelayAfterResult);
    }

    /// <summary>
    /// 結果画面用UIの切り替え処理
    /// </summary>
    /// <param name="targetUI">表示するUIオブジェクト</param>
    private void ShowResultUI(GameObject targetUI)
    {
        // 結果パネル自体は常に表示する
        resultPanel.SetActive(true);

        // 全UIを一旦非表示にする
        clearUI.SetActive(false);
        gameOverUI.SetActive(false);

        // 引数のUIだけを表示状態にする
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
            // ここでポーズUIを非表示に（別クラスでも可）
            PauseMenuUIController.Instance?.ClosePauseMenu();
        }
        else
        {
            PauseGame();
            // ここでポーズUIを表示（別クラスでも可）
            PauseMenuUIController.Instance?.OpenPauseMenu();
        }

        isPaused = !isPaused;
    }


    /// <summary>
    /// ゲームを一時停止する（Time.timeScaleを0に設定）
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = pausedTimeScale;
    }

    /// <summary>
    /// ゲームの一時停止を解除する（Time.timeScaleを通常値に戻す）
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = normalTimeScale;
    }

    #endregion
}
