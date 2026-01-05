using System.Collections;
using TMPro;
using UnityEngine;

public class LanguageSelectorUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text currentLabel; // 「日本語 / English」

    [Header("SE")]
    [SerializeField] private AudioClip clickSE;     // 言語切替時のSE
    [SerializeField] private bool playSeOnNoChange = false; // 同じ言語を選んでも鳴らすか（基本false）

    private Coroutine _bindRoutine;

    private void OnEnable()
    {
        // StartupでLocalizationManagerが後から生成される可能性があるので待つ
        _bindRoutine = StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        if (_bindRoutine != null)
        {
            StopCoroutine(_bindRoutine);
            _bindRoutine = null;
        }

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnChanged;
    }

    private IEnumerator BindWhenReady()
    {
        // Instanceが生成されるまで待つ（1フレーム〜数フレーム程度の想定）
        while (LocalizationManager.Instance == null)
            yield return null;

        // 二重購読防止で一度外してから付ける
        LocalizationManager.Instance.OnLanguageChanged -= OnChanged;
        LocalizationManager.Instance.OnLanguageChanged += OnChanged;

        // 現在言語で即反映
        OnChanged(LocalizationManager.Instance.CurrentLanguage);
    }

    public void OnPrev() => ToggleLanguage();
    public void OnNext() => ToggleLanguage(); // 2択なら同じでOK

    private void ToggleLanguage()
    {
        if (LocalizationManager.Instance == null) return;

        var now = LocalizationManager.Instance.CurrentLanguage;
        var next = (now == Language.Japanese) ? Language.English : Language.Japanese;

        if (next != now)
        {
            LocalizationManager.Instance.SetLanguage(next);
            PlayClickSE();
        }
        else if (playSeOnNoChange)
        {
            PlayClickSE();
        }
    }

    private void PlayClickSE()
    {
        if (clickSE == null) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(clickSE);
        else
            AudioSource.PlayClipAtPoint(clickSE, Vector3.zero);
    }

    private void OnChanged(Language lang)
    {
        if (currentLabel != null)
            currentLabel.text = (lang == Language.Japanese) ? "日本語" : "English";
    }
}
