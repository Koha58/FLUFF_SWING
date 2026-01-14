using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using ExcelDataReader;

/// <summary>
/// Excelからローカライズデータをインポートするためのエディタウィンドウ
/// </summary>
public class LocalizationExcelImporter : EditorWindow
{
    // デフォルトの設定パス
    string excelPath = "Assets/Data/Localization.xlsx";
    string saveFolder = "Assets/Resources/Localization/";
    string assetName = "LocalizationTable";

    /// <summary>
    /// Unityメニューの「Tools > Localization Excel Importer」からウィンドウを開く
    /// </summary>
    [MenuItem("Tools/Localization Excel Importer")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationExcelImporter>("Localization Excel Importer");
    }

    /// <summary>
    /// エディタウィンドウのGUI描画
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Excel File Path (.xlsx)", EditorStyles.boldLabel);
        excelPath = EditorGUILayout.TextField(excelPath);

        GUILayout.Label("Save Folder (inside Resources recommended)", EditorStyles.boldLabel);
        saveFolder = EditorGUILayout.TextField(saveFolder);

        GUILayout.Label("Asset Name", EditorStyles.boldLabel);
        assetName = EditorGUILayout.TextField(assetName);

        EditorGUILayout.Space(10);

        // インポート実行ボタン
        if (GUILayout.Button("Import Localization", GUILayout.Height(30)))
        {
            ImportExcel();
        }
    }

    /// <summary>
    /// Excelファイルを読み込み、ScriptableObjectを生成する
    /// </summary>
    void ImportExcel()
    {
        // ファイルの存在チェック
        if (!File.Exists(excelPath))
        {
            Debug.LogError($"Excelファイルが見つかりません: {excelPath}");
            return;
        }

        // ExcelDataReaderを使用してファイルを開く
        using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        // 1枚目のシート名を取得
        string sheetName = reader.Name.Trim();

        var entries = new List<LocalizationEntry>();
        bool isHeader = true;

        // 1行ずつ読み込み
        while (reader.Read())
        {
            // 最初の1行目はヘッダー（列名）としてスキップ
            if (isHeader) { isHeader = false; continue; }

            // A列（Index 0）をキーとして取得
            var keyObj = reader.GetValue(0);
            if (keyObj == null) continue;

            string key = keyObj.ToString().Trim();
            if (string.IsNullOrEmpty(key)) continue;

            // 各列のデータを取得（Index 1:日本語, 2:英語, 3:備考）
            string ja = SafeGetString(reader, 1);
            string en = SafeGetString(reader, 2);
            string note = SafeGetString(reader, 3);

            // リストに追加
            entries.Add(new LocalizationEntry
            {
                key = key,
                ja = ja,
                en = en,
                note = note
            });
        }

        // データが空の場合の警告
        if (entries.Count == 0)
        {
            Debug.LogWarning($"[{sheetName}] 有効なデータがありません。");
            return;
        }

        // ScriptableObjectのインスタンスを作成し、データを流し込む
        var table = ScriptableObject.CreateInstance<LocalizationTableSO>();
        table.entries = entries.ToArray();

        // 保存先ディレクトリがなければ作成
        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);

        // 保存パスの構築
        string path = Path.Combine(saveFolder, $"{assetName}.asset");

        // 同名の既存アセットがある場合は一度削除（上書きを確実にするため）
        var existing = AssetDatabase.LoadAssetAtPath<LocalizationTableSO>(path);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        // アセットとして保存
        AssetDatabase.CreateAsset(table, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Localization 取込み完了: {path}（{entries.Count}件）");
    }

    /// <summary>
    /// 指定した列の値を安全に文字列として取得する
    /// </summary>
    static string SafeGetString(IExcelDataReader reader, int col)
    {
        // 列の範囲外アクセス防止
        if (reader.FieldCount <= col) return "";
        var v = reader.GetValue(col);
        // nullなら空文字、値があれば文字列に変換して返す
        return v == null ? "" : v.ToString();
    }
}