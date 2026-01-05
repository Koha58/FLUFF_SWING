using System;
using System.Collections.Generic;
using UnityEngine;

public enum Language { Japanese = 0, English = 1 }

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public event Action<Language> OnLanguageChanged;

    private const string PrefLangKey = "LANG";
    private const string PrefLangSelectedOnceKey = "LANG_SELECTED_ONCE";

    public Language CurrentLanguage { get; private set; }

    // 初回選択が終わっているか（＝次回から言語選択画面を出さない）
    public bool HasSelectedLanguageOnce => PlayerPrefs.GetInt(PrefLangSelectedOnceKey, 0) == 1;

    private readonly Dictionary<string, (string ja, string en)> _dict = new();

    private void Awake()
    {
        // Singleton
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // テーブル読み込み
        LoadTableFromResources();

        // 言語決定
        if (!HasSelectedLanguageOnce)
        {
            // ★初回：OS言語から仮決定（後でユーザーが選び直せる）
            CurrentLanguage = DetectDefaultLanguageFromOS();
            // ここでは「確定」ではないので、保存は必須ではない（保存したいならしてもOK）
        }
        else
        {
            // ★2回目以降：保存値を使う
            CurrentLanguage = (Language)PlayerPrefs.GetInt(PrefLangKey, (int)Language.Japanese);
        }

        // 起動直後に反映
        OnLanguageChanged?.Invoke(CurrentLanguage);
    }

    private void LoadTableFromResources()
    {
        var table = Resources.Load<LocalizationTableSO>("Localization/LocalizationTable");
        if (table == null)
        {
            Debug.LogError("LocalizationTableSO が Resources/Localization/LocalizationTable に見つかりません");
            return;
        }

        _dict.Clear();

        var seen = new HashSet<string>();
        foreach (var e in table.entries)
        {
            if (string.IsNullOrEmpty(e.key)) continue;

            if (!seen.Add(e.key))
                Debug.LogWarning($"⚠ key 重複: {e.key}（後勝ちで上書きされます）");

            _dict[e.key] = (e.ja ?? "", e.en ?? "");
        }
    }

    /// <summary>
    /// OS言語を見て初期言語を決める
    /// </summary>
    private Language DetectDefaultLanguageFromOS()
    {
        return Application.systemLanguage == SystemLanguage.Japanese
            ? Language.Japanese
            : Language.English;
    }

    /// <summary>
    /// 言語をセット（選択画面・設定画面などから呼ぶ）
    /// forceNotify=true なら「同じ言語でもイベントを飛ばす」（初回確定UIで便利）
    /// </summary>
    public void SetLanguage(Language lang, bool forceNotify = false)
    {
        bool changed = CurrentLanguage != lang;

        if (!changed && !forceNotify) return;

        CurrentLanguage = lang;

        // 「保存」もここで統一してしまうのが分かりやすい
        PlayerPrefs.SetInt(PrefLangKey, (int)lang);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke(CurrentLanguage);
    }

    /// <summary>
    /// ★初回の言語選択が完了したフラグを立てる（Confirm/OKボタンで呼ぶ）
    /// </summary>
    public void MarkLanguageSelectedOnce()
    {
        PlayerPrefs.SetInt(PrefLangSelectedOnceKey, 1);
        PlayerPrefs.Save();
    }

    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";

        if (_dict.TryGetValue(key, out var v))
        {
            // enが空ならjaにフォールバック（未翻訳でも崩れない）
            if (CurrentLanguage == Language.English && string.IsNullOrEmpty(v.en))
                return v.ja;

            return CurrentLanguage == Language.Japanese ? v.ja : v.en;
        }

        return $"[{key}]";
    }
}
