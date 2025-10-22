// ステージセレクト>SceneManagerで使用

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SceneSelectScript : MonoBehaviour
{
    [Header("クリック音")]
    [Tooltip("ステージ選択・設定ボタンを押したときに鳴らす効果音")]
    public AudioClip onClickSE;
    [Tooltip("ホーム・閉じるボタンを押したときに鳴らす効果音")]
    public AudioClip offClickSE;

    [Tooltip("接続するAudioMixerのSEグループ（任意）")]
    public AudioMixerGroup seMixerGroup;

    private AudioSource audioSource;

    void Start()
    {
        // AudioSourceを自動追加または取得
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // AudioMixerGroupを設定（Inspectorから指定）
        if (seMixerGroup != null)
            audioSource.outputAudioMixerGroup = seMixerGroup;
    }

    public void SelectStage(String StageName)
    {
        OnClickSE();
        SceneManager.LoadScene(StageName);
    }

    private void OnClickSE()
    {
        if (onClickSE != null)
            audioSource.PlayOneShot(onClickSE);
        else
            Debug.LogWarning("クリック音が設定されていません。");
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }

    // 鍵がついているステージは選択不可にする
    // クリア時にクリア判定用の変数に加算していく
    // →数に応じて鍵を解除、選択可能にする

    // 音量設定ができるようにする
    // ボタン押下時のSE追加
}
