// ステージセレクト>SceneManagerで使用

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SelectManager : MonoBehaviour
{
    #region インスペクター

    [Header("音量設定パネル")]
    [SerializeField] private GameObject setPanel;

    [Header("クリック音設定")]
    [Tooltip("ステージ選択・設定ボタンを押したときに鳴らす効果音")]
    public AudioClip onClickSE;
    [Tooltip("ホーム・閉じるボタンを押したときに鳴らす効果音")]
    public AudioClip offClickSE;

    [Tooltip("接続するAudioMixerのSEグループ")]
    public AudioMixerGroup seMixerGroup;

    private AudioSource audioSource;

    private string nextStageName;

    [Header("各ステージのロック")]   // 各ボタンのLock（子オブジェクト）をリストに設定
    [SerializeField] private GameObject[] stageLocks;

    #endregion

    #region パネルの初期設定・ロックの状態更新

    private void Start()
    {
        // --- パネルの初期設定 ---
        if (setPanel != null)
            setPanel.SetActive(false);

        // --- ステージロックの状態を更新 ---
        UpdateStageLocks();
    }

    // ======== 各ステージのロック状態を更新 =========
    private void UpdateStageLocks()
    {
        int clearedStage = PlayerPrefs.GetInt("ClearedStage", 0);

        for (int i = 0; i < stageLocks.Length; i++)
        {
            bool unlocked = i <= clearedStage; // クリア済み + 1 ステージまで解放
                                               // 初回起動時ステージ１解放
            if (stageLocks[i] != null)
                stageLocks[i].SetActive(!unlocked); // 非表示 = 解放済み
        }
    }

    #endregion

    #region ステージ遷移

    // ======== ステージ遷移 =========
    public void SelectStage(String StageName)
    {
        // --- ロックチェック ---
        int stageIndex = GetStageIndex(StageName);
        if (stageIndex >= 0 && stageLocks[stageIndex] != null && stageLocks[stageIndex].activeSelf)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE(offClickSE);
            Debug.Log(StageName + " はロック中です。");
            return;
        }

        nextStageName = StageName;

        // AudioManager経由でSEを再生（統一音量）
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        // 効果音の長さ分だけ待ってからシーン移動
        //float delay = onClickSE != null ? onClickSE.length : 0.1f;
        //Invoke(nameof(LoadNextScene), delay);

        LoadNextScene();
    }

    // シーン移動用
    private void LoadNextScene()
    {
        //SceneManager.LoadScene(nextStageName);
        TransitionManager.Instance.PlayTransitionAndLoadScene(nextStageName);
    }

    // ======== Stage名 → Index変換 ========
    private int GetStageIndex(string stageName)
    {
        // 例：Stage1, Stage2,... を前提
        if (stageName.StartsWith("Stage"))
        {
            if (int.TryParse(stageName.Replace("Stage", ""), out int num))
                return num - 1; // 配列は0始まりなので調整
        }
        return -1;
    }

    // ========タイトルに戻る========
    public void TitleBack(String StageName)
    {
        nextStageName = StageName;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        float delay = offClickSE != null ? offClickSE.length : 0.1f;
        Invoke(nameof(LoadNextScene), delay);
    }

    // ======== アプリ終了 =========
    public void OnApplicationQuit()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Application.Quit();
    }

    #endregion

    #region パネルの表示・非表示切り替え

    // ======== パネル表示 =========
    public void OnPanel()
    {
        if (setPanel == null) return;

        setPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        Debug.Log("設定パネル表示");
    }

    // ======== パネル非表示 =========
    public void OffPanel()
    {
        if (setPanel == null) return;

        setPanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Debug.Log("設定パネル非表示");
    }

    #endregion

    #region デバッグ用アンロック

    private void Update()
    {
        CheckDebugUnlockInput();
    }

    /// <summary>
    /// デバッグ用（全解除 / リセット）
    /// Ctrl + Shift + U → 全解除
    /// Ctrl + Shift + R → リセット
    /// </summary>
    private void CheckDebugUnlockInput()
    {
        // 全ステージ解放
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.U))
        {
            DebugUnlockAllStages();
        }

        // ロック初期化（ステージ1だけ解放）
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.R))
        {
            DebugResetStages();
        }
    }

    private void DebugUnlockAllStages()
    {
        int totalStages = stageLocks.Length;

        PlayerPrefs.SetInt("ClearedStage", totalStages);
        PlayerPrefs.Save();

        Debug.Log($"【DEBUG】全 {totalStages} ステージを解放しました");

        UpdateStageLocks();
    }

    private void DebugResetStages()
    {
        // 初期状態：ClearedStage = 0 → ステージ1だけ解放
        PlayerPrefs.SetInt("ClearedStage", 0);
        PlayerPrefs.Save();

        Debug.Log("【DEBUG】ステージロックを初期状態に戻しました（ステージ1のみ解放）");

        UpdateStageLocks();
    }

    /// <summary>
    /// データリセット（ステージ1のみ解放の初期状態）
    /// UIボタンから呼び出す用
    /// </summary>
    public void ResetStageData()
    {
        PlayerPrefs.SetInt("ClearedStage", 0);
        PlayerPrefs.Save();

        Debug.Log("【デバッグ】ステージデータをリセットしました（ステージ1のみ解放）");

        // ロック状態を即時更新
        UpdateStageLocks();
    }

    #endregion

}

//// ステージセレクト>SceneManagerで使用

//using System;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.Audio;

//public class SelectManager : MonoBehaviour
//{
//    [Header("音量設定パネル")]
//    [SerializeField] private GameObject setPanel;

//    [Header("クリック音設定")]
//    [Tooltip("ステージ選択・設定ボタンを押したときに鳴らす効果音")]
//    public AudioClip onClickSE;
//    [Tooltip("ホーム・閉じるボタンを押したときに鳴らす効果音")]
//    public AudioClip offClickSE;

//    [Tooltip("接続するAudioMixerのSEグループ")]
//    public AudioMixerGroup seMixerGroup;

//    private AudioSource audioSource;

//    private string nextStageName;

//    private void Start()
//    {
//        // --- パネルの初期設定 ---
//        if (setPanel != null)
//            setPanel.SetActive(false);
//    }

//    // ======== ステージ遷移 =========
//    public void SelectStage(String StageName)
//    {
//        nextStageName = StageName;

//        // AudioManager経由でSEを再生（統一音量）
//        if (AudioManager.Instance != null)
//            AudioManager.Instance.PlaySE(onClickSE);

//        // 効果音の長さ分だけ待ってからシーン移動
//        float delay = onClickSE != null ? onClickSE.length : 0.2f;
//        Invoke(nameof(LoadNextScene), delay);
//    }

//    private void LoadNextScene()
//    {
//        SceneManager.LoadScene(nextStageName);
//    }

//    // ======== パネル表示 =========
//    public void OnPanel()
//    {
//        if (setPanel == null) return;

//        setPanel.SetActive(true);

//        if (AudioManager.Instance != null)
//            AudioManager.Instance.PlaySE(onClickSE);

//        Debug.Log("設定パネル表示");
//    }

//    // ======== パネル非表示 =========
//    public void OffPanel()
//    {
//        if (setPanel == null) return;

//        setPanel.SetActive(false);

//        if (AudioManager.Instance != null)
//            AudioManager.Instance.PlaySE(offClickSE);

//        Debug.Log("設定パネル非表示");
//    }

//    // ========タイトルに戻る========
//    public void TitleBack(String StageName)
//    {
//        nextStageName = StageName;

//        if (AudioManager.Instance != null)
//            AudioManager.Instance.PlaySE(offClickSE);

//        float delay = offClickSE != null ? offClickSE.length : 0.2f;
//        Invoke(nameof(LoadNextScene), delay);
//    }

//    // ======== アプリ終了 =========
//    public void OnApplicationQuit()
//    {
//        if (AudioManager.Instance != null)
//            AudioManager.Instance.PlaySE(offClickSE);

//        Application.Quit();
//    }
//}
