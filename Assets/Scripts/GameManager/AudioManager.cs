using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// 🎧 ゲーム全体の音量（BGM・SE）を統一的に管理するクラス。
/// ・AudioMixer経由でBGM/SEの音量を制御
/// ・スライダーUIとリアルタイム連動
/// ・音量設定をPlayerPrefsに保存し次回起動時に復元
/// ・SEを名前指定で再生可能
/// ・シーンをまたいでも破棄されない（シングルトン）
/// </summary>
public class AudioManager : MonoBehaviour
{
    //==============================
    // 🧭 シングルトン
    //==============================
    public static AudioManager Instance; // どこからでも呼び出せるようにする（例：AudioManager.Instance.PlaySE("Jump")）

    //==============================
    // 🎚 インスペクター設定項目
    //==============================
    [Header("Audio Mixer & Sliders")]
    [SerializeField] private AudioMixer audioMixer; // 実際の音量制御を行うAudioMixer（Expose Parameters利用）
    [SerializeField] private Slider bgmSlider;      // BGM音量スライダー（UI）
    [SerializeField] private Slider seSlider;       // SE音量スライダー（UI）

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // BGM再生専用AudioSource
    [SerializeField] private AudioSource seSource;  // SE再生専用AudioSource

    [Header("確認用SE")]
    [SerializeField] private AudioClip testSE; // スライダー操作時に鳴らす確認音（ボタン音など）

    [Header("SEクリップリスト（名前指定再生用）")]
    [SerializeField] private AudioClip[] seClips; // 名前指定で再生したいSEを登録（例：Jump, Hit, EnemyDead）

    //==============================
    // 🎯 定数定義（マジックナンバー完全排除）
    //==============================

    // PlayerPrefs用キー名（保存データ名）
    private const string PREF_KEY_BGM_VOLUME = "BGMVolume";
    private const string PREF_KEY_SE_VOLUME = "SEVolume";

    // AudioMixerのExpose Parameter名（AudioMixer内でExposeしておく必要あり）
    private const string MIXER_PARAM_BGM_VOLUME = "BGMVolume";
    private const string MIXER_PARAM_SE_VOLUME = "SEVolume";

    // 音量値設定関連
    private const float VOLUME_MIN = 0.0001f;    // 🔢 0はlog10で無限小になるため、これ以上は下げない
    private const float VOLUME_MAX = 1.0f;       // 🔢 音量スライダーの上限値
    private const float VOLUME_DEFAULT = 1.0f;   // 🔢 初期設定音量
    private const float DECIBEL_MULTIPLIER = 20f; // 📏 「音量倍率 → デシベル変換」に使う係数（20 * log10(x)）

    // スライダー確認音設定
    private const float TEST_SE_COOLDOWN = 0.2f; // ⏱ 短時間で何度も鳴らさないよう再生間隔を制限（秒）

    //==============================
    // 🔒 内部変数
    //==============================
    private Dictionary<string, AudioClip> seClipDict; // 名前→AudioClipの辞書（高速アクセス用）
    private float lastPlayTime = -9999f;              // 最後に確認SEを鳴らした時刻
    private bool isInitializing = true;               // 初期化中はスライダー変更イベントを無視

    //==============================
    // 🧱 公開プロパティ
    //==============================
    public AudioClip TestSE => testSE; // テスト用SEを外部から参照可能（読み取り専用）

    //==============================
    // 🎬 Awake（シングルトン生成）
    //==============================
    private void Awake()
    {
        // すでにインスタンスが存在する場合は自分を破棄（シングルトン維持）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄されないように設定
        }
        else
        {
            Destroy(gameObject); // 二重生成防止
            return;
        }
    }

    //==============================
    // 🎬 Start（初期化処理）
    //==============================
    private void Start()
    {
        // --- PlayerPrefsから音量を取得（なければ初期値を使う） ---
        float bgmVolume = PlayerPrefs.GetFloat(PREF_KEY_BGM_VOLUME, VOLUME_DEFAULT);
        float seVolume = PlayerPrefs.GetFloat(PREF_KEY_SE_VOLUME, VOLUME_DEFAULT);

        // --- AudioMixerに音量を反映 ---
        SetBGMVolume(bgmVolume);
        SetSEVolume(seVolume);

        // --- スライダーに反映（イベントを発火させない） ---
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(bgmVolume);
        if (seSlider != null) seSlider.SetValueWithoutNotify(seVolume);

        // --- SE辞書初期化 ---
        InitializeSEDictionary();

        // --- 初期化完了 ---
        isInitializing = false;
    }

    //==============================
    // 🧩 SE辞書の初期化
    //==============================
    private void InitializeSEDictionary()
    {
        seClipDict = new Dictionary<string, AudioClip>();

        foreach (var clip in seClips)
        {
            // nullチェック & 重複登録防止
            if (clip == null) continue;
            if (!seClipDict.ContainsKey(clip.name))
            {
                seClipDict.Add(clip.name, clip);
            }
        }
    }

    //==============================
    // 🎚 スライダー変更イベント
    //==============================

    /// <summary>
    /// BGMスライダー変更時に呼ばれる（UIイベント）
    /// </summary>
    public void OnBGMVolumeChanged(float value)
    {
        if (isInitializing) return; // 初期設定中は無視

        SetBGMVolume(value);
        PlayerPrefs.SetFloat(PREF_KEY_BGM_VOLUME, value); // 保存
    }

    /// <summary>
    /// SEスライダー変更時に呼ばれる（UIイベント）
    /// </summary>
    public void OnSEVolumeChanged(float value)
    {
        if (isInitializing) return;

        SetSEVolume(value);
        PlayerPrefs.SetFloat(PREF_KEY_SE_VOLUME, value); // 保存

        // 🎧 テスト音を一定間隔で再生（連打防止）
        if (testSE != null && Time.time - lastPlayTime > TEST_SE_COOLDOWN)
        {
            PlaySE(testSE);
            lastPlayTime = Time.time;
        }
    }

    //==============================
    // 🎚 Volume Setter（AudioMixerへ反映）
    //==============================

    /// <summary>
    /// BGM音量をAudioMixerに反映
    /// </summary>
    private void SetBGMVolume(float value)
    {
        float clamped = Mathf.Clamp(value, VOLUME_MIN, VOLUME_MAX);       // 範囲制限
        float decibel = Mathf.Log10(clamped) * DECIBEL_MULTIPLIER;       // 倍率をdBに変換
        audioMixer.SetFloat(MIXER_PARAM_BGM_VOLUME, decibel);            // Mixerに反映
    }

    /// <summary>
    /// SE音量をAudioMixerに反映
    /// </summary>
    private void SetSEVolume(float value)
    {
        float clamped = Mathf.Clamp(value, VOLUME_MIN, VOLUME_MAX);
        float decibel = Mathf.Log10(clamped) * DECIBEL_MULTIPLIER;
        audioMixer.SetFloat(MIXER_PARAM_SE_VOLUME, decibel);
    }

    //==============================
    // 🔊 SE再生（AudioClip指定）
    //==============================

    /// <summary>
    /// 直接AudioClipを指定して再生する
    /// </summary>
    public void PlaySE(AudioClip clip)
    {
        if (clip == null || seSource == null) return;
        seSource.PlayOneShot(clip);
    }

    //==============================
    // 🔊 SE再生（名前指定）
    //==============================

    /// <summary>
    /// AudioClipの名前で再生する（例：PlaySE("EnemyDead")）
    /// </summary>
    public void PlaySE(string clipName)
    {
        if (string.IsNullOrEmpty(clipName) || seClipDict == null) return;

        if (seClipDict.TryGetValue(clipName, out var clip))
        {
            PlaySE(clip);
        }
        else
        {
            Debug.LogWarning($"指定されたSE '{clipName}' はAudioManagerに登録されていません。");
        }
    }
}
