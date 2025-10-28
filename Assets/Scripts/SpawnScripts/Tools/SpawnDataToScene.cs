using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using ExcelDataReader;
using UnityEngine.SceneManagement;

public class SpawnDataToScene : EditorWindow
{
    string excelPath = "Assets/Data/SpawnData.xlsx";

    [MenuItem("Tools/SpawnData → Scene Markers")]
    public static void ShowWindow()
    {
        GetWindow<SpawnDataToScene>("SpawnData → Scene Markers");
    }

    void OnGUI()
    {
        GUILayout.Label("Excel Path");
        excelPath = EditorGUILayout.TextField(excelPath);

        if (GUILayout.Button("Import Excel to Scene"))
        {
            ImportExcelToScene();
        }
    }

    void ImportExcelToScene()
    {
        if (!File.Exists(excelPath))
        {
            Debug.LogError("Excelファイルが見つかりません: " + excelPath);
            return;
        }

        // ExcelDataReader の設定
        using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        do
        {
            string sheetName = reader.Name;
            bool isHeader = true;

            while (reader.Read())
            {
                if (isHeader) { isHeader = false; continue; } // 1行目はヘッダー

                if (reader.FieldCount < 6) continue;

                try
                {
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    marker.name = reader.GetString(2).Trim(); // prefabNameを名前に
                    marker.transform.position = new Vector3(
                        float.Parse(reader.GetValue(3).ToString()),
                        float.Parse(reader.GetValue(4).ToString()),
                        float.Parse(reader.GetValue(5).ToString())
                    );

                    var comp = marker.AddComponent<SpawnMarker>();
                    comp.id = int.Parse(reader.GetValue(0).ToString());
                    comp.type = reader.GetString(1).Trim();
                    comp.prefabName = reader.GetString(2).Trim();

                    // ここで必ず現在のSceneに移動
                    SceneManager.MoveGameObjectToScene(marker, SceneManager.GetActiveScene());
                }
                catch
                {
                    Debug.LogWarning($"[{sheetName}] 行 {reader.Depth + 1} でマーカー生成失敗");
                }
            }

        } while (reader.NextResult());

        Debug.Log("✅ ExcelデータをSceneマーカーに変換完了！");
    }
}
