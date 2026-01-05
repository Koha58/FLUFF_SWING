using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using ExcelDataReader;

public class LocalizationExcelImporter : EditorWindow
{
    string excelPath = "Assets/Data/Localization.xlsx";
    string saveFolder = "Assets/Resources/Localization/";
    string assetName = "LocalizationTable";

    [MenuItem("Tools/Localization Excel Importer")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationExcelImporter>("Localization Excel Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Excel File Path (.xlsx)");
        excelPath = EditorGUILayout.TextField(excelPath);

        GUILayout.Label("Save Folder (inside Resources recommended)");
        saveFolder = EditorGUILayout.TextField(saveFolder);

        GUILayout.Label("Asset Name");
        assetName = EditorGUILayout.TextField(assetName);

        if (GUILayout.Button("Import Localization"))
        {
            ImportExcel();
        }
    }

    void ImportExcel()
    {
        if (!File.Exists(excelPath))
        {
            Debug.LogError($"Excelファイルが見つかりません: {excelPath}");
            return;
        }

        using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        // 1シート目を読む
        string sheetName = reader.Name.Trim();

        var entries = new List<LocalizationEntry>();
        bool isHeader = true;

        while (reader.Read())
        {
            if (isHeader) { isHeader = false; continue; }

            var keyObj = reader.GetValue(0);
            if (keyObj == null) continue;

            string key = keyObj.ToString().Trim();
            if (string.IsNullOrEmpty(key)) continue;

            string ja = SafeGetString(reader, 1);
            string en = SafeGetString(reader, 2);
            string note = SafeGetString(reader, 3);

            entries.Add(new LocalizationEntry
            {
                key = key,
                ja = ja,
                en = en,
                note = note
            });
        }

        if (entries.Count == 0)
        {
            Debug.LogWarning($"[{sheetName}] 有効なデータがありません。");
            return;
        }

        var table = ScriptableObject.CreateInstance<LocalizationTableSO>();
        table.entries = entries.ToArray();

        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);

        string path = Path.Combine(saveFolder, $"{assetName}.asset");

        // 既存があれば差し替え
        var existing = AssetDatabase.LoadAssetAtPath<LocalizationTableSO>(path);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.CreateAsset(table, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Localization 取込み完了: {path}（{entries.Count}件）");
    }

    static string SafeGetString(IExcelDataReader reader, int col)
    {
        if (reader.FieldCount <= col) return "";
        var v = reader.GetValue(col);
        return v == null ? "" : v.ToString();
    }
}
