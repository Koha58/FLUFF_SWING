using UnityEngine;
using UnityEditor;
using System.IO;
using ExcelDataReader;
using UnityEngine.SceneManagement;

/// <summary>
/// Excel の SpawnMarker データを現在のシーンに読み込み、
/// シーン内に SpawnMarker を生成するエディタ拡張。
/// </summary>
public class SpawnDataToScene : EditorWindow
{
    /// <summary>読み込む Excel ファイルのパス</summary>
    string excelPath = "Assets/Data/SpawnData.xlsx";

    /// <summary>メニューからウィンドウを開く</summary>
    [MenuItem("Tools/SpawnData → Scene Markers")]
    public static void ShowWindow()
    {
        GetWindow<SpawnDataToScene>("SpawnData → Scene Markers");
    }

    /// <summary>
    /// エディタウィンドウの GUI を描画
    /// - Excel パス入力欄
    /// - 読み込みボタン
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Excel Path");
        excelPath = EditorGUILayout.TextField(excelPath);

        if (GUILayout.Button("Import Excel to Scene"))
        {
            ImportExcelToScene();
        }
    }

    /// <summary>
    /// Excel ファイルを読み込み、現在のシーンに SpawnMarker を生成する
    /// </summary>
    void ImportExcelToScene()
    {
        // ---------------- 1. ファイル存在チェック ----------------
        if (!File.Exists(excelPath))
        {
            Debug.LogError("Excelファイルが見つかりません: " + excelPath);
            return;
        }

        // 現在のシーン名を取得
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"📄 現在のScene名: {currentSceneName}");

        // ---------------- 2. Excel 読み込み設定 ----------------
        using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        bool found = false; // 現在シーン用のシートを見つけたか

        // ---------------- 3. シート単位でループ ----------------
        do
        {
            string sheetName = reader.Name;

            // 現在のシーン名と一致しないシートはスキップ
            if (!sheetName.Equals(currentSceneName, System.StringComparison.OrdinalIgnoreCase))
                continue;

            found = true;
            Debug.Log($"✅ シート「{sheetName}」を読み込み中...");

            bool isHeader = true; // 1行目はヘッダー

            // ---------------- 4. 行単位でループ ----------------
            while (reader.Read())
            {
                if (isHeader) { isHeader = false; continue; } // ヘッダーはスキップ

                if (reader.FieldCount < 6) continue; // データ列が不足している場合はスキップ

                try
                {
                    // ---------------- 4-1. GameObject生成 ----------------
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube); // 仮の形状はキューブ
                    marker.name = reader.GetString(2).Trim(); // prefabName をオブジェクト名に設定
                    marker.transform.position = new Vector3(
                        float.Parse(reader.GetValue(3).ToString()),
                        float.Parse(reader.GetValue(4).ToString()),
                        float.Parse(reader.GetValue(5).ToString())
                    );

                    // ---------------- 4-2. SpawnMarker コンポーネント追加 ----------------
                    var comp = marker.AddComponent<SpawnMarker>();
                    comp.id = int.Parse(reader.GetValue(0).ToString());
                    comp.type = reader.GetString(1).Trim();
                    comp.prefabName = reader.GetString(2).Trim();

                    // ---------------- 4-3. シーンに移動 ----------------
                    SceneManager.MoveGameObjectToScene(marker, SceneManager.GetActiveScene());
                }
                catch
                {
                    Debug.LogWarning($"[{sheetName}] 行 {reader.Depth + 1} でマーカー生成失敗");
                }
            }

            Debug.Log($"✅ シート「{sheetName}」のマーカー生成完了！");

        } while (reader.NextResult()); // 次のシートへ

        // ---------------- 5. シート未発見時の警告 ----------------
        if (!found)
        {
            Debug.LogWarning($"⚠ 現在のScene名「{currentSceneName}」に対応するシートが見つかりませんでした。");
        }
        else
        {
            Debug.Log($"🎯 Scene「{currentSceneName}」のマーカーを配置しました。");
        }
    }
}
