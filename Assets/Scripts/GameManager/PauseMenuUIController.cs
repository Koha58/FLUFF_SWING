using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ポーズメニューUIを制御するクラス。
/// ポーズ時のUI表示・非表示やボタン操作を制御する。
/// </summary>
public class PauseMenuUIController : MonoBehaviour
{
    /// <summary>シングルトンインスタンス</summary>
    public static PauseMenuUIController Instance { get; private set; }

    [Header("UI本体")]
    [SerializeField]
    private GameObject pauseUI; // ポーズメニューのUIオブジェクト

    [Header("入力制御")]
    [SerializeField]
    private GraphicRaycaster pauseRaycaster; // ポーズ中のUI操作有効化用Raycaster

    /// <summary>セレクトシーンの名前（SceneManagerで使用）</summary>
    private const string SelectSceneName = "SelectScene";

    /// <summary>タイトルシーンの名前（SceneManagerで使用）</summary>
    private const string TitleSceneName = "SubTitleScene";

    /// <summary>
    /// 初期化処理。UIを非表示にし、ボタン操作も無効化する。
    /// </summary>
    private void Awake()
    {
        // シングルトンインスタンスを登録
        Instance = this;

        // 初期状態ではポーズUIを非表示にし、操作も不可にする
        pauseUI.SetActive(false);
        if (pauseRaycaster != null)
            pauseRaycaster.enabled = false;
    }

    //========================================
    // 外部から呼び出す UI 開閉 API
    //========================================

    /// <summary>
    /// ポーズメニューを開く。UI表示＋入力有効化。
    /// </summary>
    public void OpenPauseMenu()
    {
        pauseUI.SetActive(true);

        if (pauseRaycaster != null)
            pauseRaycaster.enabled = true;

        Debug.Log("OpenPause");
    }

    /// <summary>
    /// ポーズメニューを閉じる。UI非表示＋入力無効化。
    /// </summary>
    public void ClosePauseMenu()
    {
        pauseUI.SetActive(false);

        if (pauseRaycaster != null)
            pauseRaycaster.enabled = false;
    }

    //========================================
    // === UI ボタンハンドラ ===
    //========================================

    /// <summary>
    /// Resumeボタン：ゲームを再開してポーズメニューを閉じる。
    /// </summary>
    public void ClickResume()
    {
        GameManager.Instance.ResumeFromPauseMenu();
        ClosePauseMenu();
    }

    /// <summary>
    /// ステージ選択画面に戻るボタン（現在はタイトル画面と同一）。
    /// </summary>
    public void ClickQuitToStageSelect()
    {
        ResumeAndLoadScene(SelectSceneName);
    }

    /// <summary>
    /// タイトル画面に戻るボタン。
    /// </summary>
    public void ClickQuitToTitle()
    {
        ResumeAndLoadScene(TitleSceneName);
    }

    //========================================
    // 内部共通処理
    //========================================

    /// <summary>
    /// ゲームを再開して指定シーンを読み込む。
    /// </summary>
    /// <param name="sceneName">遷移先のシーン名</param>
    private void ResumeAndLoadScene(string sceneName)
    {
        GameManager.Instance.ResumeFromPauseMenu();
        SceneManager.LoadScene(sceneName);
    }
}
