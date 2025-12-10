using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 🎧 ゲーム全体の音量（BGM・SE）を統一的に管理するクラス。
/// 
/// 【主な機能】
/// ・AudioMixer 経由で BGM / SE の音量を制御  
/// ・スライダー UI とリアルタイムで連動  
/// ・音量設定を PlayerPrefs に保存・復元  
/// ・SE を名前指定で再生可能  
/// ・シーンをまたいでも破棄されない（シングルトン）  
/// </summary>
public class AudioManager : MonoBehaviour
{
    //====================================================================
    // 🧭 シングルトン設定（唯一のインスタンスを保持）
    //====================================================================
    public static AudioManager Instance;

    //====================================================================
    // 🎚 インスペクター設定項目
    //====================================================================
    [Header("🎛 Audio Mixer & UIスライダー")]
    [SerializeField] private AudioMixer audioMixer; // 音量を制御する AudioMixer
    [SerializeField] private Slider bgmSlider;      // BGM 用スライダー
    [SerializeField] private Slider seSlider;       // SE 用スライダー

    [Header("🔊 オーディオソース")]
    [SerializeField] private AudioSource bgmSource; // BGM 再生用
    [SerializeField] private AudioSource seSource;  // SE 再生用

    [Header("🎵 確認用SE（音量調整テスト用）")]
    [SerializeField] private AudioClip testSE;      // スライダー操作時に鳴らす確認音

    [Header("📦 SEクリップリスト（名前指定再生用）")]
    [SerializeField] private AudioClip[] seClips;   // 登録済みの SE 一覧

    //====================================================================
    // ⚙ 定数定義
    //====================================================================
    private const string PREF_KEY_BGM_VOLUME = "BGMVolume";
    private const string PREF_KEY_SE_VOLUME = "SEVolume";

    private const string MIXER_PARAM_BGM_VOLUME = "BGMVolume";
    private const string MIXER_PARAM_SE_VOLUME = "SEVolume";

    private const float VOLUME_MIN = 0.0001f;  // 0だとlog10でエラーになるため最小値を設定
    private const float VOLUME_MAX = 1.0f;
    private const float VOLUME_DEFAULT = 1.0f;
    private const float DECIBEL_MULTIPLIER = 20f; // 線形値をデシベルに変換（20 * log10）

    private const float TEST_SE_COOLDOWN = 0.2f; // テストSEの再生間隔

    //====================================================================
    // 🔒 内部変数
    //====================================================================
    private Dictionary<string, AudioClip> seClipDict; // SE名 → AudioClip の辞書
    private float lastPlayTime = -9999f;              // テストSE再生のクールダウン管理
    private bool isInitializing = true;               // 起動時フラグ（イベント暴発防止）

    //====================================================================
    // 🧱 公開プロパティ
    //====================================================================
    public AudioClip TestSE => testSE;

    //====================================================================
    // 🎬 Awake（シングルトン生成・永続化）
    //====================================================================
    private void Awake()
    {
        // すでに別のAudioManagerが存在する場合は自分を破棄
        if (Instance == null)
        {
            Instance = this;

            // シーン切り替え時にも破棄されないようにする
            DontDestroyOnLoad(gameObject);

            // 新しいシーンが読み込まれたときにUIを再リンク
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        // 自分が唯一のインスタンスならイベント解除（メモリリーク防止）
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    //====================================================================
    // 🎬 Start（初期化処理）
    //====================================================================
    private void Start()
    {
        InitializeVolumes();      // PlayerPrefsから音量を復元
        InitializeSEDictionary(); // SE辞書を作成
        isInitializing = false;   // 初期化完了（スライダーイベントを有効化）
    }

    //====================================================================
    // 🧩 シーンロード時：スライダーの再リンク
    //====================================================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // PlayerPrefsから保存済み音量を取得
        float bgmVolume = PlayerPrefs.GetFloat(PREF_KEY_BGM_VOLUME, VOLUME_DEFAULT);
        float seVolume = PlayerPrefs.GetFloat(PREF_KEY_SE_VOLUME, VOLUME_DEFAULT);

        // --- BGMスライダー再リンク ---
        if (bgmSlider != null)
        {
            // 値を設定（イベントを発火させず）
            bgmSlider.SetValueWithoutNotify(bgmVolume);

            // イベントを一度クリアしてから再登録（重複防止）
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        // --- SEスライダー再リンク ---
        if (seSlider != null)
        {
            seSlider.SetValueWithoutNotify(seVolume);
            seSlider.onValueChanged.RemoveAllListeners();
            seSlider.onValueChanged.AddListener(OnSEVolumeChanged);
        }

        // --- シーン内のスライダーを自動で探す ---
        if (bgmSlider == null)
        {
            var foundBgmSlider = GameObject.Find("BGMSlider")?.GetComponent<Slider>();
            if (foundBgmSlider != null) BindBGMSlider(foundBgmSlider);
        }

        if (seSlider == null)
        {
            var foundSeSlider = GameObject.Find("SESlider")?.GetComponent<Slider>();
            if (foundSeSlider != null) BindSESlider(foundSeSlider);
        }
    }

    //====================================================================
    // 🎚 音量初期化（PlayerPrefs → AudioMixer）
    //====================================================================
    private void InitializeVolumes()
    {
        // 保存された音量値を取得（未保存ならデフォルト値）
        float bgmVolume = PlayerPrefs.GetFloat(PREF_KEY_BGM_VOLUME, VOLUME_DEFAULT);
        float seVolume = PlayerPrefs.GetFloat(PREF_KEY_SE_VOLUME, VOLUME_DEFAULT);

        // AudioMixerへ反映
        SetBGMVolume(bgmVolume);
        SetSEVolume(seVolume);

        // UIスライダーに反映（nullチェック付き）
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(bgmVolume);
        if (seSlider != null) seSlider.SetValueWithoutNotify(seVolume);
    }

    //====================================================================
    // 📦 SE辞書構築（名前指定で再生可能にする）
    //====================================================================
    private void InitializeSEDictionary()
    {
        seClipDict = new Dictionary<string, AudioClip>();

        // 登録済みSEを名前キーで格納
        foreach (var clip in seClips)
        {
            if (clip == null) continue; // nullクリップをスキップ

            if (!seClipDict.ContainsKey(clip.name))
            {
                seClipDict.Add(clip.name, clip);
            }
        }
    }

    //====================================================================
    // 🎚 スライダー変更イベント
    //====================================================================
    public void OnBGMVolumeChanged(float value)
    {
        // 初期化中に呼ばれた場合は無視（Start時にSetValueWithoutNotifyしてるため）
        if (isInitializing) return;

        // Mixerへ反映し、PlayerPrefsに保存
        SetBGMVolume(value);
        PlayerPrefs.SetFloat(PREF_KEY_BGM_VOLUME, value);
    }

    public void OnSEVolumeChanged(float value)
    {
        if (isInitializing) return;

        // Mixerへ反映し、PlayerPrefsに保存
        SetSEVolume(value);
        PlayerPrefs.SetFloat(PREF_KEY_SE_VOLUME, value);

        // 音量確認用に短い間隔でSEを再生
        if (testSE != null && Time.time - lastPlayTime > TEST_SE_COOLDOWN)
        {
            PlaySE(testSE);
            lastPlayTime = Time.time;
        }
    }

    //====================================================================
    // 🔗 外部UIからスライダーを登録
    //====================================================================
    public void BindBGMSlider(Slider slider)
    {
        bgmSlider = slider;

        // 保存済み音量を反映
        float value = PlayerPrefs.GetFloat(PREF_KEY_BGM_VOLUME, VOLUME_DEFAULT);
        slider.SetValueWithoutNotify(value);

        // イベント再登録
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(OnBGMVolumeChanged);
    }

    public void BindSESlider(Slider slider)
    {
        seSlider = slider;

        float value = PlayerPrefs.GetFloat(PREF_KEY_SE_VOLUME, VOLUME_DEFAULT);
        slider.SetValueWithoutNotify(value);

        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(OnSEVolumeChanged);
    }

    //====================================================================
    // 🎚 AudioMixer音量設定
    //====================================================================
    private void SetBGMVolume(float value)
    {
        // 値を安全範囲にクランプ
        float clamped = Mathf.Clamp(value, VOLUME_MIN, VOLUME_MAX);

        // 線形値をデシベルに変換（AudioMixerはdB値を取る）
        float decibel = Mathf.Log10(clamped) * DECIBEL_MULTIPLIER;

        // AudioMixerへ反映
        audioMixer.SetFloat(MIXER_PARAM_BGM_VOLUME, decibel);
    }

    private void SetSEVolume(float value)
    {
        float clamped = Mathf.Clamp(value, VOLUME_MIN, VOLUME_MAX);
        float decibel = Mathf.Log10(clamped) * DECIBEL_MULTIPLIER;
        audioMixer.SetFloat(MIXER_PARAM_SE_VOLUME, decibel);
    }

    //====================================================================
    // 🔊 BGM再生制御
    //====================================================================

    /// <summary>
    /// 新しいBGMをセットし、再生を開始します。
    /// </summary>
    /// <param name="newClip">再生する新しいBGMクリップ。nullの場合は現在のBGMを停止します。</param>
    public void PlayBGM(AudioClip newClip)
    {
        if (bgmSource == null) return;

        if (bgmSource.clip == newClip && bgmSource.isPlaying)
        {
            // 現在と同じクリップが再生中なら何もしない
            return;
        }

        if (newClip == null)
        {
            // nullが渡されたら停止
            bgmSource.Stop();
            bgmSource.clip = null;
            return;
        }

        // 新しいクリップに切り替えて再生
        bgmSource.clip = newClip;
        bgmSource.Play();
         bgmSource.loop = true; 
    }

    //====================================================================
    // 🔊 SE再生関数
    //====================================================================

    /// <summary>
    /// AudioClipを直接指定して、カスタム音量で再生。
    /// PlayOneShotを使用して同時再生も可能。
    /// </summary>
    /// <param name="clip">再生するオーディオクリップ。</param>
    /// <param name="volume">再生するカスタム音量（0.0～1.0）。AudioMixerの設定音量と掛け合わせられる。</param>
    public void PlaySE(AudioClip clip, float volume)
    {
        if (clip == null || seSource == null) return;

        // 距離で減衰された音量をPlayOneShotに渡す。
        // 最終的な音量は、volume * seSource.volume (Mixer設定値) となる。
        seSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// AudioClipを直接指定して、標準音量 (1.0f) で再生。
    /// </summary>
    public void PlaySE(AudioClip clip)
    {
        // カスタム音量 1.0f で新しいオーバーロードを呼び出す
        PlaySE(clip, 1.0f);
    }

    /// <summary>
    /// クリップ名を指定して、カスタム音量で再生。
    /// </summary>
    /// <param name="clipName">登録されているクリップ名。</param>
    /// <param name="volume">再生するカスタム音量（0.0～1.0）。</param>
    public void PlaySE(string clipName, float volume)
    {
        if (string.IsNullOrEmpty(clipName) || seClipDict == null) return;

        if (seClipDict.TryGetValue(clipName, out var clip))
        {
            PlaySE(clip, volume);
        }
        else
        {
            Debug.LogWarning($"指定されたSE '{clipName}' はAudioManagerに登録されていません。");
        }
    }

    /// <summary>
    /// クリップ名を指定して、標準音量 (1.0f) で再生（辞書検索）。
    /// 名前が登録されていない場合は警告を出す。
    /// </summary>
    public void PlaySE(string clipName)
    {
        // 標準音量 1.0f でカスタム音量オーバーロードを呼び出す
        PlaySE(clipName, 1.0f);
    }
}
