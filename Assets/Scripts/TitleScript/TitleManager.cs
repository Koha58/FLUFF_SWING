using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

/// <summary>
/// タイトル画面全体のUI制御を行うマネージャ。
/// 
/// 主な役割：
/// ・各種設定／操作説明パネルの表示・非表示管理
/// ・同時に複数の操作説明パネルが開かないよう制御
/// ・ボタン操作時のSE再生
/// ・アプリ終了処理
/// 
/// ※ ゲーム進行ロジックは持たず、
///    「UIと演出」に責務を限定している
/// </summary>
public class TitleManager : MonoBehaviour
{
    #region Inspector References

    // =========================================================
    // Panels
    // =========================================================

    [Header("音量設定パネル")]
    [Tooltip("音量設定用のUIパネル")]
    [SerializeField] private GameObject setPanel;

    [Header("操作説明パネル")]
    [Tooltip("移動操作の説明パネル")]
    [SerializeField] private GameObject moveInfoPanel;

    [Tooltip("ワイヤー操作の説明パネル")]
    [SerializeField] private GameObject wireInfoPanel;

    [Tooltip("攻撃操作の説明パネル")]
    [SerializeField] private GameObject attackInfoPanel;

    /// <summary>
    /// 現在表示中の操作説明パネル（同時表示防止用）
    /// </summary>
    private GameObject currentInfoPanel;

    // =========================================================
    // Input Blocker
    // =========================================================

    [Header("入力ブロッカー")]
    [Tooltip(
        "ポップアップ表示中に背面ボタンの入力を遮断するための全画面パネル。\n" +
        "Canvas内で【背面ボタンより上】かつ【ポップアップより下】に配置してください。\n" +
        "Image(透明でもOK)で Raycast Target をONにするのがポイント。")]
    [SerializeField] private GameObject inputBlocker;

    // =========================================================
    // Audio
    // =========================================================

    [Header("クリック音設定")]

    [Tooltip("ステージセレクトシーン遷移時操作時に鳴らすSE")]
    [SerializeField] private AudioClip startClickSE;

    [Tooltip("決定・開く操作時に鳴らすSE")]
    [SerializeField] private AudioClip onClickSE;

    [Tooltip("閉じる・戻る操作時に鳴らすSE")]
    [SerializeField] private AudioClip offClickSE;

    [Tooltip("SEを流すAudioMixerGroup")]
    [SerializeField] private AudioMixerGroup seMixerGroup;

    /// <summary>ステージセレクトシーンの名前（SceneManagerで使用）</summary>
    private const string SelectSceneName = "SelectScene";

    #endregion

    // =========================================================
    // Unity Lifecycle
    // =========================================================

    private void Start()
    {
        // 起動時はすべてのパネルを非表示にする
        if (setPanel != null) setPanel.SetActive(false);
        if (moveInfoPanel != null) moveInfoPanel.SetActive(false);
        if (wireInfoPanel != null) wireInfoPanel.SetActive(false);
        if (attackInfoPanel != null) attackInfoPanel.SetActive(false);

        currentInfoPanel = null;

        // 起動時は入力ブロック解除
        SetPopupState(false);
    }

    // =========================================================
    // Scene Changer
    // =========================================================

    /// <summary>
    /// ステージセレクトシーンに遷移する
    /// </summary>
    public void ChangeScenes()
    {
        if (TransitionManager.Instance == null)
        {
            Debug.LogError("TransitionManagerが見つかりません。直接シーンロードします。");
            SceneManager.LoadScene(SelectSceneName);
            return;
        }

        // ★遷移開始できた時だけSEを鳴らす
        bool started = TransitionManager.Instance.TryPlayTransitionAndLoadScene(SelectSceneName);
        if (started && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(startClickSE);
        }
    }


    // =========================================================
    // Panel Control (Internal)
    // =========================================================

    /// <summary>
    /// 入力ブロッカーのON/OFFを切り替える。
    /// ポップアップ表示中は true（背面入力遮断）にする。
    /// </summary>
    private void SetPopupState(bool isOpen)
    {
        if (inputBlocker != null)
            inputBlocker.SetActive(isOpen);
    }

    /// <summary>
    /// 何かしらのポップアップ（設定 or 操作説明）が開いているか
    /// </summary>
    private bool IsAnyPopupOpen()
    {
        bool setOpen = setPanel != null && setPanel.activeSelf;
        bool infoOpen = currentInfoPanel != null && currentInfoPanel.activeSelf;
        return setOpen || infoOpen;
    }

    /// <summary>
    /// ポップアップを閉じたあと、まだ別ポップアップが開いていないか確認し、
    /// 全部閉じたときだけ入力ブロッカーを解除する。
    /// </summary>
    private void RefreshInputBlocker()
    {
        SetPopupState(IsAnyPopupOpen());
    }

    /// <summary>
    /// 指定した操作説明パネルを表示する。
    /// すでに別の操作説明パネルが開いている場合は閉じてから表示する。
    /// </summary>
    private void ShowInfoPanel(GameObject panel)
    {
        if (panel == null) return;

        // すでに別パネルが表示されていれば閉じる
        if (currentInfoPanel != null && currentInfoPanel != panel)
        {
            currentInfoPanel.SetActive(false);
        }

        // 指定パネルを表示
        currentInfoPanel = panel;
        currentInfoPanel.SetActive(true);

        // ポップアップが開いたので背面入力を遮断
        SetPopupState(true);

        // 決定音（※背面ボタンが押せなくなるので、余計なSEは鳴らなくなる）
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);
    }

    // =========================================================
    // Application
    // =========================================================

    /// <summary>
    /// アプリ終了ボタン用
    /// </summary>
    public void OnApplicationQuit()
    {
        // 閉じるSEを鳴らす（※実機では鳴り切らない場合あり）
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Application.Quit();
    }

    // =========================================================
    // Panel Show
    // =========================================================

    /// <summary>
    /// 音量設定パネルを表示する
    /// </summary>
    public void OnSetPanel()
    {
        if (setPanel == null) return;

        setPanel.SetActive(true);

        // ポップアップが開いたので背面入力を遮断
        SetPopupState(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        Debug.Log("音量設定パネル表示");
    }

    /// <summary>
    /// 操作説明（移動）パネルを表示する
    /// </summary>
    public void OnInfoMovePanel()
    {
        ShowInfoPanel(moveInfoPanel);
        Debug.Log("操作方法：移動 パネル表示");
    }

    /// <summary>
    /// 操作説明（ワイヤー）パネルを表示する
    /// </summary>
    public void OnInfoWirePanel()
    {
        ShowInfoPanel(wireInfoPanel);
        Debug.Log("操作方法：ワイヤー パネル表示");
    }

    /// <summary>
    /// 操作説明（攻撃）パネルを表示する
    /// </summary>
    public void OnInfoAttackPanel()
    {
        ShowInfoPanel(attackInfoPanel);
        Debug.Log("操作方法：攻撃 パネル表示");
    }

    // =========================================================
    // Panel Hide
    // =========================================================

    /// <summary>
    /// 音量設定パネルを閉じる
    /// </summary>
    public void OffSetPanel()
    {
        if (setPanel == null) return;

        setPanel.SetActive(false);

        // ほかのポップアップが開いていなければ、背面入力を戻す
        RefreshInputBlocker();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Debug.Log("音量設定パネル非表示");
    }

    /// <summary>
    /// 表示中の操作説明パネルを閉じる
    /// </summary>
    public void OffInfoPanel()
    {
        // 表示中でなければ何もしない
        if (currentInfoPanel == null) return;

        currentInfoPanel.SetActive(false);
        currentInfoPanel = null;

        // ほかのポップアップが開いていなければ、背面入力を戻す
        RefreshInputBlocker();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Debug.Log("操作方法パネル非表示");
    }

    // =========================================================
    // Debug
    // =========================================================

#if UNITY_EDITOR
    /// <summary>
    /// デバッグ用：
    /// ステージ解放状態を初期化する
    /// </summary>
    private void DebugResetStages()
    {
        // 初期状態：ClearedStage = 0 → ステージ1のみ解放
        PlayerPrefs.SetInt("ClearedStage", 0);
        PlayerPrefs.Save();

        Debug.Log("【DEBUG】ステージロックを初期状態に戻しました（ステージ1のみ解放）");
    }
#endif
}
