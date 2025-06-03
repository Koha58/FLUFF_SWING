using UnityEditor;
using UnityEngine;
using System.IO;
using System.Globalization;

/// <summary>
/// CSVファイルからCharacterStatusのScriptableObjectを生成・更新するエディタ拡張クラス
/// </summary>
public class CsvToScriptableObjectImporter : EditorWindow
{
    // メニューに「Tools/Import Character Status CSV」を追加し、そこからImportメソッドを実行可能にする
    [MenuItem("Tools/Import Character Status CSV")]
    public static void Import()
    {
        // CSVファイルのパス（Assetsフォルダ直下のDataフォルダ内）
        string csvPath = Application.dataPath + "/Data/CharacterStatus.csv";
        // ScriptableObjectの保存先パス（Resourcesフォルダ内）
        string assetPath = "Assets/Resources/CharacterStatus/";

        // 保存先フォルダがなければ作成する
        if (!Directory.Exists(assetPath))
            Directory.CreateDirectory(assetPath);

        // 既存のScriptableObjectをすべて削除し、重複を防ぐ
        foreach (string file in Directory.GetFiles(assetPath, "*.asset"))
        {
            AssetDatabase.DeleteAsset(file);
        }

        // CSVファイルの全行を読み込む（1行目はヘッダーと想定）
        string[] lines = File.ReadAllLines(csvPath);

        // 2行目（インデックス1）以降をループ処理
        for (int i = 1; i < lines.Length; i++) // skip header
        {
            // 行の内容をカンマで分割
            string[] values = lines[i].Split(',');

            // 値が不足しているか、キャラクター名が空の場合は処理をスキップし警告ログを出す
            if (values.Length < 5 || string.IsNullOrEmpty(values[1]))
            {
                Debug.LogWarning($"スキップされた行: {lines[i]}");
                continue;
            }

            // 新しいCharacterStatus ScriptableObjectを作成
            CharacterStatus status = ScriptableObject.CreateInstance<CharacterStatus>();
            // CSVの各列を対応するフィールドに変換してセット
            status.id = int.Parse(values[0]);
            status.characterName = values[1];
            status.maxHP = int.Parse(values[2]);
            status.attack = int.Parse(values[3]);
            // floatのパースはカルチャー依存しないようInvariantCultureを指定
            status.moveSpeed = float.Parse(values[4], CultureInfo.InvariantCulture);

            // 保存するファイル名をキャラクター名に基づいて作成
            string name = $"CharacterStatus_{status.characterName}";
            // 指定フォルダにScriptableObjectアセットとして保存
            AssetDatabase.CreateAsset(status, $"{assetPath}{name}.asset");
        }

        // アセットデータベースに変更を保存し、Unityエディタに反映させる
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CharacterStatus ScriptableObjects Imported!");
    }
}