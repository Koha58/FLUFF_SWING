using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// 🎧 ゲーム全体の音量（BGM・SE）を統一的に管理するマネージャークラス。
/// - AudioMixerを介して実際の音量を制御
/// - スライダーUIと連動して音量を変更・保存
/// - ゲーム間で音量設定を保持（DontDestroyOnLoad）
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>グローバルにアクセス可能なシングルトンインスタンス</summary>
    public static AudioManager Instance;

    [Header("Audio Mixer & Sliders")]
    [SerializeField] private AudioMixer audioMixer; // 🎚 実際の音量制御に使うミキサー
    [SerializeField] private Slider bgmSlider;      // 🎵 BGM音量用スライダー
    [SerializeField] private Slider seSlider;       // 🔊 SE音量用スライダー

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // BGM再生用
    [SerializeField] private AudioSource seSource;  // SE再生用

    [Header("確認用SE")]
    [SerializeField] private AudioClip testSE; // 🎵 スライダー操作時に流す確認用SE

    // 🔑 PlayerPrefs用の保存キー
    private const string BGM_KEY = "BGMVolume";
    private const string SE_KEY = "SEVolume";

    private float lastPlayTime;
    private const float playCooldown = 0.2f; // 0.2秒間隔で制限
    private bool isInitializing = true; // 🟡 初期化中フラグ

    public AudioClip TestSE => testSE;

    /// <summary>
    /// 初期化処理（シングルトン構築のみ）。
    /// 音量設定の反映は AudioMixer の初期化が終わる Start() で行う。
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも破棄されないようにする
        }
        else
        {
            Destroy(gameObject); // 二重生成を防ぐ
            return;
        }
    }

    /// <summary>
    /// 起動時に前回保存した音量設定を AudioMixer に適用。
    /// AudioMixer が Awake() 時点ではまだ初期化されていないため、
    /// Start() で行うことで「初回起動時に音がMAXになる問題」を防ぐ。
    /// </summary>
    private void Start()
    {
        // PlayerPrefsから前回保存した音量を取得（初回起動時は1.0f）
        float bgmVolume = PlayerPrefs.GetFloat(BGM_KEY, 1f);
        float seVolume = PlayerPrefs.GetFloat(SE_KEY, 1f);

        // 実際の音量をミキサーに反映
        SetBGMVolume(bgmVolume);
        SetSEVolume(seVolume);

        // 初期設定中はOnValueChangedを無視
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(bgmVolume);
        if (seSlider != null) seSlider.SetValueWithoutNotify(seVolume);

        // ✅ 初期化完了（これ以降はイベント反応OK）
        isInitializing = false;
    }

    /// <summary>
    /// 🎚 BGMスライダーが変更されたときに呼ばれる。
    /// ミキサーに反映し、PlayerPrefsに保存する。
    /// </summary>
    public void OnBGMVolumeChanged(float value)
    {
        if (isInitializing) return; // ← 初期化中なら無視

        SetBGMVolume(value);
        PlayerPrefs.SetFloat(BGM_KEY, value);
    }

    /// <summary>
    /// 🎚 SEスライダーが変更されたときに呼ばれる。
    /// ミキサーに反映し、PlayerPrefsに保存する。
    /// </summary>
    public void OnSEVolumeChanged(float value)
    {
        if (isInitializing) return; // ← 初期化中なら無視

        SetSEVolume(value);
        PlayerPrefs.SetFloat(SE_KEY, value);

        // 🔊 確認用SEを流す
        if (testSE != null && Time.time - lastPlayTime > playCooldown)
        {
            PlaySE(testSE);
            lastPlayTime = Time.time;
        }
    }

    /// <summary>
    /// BGM音量をAudioMixerに反映。
    /// スライダー値（0〜1）をdB値（-80〜0）に変換して設定。
    /// </summary>
    private void SetBGMVolume(float value)
    {
        float clamped = Mathf.Clamp(value, 0.0001f, 1f); // log10(0)防止
        float dB = Mathf.Log10(clamped) * 20f;           // 対数変換で音量カーブを自然に
        audioMixer.SetFloat("BGMVolume", dB);
    }

    /// <summary>
    /// SE音量をAudioMixerに反映。
    /// スライダー値（0〜1）をdB値（-80〜0）に変換して設定。
    /// </summary>
    private void SetSEVolume(float value)
    {
        float clamped = Mathf.Clamp(value, 0.0001f, 1f);
        float dB = Mathf.Log10(clamped) * 20f;
        audioMixer.SetFloat("SEVolume", dB);
    }

    /// <summary>
    /// 🎵 SEを一度だけ再生する。
    /// AudioSourceの音量はAudioMixerの「SEVolume」パラメータに従う。
    /// </summary>
    public void PlaySE(AudioClip clip)
    {
        if (clip == null || seSource == null) return;
        seSource.PlayOneShot(clip);
    }
}
