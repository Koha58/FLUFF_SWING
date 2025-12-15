using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using ExcelDataReader; // このライブラリをプロジェクトにインポートしてください

/// <summary>
/// Excelファイルからエンドロールデータを読み込み、UnityのCreditDataSOとして生成する
/// エディタ拡張ウィンドウ。
/// </summary>
public class CreditDataExcelImporter : EditorWindow
{
    // Excelファイルのパス
    string excelPath = "Assets/Data/CreditData.xlsx";
    // ScriptableObjectを保存するフォルダ（Resources内推奨）
    string saveFolder = "Assets/Resources/CreditData/";

    // メニューからエディタウィンドウを開くためのメソッド
    [MenuItem("Tools/CreditData Excel Importer")]
    public static void ShowWindow()
    {
        GetWindow<CreditDataExcelImporter>("CreditData Excel Importer");
    }

    // エディタウィンドウのGUI描画
    void OnGUI()
    {
        GUILayout.Label("Excel File Path (.xlsx)");
        excelPath = EditorGUILayout.TextField(excelPath);

        GUILayout.Label("Save Folder (inside Resources)");
        saveFolder = EditorGUILayout.TextField(saveFolder);

        if (GUILayout.Button("Import Credit Data"))
        {
            ImportExcel();
        }
    }

    // Excelファイルを読み込み、各シートをScriptableObjectとして生成
    void ImportExcel()
    {
        if (!File.Exists(excelPath))
        {
            Debug.LogError($"Excelファイルが見つかりません: {excelPath}");
            return;
        }

        try
        {
            using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            do
            {
                string sheetName = reader.Name.Trim();
                List<CreditEntry> entries = new List<CreditEntry>();
                bool isHeader = true;

                while (reader.Read())
                {
                    if (isHeader)
                    {
                        isHeader = false;
                        continue; // ヘッダー行をスキップ
                    }

                    // 必要な列数（Type, Title, Name, SpecialData）は最低3列
                    if (reader.FieldCount < 3) continue;

                    try
                    {
                        // --- データ取得のヘルパー関数 ---
                        // セルが空でも、数値が入っていても安全に文字列を取得する
                        string GetSafeString(int index)
                        {
                            // インデックスがフィールド数を超えているか、DBNull（空セル）かチェック
                            if (index >= reader.FieldCount || reader.IsDBNull(index))
                            {
                                return string.Empty; // 空の場合は空文字列を返す
                            }
                            // GetValue()で値を取得し、ToString()で文字列に変換（Null参照を避けるために ?. を使用）
                            object value = reader.GetValue(index);
                            return value?.ToString().Trim() ?? string.Empty;
                        }
                        // ------------------------------------

                        CreditEntry entry = new CreditEntry();

                        // A列 (Type)
                        entry.type = GetSafeString(0);

                        // B列 (Title)
                        entry.title = GetSafeString(1);

                        // C列 (Name)
                        entry.name = GetSafeString(2);

                        // D列 (SpecialData)
                        entry.specialData = GetSafeString(3);

                        // Typeが空の場合はスキップ
                        if (string.IsNullOrEmpty(entry.type)) continue;

                        entries.Add(entry);
                    }
                    catch (System.Exception e)
                    {
                        // データ解析に失敗した行を警告
                        Debug.LogWarning($"[{sheetName}] 行 {reader.Depth + 1} でデータ解析に失敗しました: {e.GetType().Name} - {e.Message}");
                    }
                }

                if (entries.Count == 0) continue;

                // ScriptableObjectを作成し、データを格納
                var dataSO = ScriptableObject.CreateInstance<CreditDataSO>();
                dataSO.entries = entries.ToArray();

                // 保存処理
                string path = Path.Combine(saveFolder, $"{sheetName}.asset");
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                AssetDatabase.CreateAsset(dataSO, path);
                Debug.Log($"✅ シート「{sheetName}」→ {path} を生成しました（{entries.Count}件）");

            } while (reader.NextResult());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ Excelの全シートをCreditDataSOとして生成完了！");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Excelファイル操作中に致命的なエラーが発生しました: {ex.Message}");
        }
    }
}