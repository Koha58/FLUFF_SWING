using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 言語選択UIの表示と入力を管理するクラス
/// LocalizationManager と連携し、言語切替・表示更新・SE再生を担当する
/// </summary>
public class LanguageSelectorUI : MonoBehaviour
{
    [Header("UI")]
    // 現在選択されている言語を表示するラベル（例：「日本語 / English」）
    [SerializeField] private TMP_Text currentLabel;

    [Header("SE")]
    // 言語切替時に鳴らすクリックSE
    [SerializeField] private AudioClip clickSE;

    // 同じ言語を選んだ場合でもSEを鳴らすかどうか（基本は false）
    [SerializeField] private bool playSeOnNoChange = false;

    // LocalizationManager の生成を待つためのコルーチン参照
    private Coroutine _bindRoutine;

    /// <summary>
    /// オブジェクト有効化時に呼ばれる
    /// LocalizationManager がまだ生成されていない可能性があるため、
    /// コルーチンでバインドを遅延する
    /// </summary>
    private void OnEnable()
    {
        _bindRoutine = StartCoroutine(BindWhenReady());
    }

    /// <summary>
    /// オブジェクト無効化時に呼ばれる
    /// イベント購読とコルーチンを確実に解除する
    /// </summary>
    private void OnDisable()
    {
        // 待機中のコルーチンを停止
        if (_bindRoutine != null)
        {
            StopCoroutine(_bindRoutine);
            _bindRoutine = null;
        }

        // LocalizationManager が存在する場合のみイベント購読解除
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnChanged;
    }

    /// <summary>
    /// LocalizationManager.Instance が生成されるまで待ち、
    /// イベント購読と初期表示を行う
    /// </summary>
    private IEnumerator BindWhenReady()
    {
        // Instance が生成されるまで待機
        // Startup 経由で遅れて生成されるケースを考慮
        while (LocalizationManager.Instance == null)
            yield return null;

        // 二重購読防止のため一度解除してから購読
        LocalizationManager.Instance.OnLanguageChanged -= OnChanged;
        LocalizationManager.Instance.OnLanguageChanged += OnChanged;

        // 現在の言語を即座にUIへ反映
        OnChanged(LocalizationManager.Instance.CurrentLanguage);
    }

    /// <summary>
    /// 「前へ」ボタン用（2択構成のため Next と同じ処理）
    /// </summary>
    public void OnPrev() => ToggleLanguage();

    /// <summary>
    /// 「次へ」ボタン用（2択構成のため Prev と同じ処理）
    /// </summary>
    public void OnNext() => ToggleLanguage();

    /// <summary>
    /// 現在の言語からもう一方の言語へ切り替える
    /// </summary>
    private void ToggleLanguage()
    {
        // LocalizationManager が未生成の場合は何もしない
        if (LocalizationManager.Instance == null) return;

        var now = LocalizationManager.Instance.CurrentLanguage;
        var next = (now == Language.Japanese) ? Language.English : Language.Japanese;

        // 実際に言語が変わる場合
        if (next != now)
        {
            LocalizationManager.Instance.SetLanguage(next);
            PlayClickSE();
        }
        // 同じ言語だった場合でも SE を鳴らしたい設定の場合
        else if (playSeOnNoChange)
        {
            PlayClickSE();
        }
    }

    /// <summary>
    /// 言語切替時のクリックSEを再生する
    /// </summary>
    private void PlayClickSE()
    {
        if (clickSE == null) return;

        // AudioManager があればそちらを使用
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(clickSE);
        // フォールバック（AudioManager 不在時）
        else
            AudioSource.PlayClipAtPoint(clickSE, Vector3.zero);
    }

    /// <summary>
    /// 言語変更イベント時に呼ばれ、表示ラベルを更新する
    /// </summary>
    private void OnChanged(Language lang)
    {
        if (currentLabel == null) return;

        // 選択中の言語名を表示
        currentLabel.text = (lang == Language.Japanese) ? "日本語" : "English";
    }
}
