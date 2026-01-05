using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private TMP_Text _text;
    private bool _subscribed;
    private Coroutine _bindRoutine;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        Debug.Log($"[LocalizedText] Awake: {name}", this);
    }

    private void OnEnable()
    {
        // LocalizationManager がまだ居ない可能性があるので、待ってから購読する
        _bindRoutine = StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        // 待機コルーチン停止
        if (_bindRoutine != null)
        {
            StopCoroutine(_bindRoutine);
            _bindRoutine = null;
        }

        // 購読解除（購読していた場合のみ）
        Unsubscribe();
    }

    private IEnumerator BindWhenReady()
    {
        // 既に購読済みなら何もしない
        if (_subscribed) yield break;

        // LocalizationManager が生成されるまで待つ
        while (LocalizationManager.Instance == null)
            yield return null;

        // ここで初めて購読
        LocalizationManager.Instance.OnLanguageChanged += HandleChanged;
        _subscribed = true;

        // 初回反映（今の言語でテキストを更新）
        HandleChanged(LocalizationManager.Instance.CurrentLanguage);
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= HandleChanged;

        _subscribed = false;
    }

    private void HandleChanged(Language _)
    {
        if (string.IsNullOrEmpty(key)) return;

        // LocalizationManager が居る前提（BindWhenReadyで保証）
        _text.text = LocalizationManager.Instance.Get(key);
    }
}
