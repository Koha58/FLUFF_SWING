// ステージセレクト>SceneManagerで使用

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Security.Cryptography.X509Certificates;
// 10/23追加ここから
using System.Text.RegularExpressions;
// 10/23追加ここまで

public class SelectManager : MonoBehaviour
{
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

    // 10/23追加ここから
    [Header("各ステージのロック")]
    [SerializeField] private GameObject[] stageLocks;
    // 10/23追加ここまで

    private void Start()
    {
        // --- パネルの初期設定 ---
        if (setPanel != null)
            setPanel.SetActive(false);

        // 10/23追加ここから
        // --- ステージロックの状態を更新 ---
        UpdateStageLocks();
        // 10/23追加ここまで
    }

    // 10/23追加ここから
    // ======== 各ステージのロック状態を更新 =========
    private void UpdateStageLocks()
    {
        int clearedStage = PlayerPrefs.GetInt("ClearedStage", 0);

        for (int i = 0; i < stageLocks.Length; i++)
        {
            bool unlocked = i <= clearedStage; // クリア済み + 1 ステージまで解放
            if (stageLocks[i] != null)
                stageLocks[i].SetActive(!unlocked); // 非表示 = 解放済み
        }
    }
    // 10/23追加ここまで

    // ======== ステージ遷移 =========
    public void SelectStage(String StageName)
    {
        // 10/23追加ここから
        // --- ロックチェック ---
        int stageIndex = GetStageIndex(StageName);
        if (stageIndex >= 0 && stageLocks[stageIndex] != null && stageLocks[stageIndex].activeSelf)
        {
            Debug.Log(StageName + " はロック中です。");
            return;
        }
        // 10/23追加ここまで

        nextStageName = StageName;

        // AudioManager経由でSEを再生（統一音量）
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        // 効果音の長さ分だけ待ってからシーン移動
        float delay = onClickSE != null ? onClickSE.length : 0.2f;
        Invoke(nameof(LoadNextScene), delay);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextStageName);
    }

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

    // ========タイトルに戻る========
    public void TitleBack(String StageName)
    {
        nextStageName = StageName;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        float delay = offClickSE != null ? offClickSE.length : 0.2f;
        Invoke(nameof(LoadNextScene), delay);
    }

    // ======== アプリ終了 =========
    public void OnApplicationQuit()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Application.Quit();
    }

    // 10/23追加ここから
    // ======== Stage名 → Index変換 ========
    private int GetStageIndex(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName)) return -1;

        // 大文字小文字無視して「Stage」から始まるかチェック
        if (!stageName.StartsWith("Stage", StringComparison.OrdinalIgnoreCase))
            return -1;

        // 「Stage」以降にある連続した数字を抽出
        Match m = Regex.Match(stageName, @"Stage(\d+)");    // 要修正
        if (m.Success)
        {
            int num = int.Parse(m.Groups[1].Value);
            return num - 1; // 配列は0始まり
        }

        return -1;
    }
    // 10/23追加ここまで
}


//// ステージセレクト>SceneManagerで使用

//using System;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.Audio;
//using System.Security.Cryptography.X509Certificates;

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
