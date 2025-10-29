using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps; // TileクラスやTilemap系APIに必要

/// <summary>
/// カスタムタイル（CustomTile）を一括作成するエディタ拡張ウィンドウ。
/// 指定したスプライトフォルダ内のスプライトから、CustomTileを一括で生成します。
/// </summary>
public class CustomTileBatchCreator : EditorWindow
{
    // スプライトが格納されているフォルダ（ユーザー指定）
    private Object spriteFolder;

    // 作成したタイルを保存するパス
    private string savePath = "Assets/Tiles/";

    // 作成するタイルのデフォルトタイプ（GroundまたはHazard）
    private CustomTile.TileType defaultTileType = CustomTile.TileType.Ground;

    // Hazardタイプの場合のデフォルトダメージ量
    private int defaultDamageAmount = 1;

    // メニューから開けるようにする
    [MenuItem("Tools/Custom Tile Batch Creator")]
    public static void ShowWindow()
    {
        GetWindow<CustomTileBatchCreator>("Custom Tile Batch Creator");
    }

    // エディタウィンドウのGUI描画
    private void OnGUI()
    {
        GUILayout.Label("カスタムタイル一括作成", EditorStyles.boldLabel);

        // スプライトフォルダ指定フィールド
        spriteFolder = EditorGUILayout.ObjectField("スプライトフォルダ", spriteFolder, typeof(Object), false);

        // 保存先パス指定フィールド
        savePath = EditorGUILayout.TextField("保存先パス", savePath);

        // デフォルトのタイルタイプ選択
        defaultTileType = (CustomTile.TileType)EditorGUILayout.EnumPopup("デフォルトTileType", defaultTileType);

        // タイルタイプがHazardならダメージ量も指定
        if (defaultTileType == CustomTile.TileType.Hazard)
        {
            defaultDamageAmount = EditorGUILayout.IntField("デフォルトダメージ量", defaultDamageAmount);
        }

        // 一括作成ボタン
        if (GUILayout.Button("一括作成"))
        {
            CreateCustomTiles();
        }
    }

    /// <summary>
    /// 指定されたスプライトからCustomTileを一括生成する処理
    /// </summary>
    private void CreateCustomTiles()
    {
        // スプライトフォルダ未指定の場合はエラー
        if (spriteFolder == null)
        {
            Debug.LogError("スプライトフォルダが指定されていません。");
            return;
        }

        // フォルダパスを取得
        string folderPath = AssetDatabase.GetAssetPath(spriteFolder);

        // 指定フォルダ内の全スプライトを取得
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

        // 保存先フォルダが存在しない場合は作成
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // 各スプライトに対してタイルを作成
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            // CustomTileのインスタンスを生成
            CustomTile tile = ScriptableObject.CreateInstance<CustomTile>();

            // スプライト設定
            tile.sprite = sprite;

            // コライダーのタイプ（スプライト形状に一致）
            tile.colliderType = Tile.ColliderType.Sprite;

            // デフォルトのタイルタイプを設定
            tile.tileType = defaultTileType;

            // Hazardの場合はダメージ量も設定
            if (tile.tileType == CustomTile.TileType.Hazard)
            {
                tile.damageAmount = defaultDamageAmount;
            }

            // ファイル名と保存先パスを構築
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string tilePath = Path.Combine(savePath, fileName + ".asset");

            // アセットとして保存
            AssetDatabase.CreateAsset(tile, tilePath);
        }

        // アセットを保存・更新
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("カスタムタイルの一括作成が完了しました。");
    }
}
