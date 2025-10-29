using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using ExcelDataReader;

/// <summary>
/// Excelファイルからスポーンデータを読み込み、UnityのScriptableObjectとして生成する
/// エディタ拡張ウィンドウ。
/// 
/// <para>
/// 主な機能:
/// 1. 指定されたExcelファイルの全シートを順番に読み込み。
/// 2. 各シートのデータをSpawnDataEntryとして解析。
/// 3. シートごとにSpawnDataSOを生成して指定フォルダに保存。
/// 4. アセットの保存・更新を自動で行う。
/// </para>
///
/// <para>
/// 使用例:
/// Tools → SpawnData Excel Importer からウィンドウを開き、
/// Excelファイルパスと保存先を指定して「Import All Sheets」を押すと
/// ScriptableObjectが自動生成される。
/// </para>
///
/// <para>
/// 注意点:
/// - Excelの1行目はヘッダーとしてスキップされる。
/// - 必須列は6列(id, type, prefabName, x, y, z)。
/// - 保存先フォルダはResources配下を推奨。
/// </para>
/// </summary>
public class SpawnDataExcelImporter : EditorWindow
{
    // Excelファイルのパス（デフォルト）
    string excelPath = "Assets/Data/SpawnData.xlsx";
    // ScriptableObjectを保存するフォルダ（Resources内推奨）
    string saveFolder = "Assets/Resources/SpawnData/";

    // メニューからエディタウィンドウを開くためのメソッド
    [MenuItem("Tools/SpawnData Excel Importer")]
    public static void ShowWindow()
    {
        // ウィンドウを表示
        GetWindow<SpawnDataExcelImporter>("SpawnData Excel Importer");
    }

    // エディタウィンドウのGUI描画
    void OnGUI()
    {
        GUILayout.Label("Excel File Path (.xlsx)");
        excelPath = EditorGUILayout.TextField(excelPath); // Excelファイルパス入力欄

        GUILayout.Label("Save Folder (inside Resources)");
        saveFolder = EditorGUILayout.TextField(saveFolder); // 保存先フォルダ入力欄

        // ボタンを押したらExcelをインポート
        if (GUILayout.Button("Import All Sheets"))
        {
            ImportExcel();
        }
    }

    // Excelファイルを読み込み、各シートをScriptableObjectとして生成
    void ImportExcel()
    {
        // Excelファイル存在確認
        if (!File.Exists(excelPath))
        {
            Debug.LogError($"Excelファイルが見つかりません: {excelPath}");
            return;
        }

        // ファイルを開く
        using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        // 全シートを処理
        do
        {
            string sheetName = reader.Name.Trim(); // シート名取得
            List<SpawnDataEntry> entries = new List<SpawnDataEntry>();

            bool isHeader = true; // 1行目はヘッダーなのでスキップ

            while (reader.Read())
            {
                if (isHeader) { isHeader = false; continue; } // ヘッダー行をスキップ

                if (reader.FieldCount < 6) continue; // 必要列が不足していたらスキップ

                try
                {
                    // Excelの行データをSpawnDataEntryに変換
                    SpawnDataEntry entry = new SpawnDataEntry();
                    entry.id = reader.GetInt32(0); // ID
                    entry.type = reader.GetString(1).Trim(); // 種類
                    entry.prefabName = reader.GetString(2).Trim(); // プレハブ名

                    // 座標データを取得
                    float x = float.Parse(reader.GetValue(3).ToString());
                    float y = float.Parse(reader.GetValue(4).ToString());
                    float z = float.Parse(reader.GetValue(5).ToString());

                    entry.position = new Vector3(x, y, z);
                    entries.Add(entry); // リストに追加
                }
                catch
                {
                    // データ解析に失敗した場合は警告を出力
                    Debug.LogWarning($"[{sheetName}] 行 {reader.Depth + 1} でデータ解析に失敗しました");
                }
            }

            // エントリが無ければ次のシートへ
            if (entries.Count == 0) continue;

            // ScriptableObjectを作成し、データを格納
            var dataSO = ScriptableObject.CreateInstance<SpawnDataSO>();
            dataSO.entries = entries.ToArray();

            // 保存先のパスを作成
            string path = Path.Combine(saveFolder, $"{sheetName}.asset");
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir); // ディレクトリがなければ作成

            // ScriptableObjectをアセットとして保存
            AssetDatabase.CreateAsset(dataSO, path);
            Debug.Log($"✅ シート「{sheetName}」→ {path} を生成しました（{entries.Count}件）");

        } while (reader.NextResult()); // 次のシートへ

        // アセットを保存・リフレッシュ
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ Excelの全シートをSpawnDataSOとして生成完了！");
    }
}

