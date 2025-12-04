// ステージセレクト>SceneManagerで使用

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class TitleManager : MonoBehaviour
{
    #region インスペクター

    [Header("音量設定パネル")]
    [SerializeField] private GameObject setPanel;
    [Header("データリセットパネル")]
    [SerializeField] private GameObject resetPanel;

    [Header("クリック音設定")]
    [Tooltip("ステージ選択・設定ボタンを押したときに鳴らす効果音")]
    public AudioClip onClickSE;
    [Tooltip("ホーム・閉じるボタンを押したときに鳴らす効果音")]
    public AudioClip offClickSE;

    [Tooltip("接続するAudioMixerのSEグループ")]
    public AudioMixerGroup seMixerGroup;

    private AudioSource audioSource;

    private string nextStageName;


    #endregion

    #region パネルの初期設定・ロックの状態更新

    private void Start()
    {
        // --- パネルの初期設定 ---
        if (setPanel != null)
            setPanel.SetActive(false);
        if (resetPanel != null)
            resetPanel.SetActive(false);
    }


    #endregion


    // ======== アプリ終了 =========
    public void OnApplicationQuit()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Application.Quit();
    }

    #region パネルの表示・非表示切り替え

    // ======== パネル表示 =========
    public void OnSetPanel()
    {
        if (setPanel == null) return;

        setPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        Debug.Log("音量設定パネル表示");
    }

    public void OnControlPanel()
    {
        if (resetPanel == null) return;

        resetPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        Debug.Log("データリセットパネル表示");
    }

    // ======== パネル非表示 =========
    public void OffSetPanel()
    {
        if (setPanel == null) return;

        setPanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Debug.Log("音量設定パネル非表示");
    }

    public void OffControlPanel()
    {
        if (resetPanel == null) return;

        resetPanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Debug.Log("データリセットパネル非表示");
    }

    #endregion

    #region デバッグ用アンロック

    private void Update()
    {

    }

    private void DebugResetStages()
    {
        // 初期状態：ClearedStage = 0 → ステージ1だけ解放
        PlayerPrefs.SetInt("ClearedStage", 0);
        PlayerPrefs.Save();

        Debug.Log("【DEBUG】ステージロックを初期状態に戻しました（ステージ1のみ解放）");
    }

    #endregion

}

