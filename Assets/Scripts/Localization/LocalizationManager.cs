using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 対応言語の定義
/// ※値を保存するため enum の並び順は安易に変えないこと
/// </summary>
public enum Language
{
    Japanese = 0,
    English = 1
}

/// <summary>
/// ローカライズ管理クラス
/// ・言語の決定／保存
/// ・テキストテーブルの読み込み
/// ・言語変更イベントの通知
/// を一元管理する Singleton
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static LocalizationManager Instance { get; private set; }

    /// <summary>
    /// 言語が変更されたときに通知されるイベント
    /// UI や各種表示クラスが購読する
    /// </summary>
    public event Action<Language> OnLanguageChanged;

    // PlayerPrefs に保存するキー
    private const string PrefLangKey = "LANG";
    private const string PrefLangSelectedOnceKey = "LANG_SELECTED_ONCE";

    /// <summary>
    /// 現在の言語
    /// </summary>
    public Language CurrentLanguage { get; private set; }

    /// <summary>
    /// 初回言語選択が完了しているかどうか
    /// true の場合、次回起動時に言語選択画面をスキップできる
    /// </summary>
    public bool HasSelectedLanguageOnce =>
        PlayerPrefs.GetInt(PrefLangSelectedOnceKey, 0) == 1;

    /// <summary>
    /// ローカライズ文字列テーブル
    /// key → (日本語, 英語)
    /// </summary>
    private readonly Dictionary<string, (string ja, string en)> _dict = new();

    /// <summary>
    /// 起動時初期化
    /// </summary>
    private void Awake()
    {
        // ===== Singleton 処理 =====
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ===== ローカライズテーブル読み込み =====
        LoadTableFromResources();

        // ===== 言語決定処理 =====
        if (!HasSelectedLanguageOnce)
        {
            // ★ 初回起動時
            // OS言語を見て仮の言語を決定する
            // （この時点では「確定」ではなく、ユーザーが後で選び直す前提）
            CurrentLanguage = DetectDefaultLanguageFromOS();
        }
        else
        {
            // ★ 2回目以降の起動
            // 保存されている言語設定を使用
            CurrentLanguage = (Language)PlayerPrefs.GetInt(
                PrefLangKey,
                (int)Language.Japanese
            );
        }

        // 起動直後に現在言語を通知
        OnLanguageChanged?.Invoke(CurrentLanguage);
    }

    /// <summary>
    /// Resources から LocalizationTableSO を読み込み、
    /// 内部辞書に展開する
    /// </summary>
    private void LoadTableFromResources()
    {
        var table = Resources.Load<LocalizationTableSO>(
            "Localization/LocalizationTable"
        );

        if (table == null)
        {
            Debug.LogError(
                "LocalizationTableSO が Resources/Localization/LocalizationTable に見つかりません"
            );
            return;
        }

        _dict.Clear();

        // 重複キー検出用
        var seen = new HashSet<string>();

        foreach (var e in table.entries)
        {
            if (string.IsNullOrEmpty(e.key)) continue;

            // キー重複チェック（後勝ちで上書き）
            if (!seen.Add(e.key))
                Debug.LogWarning($"⚠ key 重複: {e.key}（後勝ちで上書きされます）");

            _dict[e.key] = (e.ja ?? "", e.en ?? "");
        }
    }

    /// <summary>
    /// OS の言語設定から初期言語を推定する
    /// </summary>
    private Language DetectDefaultLanguageFromOS()
    {
        return Application.systemLanguage == SystemLanguage.Japanese
            ? Language.Japanese
            : Language.English;
    }

    /// <summary>
    /// 言語を設定する
    /// 設定画面・言語選択画面などから呼ばれる
    /// 
    /// forceNotify = true の場合、
    /// 同じ言語でも OnLanguageChanged を発火する
    /// （初回確定UIなどで便利）
    /// </summary>
    public void SetLanguage(Language lang, bool forceNotify = false)
    {
        bool changed = CurrentLanguage != lang;

        // 言語が変わらず、強制通知もしない場合は何もしない
        if (!changed && !forceNotify) return;

        CurrentLanguage = lang;

        // 言語設定を保存
        PlayerPrefs.SetInt(PrefLangKey, (int)lang);
        PlayerPrefs.Save();

        // 言語変更通知
        OnLanguageChanged?.Invoke(CurrentLanguage);
    }

    /// <summary>
    /// ★ 初回の言語選択が完了したことを記録する
    /// Confirm / OK ボタンなどから呼ぶ
    /// </summary>
    public void MarkLanguageSelectedOnce()
    {
        PlayerPrefs.SetInt(PrefLangSelectedOnceKey, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ローカライズ文字列を取得する
    /// </summary>
    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";

        if (_dict.TryGetValue(key, out var v))
        {
            // 英語が未翻訳の場合は日本語へフォールバック
            if (CurrentLanguage == Language.English && string.IsNullOrEmpty(v.en))
                return v.ja;

            return CurrentLanguage == Language.Japanese ? v.ja : v.en;
        }

        // キーが見つからない場合は分かりやすく表示
        return $"[{key}]";
    }

    /// <summary>
    /// 言語選択シーンスキップ用(開発環境のみ使用)
    /// </summary>
    public void ResetFirstLaunch()
    {
        // PlayerPrefs 削除後に再評価
        // これにより HasSelectedLanguageOnce が正しく false になる
        CurrentLanguage = DetectDefaultLanguageFromOS();
    }

}
