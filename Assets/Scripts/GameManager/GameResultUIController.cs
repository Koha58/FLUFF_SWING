using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームのリザルトUI（クリア／ゲームオーバー）を制御するクラス。
/// </summary>
public enum GameResult { Clear, GameOver }

public class GameResultUIController : MonoBehaviour
{
    /// <summary>シングルトンインスタンス</summary>
    public static GameResultUIController Instance { get; private set; }

    [Header("共通")]
    [SerializeField]
    private GraphicRaycaster raycaster; // UIボタンの有効化制御用Raycaster

    [Header("結果別 UI")]
    [SerializeField]
    private GameObject clearUI; // ステージクリア時に表示するUI
    [SerializeField]
    private GameObject gameOverUI; // ゲームオーバー時に表示するUI

    /// <summary>タイトルシーンの名前（SceneManagerで使用）</summary>
    private const string TitleSceneName = "TitleScene";

    /// <summary>通常のゲーム進行速度（Time.timeScale = 1）</summary>
    private readonly float normalTimeScale = 1.0f;

    /// <summary>
    /// 初期化処理。UI非表示化とシングルトン登録を行う。
    /// </summary>
    private void Awake()
    {
        // シングルトン登録
        Instance = this;

        // ゲーム開始時点ではボタンを無効化し、UIを非表示にしておく
        raycaster.enabled = false;
        clearUI.SetActive(false);
        gameOverUI.SetActive(false);
    }

    //========================================
    // 外部 API
    //========================================

    /// <summary>
    /// 指定された結果に応じてリザルトUIを表示する。
    /// </summary>
    /// <param name="result">ゲーム結果（Clear or GameOver）</param>
    public void ShowResult(GameResult result)
    {
        // 結果に応じたUIを表示
        clearUI.SetActive(result == GameResult.Clear);
        gameOverUI.SetActive(result == GameResult.GameOver);

        // ボタン操作を有効にする
        raycaster.enabled = true;

        // デバッグログ（開発用）
        Debug.Log("ShowResult called. clearUI.activeSelf: " + clearUI.activeSelf);
        Debug.Log("clearUI position: " + clearUI.transform.position);
        Debug.Log("clearUI isInHierarchy: " + clearUI.activeInHierarchy);
    }

    //========================================
    // UIボタン ハンドラ
    //========================================

    /// <summary>
    /// リトライボタン押下時の処理。現在のシーンを再読み込みする。
    /// </summary>
    public void ClickRetry()
    {
        ResumeGameTime();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// タイトル画面に戻るボタン押下時の処理。
    /// </summary>
    public void ClickQuitToTitle()
    {
        ResumeGameTime();
        SceneManager.LoadScene(TitleSceneName);
    }

    /// <summary>
    /// 次のステージへ進むボタン押下時の処理（現在は同一シーンを再ロード）。
    /// </summary>
    public void ClickNext()
    {
        ResumeGameTime();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// ステージ選択画面に戻るボタン押下時の処理（現在はタイトル画面）。
    /// </summary>
    public void ClickQuitToStageSelect()
    {
        ResumeGameTime();
        SceneManager.LoadScene(TitleSceneName);
    }

    //========================================
    // 内部共通処理
    //========================================

    /// <summary>
    /// Time.timeScale を通常の1.0に戻す。
    /// </summary>
    private void ResumeGameTime()
    {
        Time.timeScale = normalTimeScale;
    }
}
