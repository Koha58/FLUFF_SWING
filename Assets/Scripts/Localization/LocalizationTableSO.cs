using System;
using UnityEngine;

/// <summary>
/// ローカライズ文字列テーブル用 ScriptableObject
/// 各言語のテキストをキーで管理するためのデータコンテナ
/// </summary>
[CreateAssetMenu(menuName = "Localization/Localization Table")]
public class LocalizationTableSO : ScriptableObject
{
    /// <summary>
    /// ローカライズエントリ一覧
    /// 1要素 = 1キー分の翻訳データ
    /// </summary>
    public LocalizationEntry[] entries;
}

/// <summary>
/// ローカライズ1エントリ分のデータ
/// </summary>
[Serializable]
public class LocalizationEntry
{
    /// <summary>
    /// 文字列を取得するためのキー
    /// 例: "TITLE_START", "OPTION_LANGUAGE"
    /// </summary>
    public string key;

    /// <summary>
    /// 日本語テキスト
    /// </summary>
    [TextArea]
    public string ja;

    /// <summary>
    /// 英語テキスト
    /// </summary>
    [TextArea]
    public string en;

    /// <summary>
    /// 開発用メモ欄（翻訳指示・文脈説明など）
    /// 実行時には使用されない
    /// </summary>
    public string note;
}
