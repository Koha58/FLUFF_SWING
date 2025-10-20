using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の状態（ゲームクリア、ゲームオーバーなど）と、
/// それに伴うUI表示・時間制御・入力処理を管理するクラス。
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Inspector Settings

    /// <summary>ゲーム終了後にUIを表示するまでの待機時間（秒）</summary>
    [SerializeField]
    private float resultDelay = 2.0f;

    /// <summary>UI表示後にゲームを一時停止するまでの遅延時間（秒）</summary>
    [SerializeField]
    private float pauseDelayAfterResult = 0.5f;

    /// <summary>通常のゲーム進行速度（Time.timeScale = 1）</summary>
    private readonly float normalTimeScale = 1.0f;

    /// <summary>一時停止時のゲーム進行速度（Time.timeScale = 0）</summary>
    private readonly float pausedTimeScale = 0.0f;

    [Header("SE設定")]
    [SerializeField] private AudioClip clearSE;      // ステージクリア時のSE
    [SerializeField] private AudioClip gameOverSE;   // ゲームオーバー時のSE
    [SerializeField] private AudioClip pauseOpenSE;  // ポーズメニューを開いた時のSE
    [SerializeField] private AudioClip pauseCloseSE; // ポーズメニューを閉じた時のSE

    #endregion

    #region State Management

    /// <summary>シングルトンインスタンス</summary>
    public static GameManager Instance { get; private set; }

    /// <summary>ゲームが終了しているかどうか</summary>
    private bool isGameEnded = false;

    #endregion

    #region Unity Lifecycle

    /// <summary>ポーズ用のInputアクション</summary>
    private InputAction pauseAction;

    /// <summary>現在ポーズ状態かどうか</summary>
    private bool isPaused = false;

    /// <summary>
    /// 初期化処理：シングルトンの設定とInputアクションの取得、TimeScaleの初期化を行う。
    /// </summary>
    private void Awake()
    {
        // ゲーム開始時のタイムスケールを通常に設定
        Time.timeScale = normalTimeScale;

        // Input System から "Pause" アクションを取得
        pauseAction = InputSystem.actions.FindAction("Pause");
        if (pauseAction == null)
        {
            Debug.LogError("Pauseアクションが見つかりません。InputActionsの設定を確認してください。");
        }

        // シングルトンインスタンスの設定
        Instance = this;
    }

    #endregion

    #region External Event Handlers

    /// <summary>
    /// プレイヤーがゴールに到達した際に呼び出される。
    /// UI表示・アニメーション再生・ゲーム一時停止処理を行う。
    /// </summary>
    /// <param name="playerTransform">ゴールしたプレイヤーのTransform</param>
    public void OnGoalReached(Transform playerTransform)
    {
        // すでに終了していれば無視
        if (isGameEnded) return;

        // ゲーム終了フラグを立てる
        isGameEnded = true;
        Debug.Log("Goal reached! Stage Clear!");

        // 🟢 プレイヤー操作を停止
        var playerMove = playerTransform.GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = false;
        }

        // 攻撃なども止めたい場合
        var playerAttack = playerTransform.GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        // プレイヤーの向きに応じたゴールアニメーションを再生
        var playerController = playerTransform.GetComponent<PlayerAnimatorController>();
        if (playerController != null)
        {
            float direction = playerTransform.localScale.x >= 0 ? 1f : -1f;
            playerController.PlayGoalAnimation(direction);
        }

        // 指定秒数後にリザルトUIを表示
        Invoke(nameof(NotifyClear), resultDelay);
    }

    /// <summary>
    /// プレイヤーが死亡したときに呼び出される。
    /// </summary>
    public void OnPlayerDead()
    {
        // すでに終了していれば無視
        if (isGameEnded) return;

        // ゲーム終了フラグを立てて、ゲームオーバー処理を遅延呼び出し
        isGameEnded = true;
        Invoke(nameof(NotifyGameOver), resultDelay);
    }

    /// <summary>ステージクリア通知処理（内部用）</summary>
    private void NotifyClear()
    {
        ShowResult(GameResult.Clear);
    }

    /// <summary>ゲームオーバー通知処理（内部用）</summary>
    private void NotifyGameOver()
    {
        ShowResult(GameResult.GameOver);
    }

    /// <summary>
    /// ゲーム結果に応じたリザルトUI表示と一時停止を行う。
    /// </summary>
    /// <param name="result">クリア or ゲームオーバー</param>
    private void ShowResult(GameResult result)
    {
        // 結果に応じたUIを表示
        GameResultUIController.Instance.ShowResult(result);

        // ゲームクリア時のSEを再生
        if (result == GameResult.Clear && clearSE != null)
        {
            AudioManager.Instance.PlaySE(clearSE);
        }
        // ゲームオーバー時のSEを再生
        else
        {
            AudioManager.Instance.PlaySE(gameOverSE);
        }

        // UI表示後に一時停止処理を遅延実行
        Invoke(nameof(PauseGame), pauseDelayAfterResult);
    }

    #endregion

    #region Time Control

    /// <summary>
    /// 有効化時にPauseアクションを登録。
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
    /// 無効化時にPauseアクションを解除。
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
    /// ポーズ入力が行われたときの処理。
    /// ゲームの一時停止/再開とUI表示制御を行う。
    /// </summary>
    /// <param name="context">入力イベントコンテキスト</param>
    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // ゲーム終了後はポーズできないようにする
        if (isGameEnded) return;

        if (isPaused)
        {
            // 再開処理：タイムスケールを戻し、ポーズUIを閉じる
            ResumeGame();
            PauseMenuUIController.Instance?.ClosePauseMenu();

            // 🔊 ポーズを閉じるSEを再生
            if (pauseCloseSE != null)
            {
                AudioManager.Instance.PlaySE(pauseCloseSE);
            }
        }
        else
        {
            // 一時停止処理：タイムスケールを0にし、ポーズUIを開く
            PauseGame();
            PauseMenuUIController.Instance?.OpenPauseMenu();

            // 🔊 ポーズを開くSEを再生
            if (pauseOpenSE != null)
            {
                AudioManager.Instance.PlaySE(pauseOpenSE);
            }
        }

        // ポーズ状態のフラグをトグル
        isPaused = !isPaused;
    }

    /// <summary>
    /// ポーズUIからResumeされた際に呼び出す。
    /// タイムスケールとフラグをリセットする。
    /// </summary>
    public void ResumeFromPauseMenu()
    {
        // ゲーム終了後は何もしない
        if (isGameEnded) return;

        // 再開処理
        ResumeGame();
        isPaused = false;

        // 🔊 ポーズを閉じるSEを再生
        if (pauseCloseSE != null)
        {
            AudioManager.Instance.PlaySE(pauseCloseSE);
        }
    }

    /// <summary>
    /// ゲームを一時停止（Time.timeScale = 0）にする。
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = pausedTimeScale;
    }

    /// <summary>
    /// ゲームの一時停止を解除（Time.timeScale = 1）に戻す。
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = normalTimeScale;
    }

    #endregion
}
