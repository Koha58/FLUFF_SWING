// ステージセレクト>SceneManagerで使用

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SelectManager : MonoBehaviour
{
    #region === インスペクター ===

    [Header("音量設定パネル")]
    [SerializeField] private GameObject setPanel;
    [Header("データリセットパネル")]
    [SerializeField] private GameObject resetPanel;

    [Header("クリック音設定")]
    [Tooltip("ステージ選択・設定ボタンを押したときに鳴らす効果音")]
    public AudioClip onClickSE;
    [Tooltip("ホーム・閉じるボタンを押したときに鳴らす効果音")]
    public AudioClip offClickSE;
    [Tooltip("データリセットボタンを押したときに鳴らす効果音")]
    public AudioClip resetClickSE;
    [Tooltip("ステージのロックを解除するときに鳴らす効果音")]
    public AudioClip unlockSE;

    [Tooltip("接続するAudioMixerのSEグループ")]
    public AudioMixerGroup seMixerGroup;

    private AudioSource audioSource;

    private string nextStageName;

    [Header("各ステージのロック")]   // 各ボタンのLock（子オブジェクト）をリストに設定
    [SerializeField] private GameObject[] stageLocks;

    #endregion

    #region === パネルの初期設定 ===

    private void Start()
    {
        // --- パネルの初期設定 ---
        if (setPanel != null)
            setPanel.SetActive(false);
        if (resetPanel != null)
            resetPanel.SetActive(false);

        // --- ステージロックの状態を更新 ---
        UpdateStageLocks();
    }

    #endregion

    #region === ロックの状態更新 ===

    private void UpdateStageLocks()
    {
        int clearedStage = Mathf.Max(1, PlayerPrefs.GetInt("ClearedStage", 0));
        int lastUnlocedStage = PlayerPrefs.GetInt("LastUnlockedStage", -1);

        for (int i = 0; i < stageLocks.Length; i++)
        {
            GameObject lockObj = stageLocks[i];
            if (lockObj == null) continue;

            int stageNumber = i + 1;

            // --- 解放済み→非表示 ---
            if (stageNumber < lastUnlocedStage)
            {
                lockObj.SetActive(false);
                continue;
            }

            // --- 今回新たに解放されたステージ→アニメーション再生 ---
            if (stageNumber == lastUnlocedStage)
            {
                var anim = lockObj.GetComponent<LockOpen>();
                lockObj.SetActive(true);

                if (anim != null)
                {
                    anim.PlayUnlockAnimation(() =>
                    {
                        if (AudioManager.Instance != null)
                            AudioManager.Instance.PlaySE(unlockSE);

                        lockObj.SetActive(false);
                    });
                }
                else
                {
                    lockObj.SetActive(false);
                }
                continue;
            }

            // --- 未解放→表示 ---
            int unlockedMax = PlayerPrefs.GetInt("UnlockedMaxStage", 1);
            bool isUnlocked = stageNumber <= unlockedMax;
            lockObj.SetActive(!isUnlocked);
        }

        // --- 一度再生したらリセット ---
        if (lastUnlocedStage != -1)
        {
            PlayerPrefs.SetInt("LastUnlockedStage", -1);
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region === ステージ遷移 ===

    public void SelectStage(String StageName)
    {
        // --- ロックチェック ---
        int stageIndex = GetStageIndex(StageName);
        if (stageIndex >= 0 && stageLocks[stageIndex] != null && stageLocks[stageIndex].activeSelf)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE(offClickSE);
            Debug.Log("<color=green>" + StageName + " はロック中です。</color>");
            return;
        }

        nextStageName = StageName;

        // --- AudioManager経由でSEを再生(統一音量) ---
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        // --- 効果音の長さ分だけ待ってからシーン移動 ---
        float delay = onClickSE != null ? onClickSE.length : 0.1f;
        Invoke(nameof(LoadNextScene), delay);
    }

    // --- シーン移動用 ---
    private void LoadNextScene()
    {
        TransitionManager.Instance.PlayTransitionAndLoadScene(nextStageName);
    }

    // --- Stage名 → Index変換 ---
    private int GetStageIndex(string stageName)
    {
        if (stageName.StartsWith("Stage"))
        {
            if (int.TryParse(stageName.Replace("Stage", ""), out int num))
                return num - 1; // 配列は0始まりなので調整
        }
        return -1;
    }

    // --- タイトルに戻る ---
    public void TitleBack(String StageName)
    {
        nextStageName = StageName;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        float delay = offClickSE != null ? offClickSE.length : 0.1f;
        Invoke(nameof(LoadNextScene), delay);
    }

    // --- アプリ終了 ---
    public void OnApplicationQuit()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Application.Quit();
    }

    #endregion

    #region パネルの表示・非表示切り替え

    // --- パネル表示 ---
    public void OnSetPanel()
    {
        if (setPanel == null) return;

        setPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        Debug.Log("<color=green>音量設定パネル表示</color>");
    }

    public void OnResetPanel()
    {
        if (resetPanel == null) return;

        resetPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(resetClickSE);

        Debug.Log("<color=green>データリセットパネル表示</color>");
    }

    // --- パネル非表示 ---
    public void OffSetPanel()
    {
        if (setPanel == null) return;

        setPanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Debug.Log("<color=green>音量設定パネル非表示</color>");
    }

    public void OffResetPanel()
    {
        if (resetPanel == null) return;

        resetPanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Debug.Log("<color=green>データリセットパネル非表示</color>");
    }

    #endregion

    #region デバッグ用アンロック・データリセット

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
        // --- 全ステージ解放 ---
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.U))
        {
            DebugUnlockAllStages();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE(onClickSE);
        }

        // --- ロック初期化（ステージ1だけ解放）---
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.R))
        {
            DebugResetStages();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE(offClickSE);
        }
    }

    private void DebugUnlockAllStages()
    {
        int totalStages = stageLocks.Length;

        PlayerPrefs.SetInt("ClearedStage", totalStages);
        PlayerPrefs.SetInt("UnlockedMaxStage", totalStages);
        PlayerPrefs.Save();

        Debug.Log("<b><color=orange>【デバッグ】Ctrl+Shift+U→全ステージを解放しました</color></b>");

        UpdateStageLocks();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(resetClickSE);
    }

    private void DebugResetStages()
    {
        // --- 初期状態：ClearedStage = 0 → ステージ1だけ解放 ---
        PlayerPrefs.SetInt("ClearedStage", 0);
        PlayerPrefs.SetInt("LastUnlockedStage", -1);
        PlayerPrefs.SetInt("UnlockedMaxStage", 1);
        PlayerPrefs.Save();

        Debug.Log("<b><color=orange>【デバッグ】Ctrl+Shift+R→ステージロックを初期状態に戻しました（ステージ1のみ解放）</color></b>");

        UpdateStageLocks();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(resetClickSE);
    }

    /// <summary>
    /// データリセット（ステージ1のみ解放の初期状態）
    /// UIボタンから呼び出す用
    /// </summary>
    public void ResetStageData()
    {
        PlayerPrefs.SetInt("ClearedStage", 0);
        PlayerPrefs.SetInt("LastUnlockedStage", -1);
        PlayerPrefs.SetInt("UnlockedMaxStage", 1);
        PlayerPrefs.Save();

        Debug.Log("<b><color=orange>ステージデータをリセットしました（ステージ1のみ解放）</color></b>");

        // --- ロック状態を即時更新 ---
        UpdateStageLocks();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(resetClickSE);
    }

    #endregion

}