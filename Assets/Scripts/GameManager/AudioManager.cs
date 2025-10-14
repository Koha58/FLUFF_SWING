using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// 全シーンで共有するオーディオ管理クラス（BGM/SE の音量管理）。
/// ・スライダーは 0〜1 の線形値を受け取り、AudioMixer に渡すときに dB（対数）に変換します。
/// ・設定は PlayerPrefs に保存して永続化します。
/// ・シーンをまたいで破棄されないように DontDestroyOnLoad を使用します。
/// </summary>
public class AudioManager : MonoBehaviour
{
    // シングルトン参照（簡易的な実装）
    public static AudioManager Instance;

    // インスペクターで割り当てる：
    // - AudioMixer: BGM/SE のグループを持つ Mixer（Exposed Parameter を作成しておく）
    // - bgmSlider / seSlider: Unity UI (UGUI) の Slider（0〜1）
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    // PlayerPrefs に保存するキー名
    private const string BGM_KEY = "BGMVolume";
    private const string SE_KEY = "SEVolume";

    private void Awake()
    {
        // --- シングルトンと DontDestroyOnLoad の設定 ---
        // 最初のインスタンスだけを残し、それ以外は破棄します。
        // これにより、オーディオ設定を保持したままシーン移動できます。
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでオブジェクトを維持
        }
        else
        {
            Destroy(gameObject); // 既に存在する場合は重複を破棄
            return;
        }

        // --- 保存された音量の読み込み（存在しなければ 1.0 を使用） ---
        // PlayerPrefs には線形 0〜1 の値を保存しておく（扱いやすい）
        float bgmVolume = PlayerPrefs.GetFloat(BGM_KEY, 1f); // デフォルトはフル音量
        float seVolume = PlayerPrefs.GetFloat(SE_KEY, 1f);

        // Mixer に適用（内部で dB に変換するメソッドを使う）
        SetBGMVolume(bgmVolume);
        SetSEVolume(seVolume);

        // スライダーがシーン上にある場合は、UI を読み込んだ値で初期化しておく
        // （スライダーが別シーンの UI の場合は、そのシーンに入ったときに値を反映する処理が必要）
        if (bgmSlider != null)
            bgmSlider.value = bgmVolume;
        if (seSlider != null)
            seSlider.value = seVolume;

        // ※ UGUI の Slider を使っている場合は、Inspector の OnValueChanged に
        //    AudioManager.OnBGMVolumeChanged/OnSEVolumeChanged を割り当ててください。
        //    UI Toolkit を使っているならコード上でイベント登録が必要です。
    }

    /// <summary>
    /// スライダー（UI）から呼ばれる public メソッド（Inspector に出るよう public にする）
    /// Slider の OnValueChanged(float) に対応するシグネチャにする必要があります。
    /// </summary>
    /// <param name="value">線形の音量（0〜1）</param>
    public void OnBGMVolumeChanged(float value)
    {
        // 実際の適用と保存を分けているのでテストしやすく、再利用性が上がる
        SetBGMVolume(value);
        PlayerPrefs.SetFloat(BGM_KEY, value); // 即保存（必要に応じて OnApplicationQuit でまとめて保存しても良い）
    }

    /// <summary>
    /// SE 用のスライダーイベント
    /// </summary>
    /// <param name="value">線形の音量（0〜1）</param>
    public void OnSEVolumeChanged(float value)
    {
        SetSEVolume(value);
        PlayerPrefs.SetFloat(SE_KEY, value);
    }

    /// <summary>
    /// 線形 0〜1 の値を dB に変換して AudioMixer にセットする（BGM）
    /// </summary>
    /// <param name="value">0〜1（線形）</param>
    private void SetBGMVolume(float value)
    {
        // 小さい値で Mathf.Log10 が -inf にならないよう下限を設ける
        // 0.0001 は -80dB 相当（ほぼ無音）なので十分小さい
        float clamped = Mathf.Clamp(value, 0.0001f, 1f);

        // dB 変換：
        // 20 * log10(linear) で線形振幅をデシベルに変換する（AudioMixer は dB を期待）
        float dB = Mathf.Log10(clamped) * 20f;

        // AudioMixer 上で Expose したパラメータ名（例："BGMVolume"）に適用する
        // ※ Expose 名は AudioMixer 側で設定しておくこと
        audioMixer.SetFloat("BGMVolume", dB);
    }

    /// <summary>
    /// 線形 0〜1 の値を dB に変換して AudioMixer にセットする（SE）
    /// </summary>
    /// <param name="value">0〜1（線形）</param>
    private void SetSEVolume(float value)
    {
        float clamped = Mathf.Clamp(value, 0.0001f, 1f);
        float dB = Mathf.Log10(clamped) * 20f;
        audioMixer.SetFloat("SEVolume", dB);
    }
}
