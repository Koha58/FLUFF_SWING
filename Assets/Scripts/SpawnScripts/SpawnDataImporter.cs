using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// CSVファイルを読み込んで SpawnDataSO（ScriptableObject）を自動生成するエディタ拡張ツール。
/// Tools > SpawnData Importer から起動可能。
/// </summary>
public class SpawnDataImporter : EditorWindow
{
    // CSVファイルのパス（初期値）
    string csvPath = "Assets/Data/SpawnData.csv";

    // ScriptableObjectの保存パス（初期値）
    string assetSavePath = "Assets/Resources/SpawnData/SpawnDataSO.asset";

    /// <summary>
    /// メニューからウィンドウを開く
    /// </summary>
    [MenuItem("Tools/SpawnData Importer")]
    public static void ShowWindow()
    {
        GetWindow<SpawnDataImporter>("SpawnData Importer");
    }

    /// <summary>
    /// ウィンドウのGUI描画
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("CSV Path"); // ラベル
        csvPath = EditorGUILayout.TextField(csvPath); // CSVファイルパス入力欄

        GUILayout.Label("Save Asset Path"); // ラベル
        assetSavePath = EditorGUILayout.TextField(assetSavePath); // 出力先パス入力欄

        // インポートボタン
        if (GUILayout.Button("Import CSV to ScriptableObject"))
        {
            ImportCSV();
        }
    }

    /// <summary>
    /// CSVファイルを読み込んで ScriptableObject を生成する
    /// </summary>
    void ImportCSV()
    {
        // ファイル存在チェック
        if (!File.Exists(csvPath))
        {
            Debug.LogError("CSVファイルが見つかりません: " + csvPath);
            return;
        }

        // 全行読み込み
        string[] lines = File.ReadAllLines(csvPath);

        if (lines.Length < 2)
        {
            Debug.LogError("CSVファイルの内容が不足しています（最低1行のデータが必要）");
            return;
        }

        // ScriptableObjectインスタンス作成
        SpawnDataSO dataSO = ScriptableObject.CreateInstance<SpawnDataSO>();
        dataSO.entries = new SpawnDataEntry[lines.Length - 1]; // 1行目はヘッダー

        // 各データ行をパース
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');

            SpawnDataEntry entry = new SpawnDataEntry();
            entry.type = cols[1].Trim();           // 種類（例: Enemy, Coin）
            entry.prefabName = cols[2].Trim();     // プレハブ名

            float x = float.Parse(cols[3]);        // 座標X
            float y = float.Parse(cols[4]);        // 座標Y
            float z = float.Parse(cols[5]);        // 座標Z

            entry.position = new Vector3(x, y, z); // Vector3として格納

            dataSO.entries[i - 1] = entry;         // 配列に追加
        }

        // 保存先ディレクトリが存在しない場合は作成
        string dir = Path.GetDirectoryName(assetSavePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh(); // Unityエディタに反映
        }

        // ScriptableObject をアセットとして保存
        AssetDatabase.CreateAsset(dataSO, assetSavePath);
        AssetDatabase.SaveAssets();

        Debug.Log("SpawnDataSOを作成しました: " + assetSavePath);
    }
}
