using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// TMP_Text をローカライズ対応させるコンポーネント
/// 指定したキーを使って LocalizationManager から文字列を取得し、
/// 言語変更時に自動で更新する
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    // ローカライズテーブル参照用キー
    [SerializeField] private string key;

    // 対象となる TMP_Text
    private TMP_Text _text;

    // イベント購読済みかどうか
    private bool _subscribed;

    // LocalizationManager の生成を待つためのコルーチン
    private Coroutine _bindRoutine;

    /// <summary>
    /// 初期化
    /// TMP_Text をキャッシュする
    /// </summary>
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();

        // デバッグ用（シーン構築時の確認に便利）
        Debug.Log($"[LocalizedText] Awake: {name}", this);
    }

    /// <summary>
    /// オブジェクト有効化時に呼ばれる
    /// LocalizationManager がまだ存在しない可能性があるため、
    /// コルーチンで待ってから購読する
    /// </summary>
    private void OnEnable()
    {
        _bindRoutine = StartCoroutine(BindWhenReady());
    }

    /// <summary>
    /// オブジェクト無効化時に呼ばれる
    /// コルーチン停止とイベント購読解除を行う
    /// </summary>
    private void OnDisable()
    {
        // 待機中のコルーチンを停止
        if (_bindRoutine != null)
        {
            StopCoroutine(_bindRoutine);
            _bindRoutine = null;
        }

        // 言語変更イベントの購読解除
        Unsubscribe();
    }

    /// <summary>
    /// LocalizationManager が生成されるまで待ち、
    /// 生成後にイベント購読と初期反映を行う
    /// </summary>
    private IEnumerator BindWhenReady()
    {
        // すでに購読済みなら何もしない
        if (_subscribed) yield break;

        // LocalizationManager.Instance が生成されるまで待機
        while (LocalizationManager.Instance == null)
            yield return null;

        // 言語変更イベントを購読
        LocalizationManager.Instance.OnLanguageChanged += HandleChanged;
        _subscribed = true;

        // 現在の言語で即座にテキストを更新
        HandleChanged(LocalizationManager.Instance.CurrentLanguage);
    }

    /// <summary>
    /// 言語変更イベントの購読を解除する
    /// </summary>
    private void Unsubscribe()
    {
        if (!_subscribed) return;

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= HandleChanged;

        _subscribed = false;
    }

    /// <summary>
    /// 言語変更時に呼ばれ、テキストを更新する
    /// </summary>
    private void HandleChanged(Language _)
    {
        // キー未設定の場合は何もしない
        if (string.IsNullOrEmpty(key)) return;

        // BindWhenReady によって LocalizationManager の存在は保証されている
        _text.text = LocalizationManager.Instance.Get(key);
    }
}
