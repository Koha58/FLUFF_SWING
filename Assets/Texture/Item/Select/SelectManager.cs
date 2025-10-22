// ステージセレクト>SceneManagerで使用

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Security.Cryptography.X509Certificates;

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

    private void Start()
    {
        // --- パネルの初期設定 ---
        if (setPanel != null)
            setPanel.SetActive(false);

        //// --- AudioSource設定 ---
        //// AudioSourceを自動追加または取得
        //audioSource = GetComponent<AudioSource>();
        //if (audioSource == null)
        //    audioSource = gameObject.AddComponent<AudioSource>();

        //// AudioMixerGroupを設定（Inspectorから指定）
        //if (seMixerGroup != null)
        //    audioSource.outputAudioMixerGroup = seMixerGroup;
    }

    // ======== ステージ遷移 =========
    public void SelectStage(String StageName)
    {
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

    private void LoadStageAfterSE()
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

    //// ======== 効果音再生共通関数 =========
    //private void PlaySE(AudioClip clip)
    //{
    //    if (clip != null)
    //        audioSource.PlayOneShot(clip);
    //    else
    //        Debug.LogWarning("SEが設定されていません。");
    //}
}
