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
    [Header("移動操作画面パネル")]
    [SerializeField] private GameObject MoveinfoPanel;
    [Header("ワイヤー操作画面パネル")]
    [SerializeField] private GameObject WireinfoPanel;
    [Header("攻撃操作画面パネル")]
    [SerializeField] private GameObject AttackinfoPanel;

    private GameObject currentInfoPanel;

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
        if (setPanel != null)
            setPanel.SetActive(false);

        if (MoveinfoPanel != null)
            MoveinfoPanel.SetActive(false);
        if (WireinfoPanel != null)
            WireinfoPanel.SetActive(false);
        if (AttackinfoPanel != null)
            AttackinfoPanel.SetActive(false);

        currentInfoPanel = null;
    }



    #endregion

    private void ShowInfoPanel(GameObject panel)
    {
        if (panel == null) return;

        // すでに別のパネルが表示されていれば閉じる
        if (currentInfoPanel != null && currentInfoPanel != panel)
        {
            currentInfoPanel.SetActive(false);
        }

        // 指定パネルを表示
        currentInfoPanel = panel;
        currentInfoPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);
    }



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

    public void OnInfoMovePanel()
    {
        ShowInfoPanel(MoveinfoPanel);
        Debug.Log("操作方法移動パネル表示");
    }

    public void OnInfoWirePanel()
    {
        ShowInfoPanel(WireinfoPanel);
        Debug.Log("操作方法ワイヤーパネル表示");
    }

    public void OnInfoAttackPanel()
    {
        ShowInfoPanel(AttackinfoPanel);
        Debug.Log("操作方法攻撃パネル表示");
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

    public void OffInfoPanel()
    {
        // 表示中の操作パネルがなければ何もしない
        if (currentInfoPanel == null) return;

        currentInfoPanel.SetActive(false);
        currentInfoPanel = null;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Debug.Log("操作方法パネル非表示");
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

