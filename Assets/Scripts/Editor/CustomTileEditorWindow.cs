using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// CustomTile と TaggedRuleTile を一括編集するためのカスタムエディタウィンドウ。
/// プロジェクト内の該当タイルをまとめて読み込み、TileType を一括で変更できます。
/// </summary>
public class CustomTileEditorWindow : EditorWindow
{
    // GUI 上で設定する新しい TileType（Ground, Hazard など）
    private CustomTile.TileType newTileType = CustomTile.TileType.Ground;

    // 読み込んだすべてのタイル（CustomTile, TaggedRuleTile）の共通インターフェース配列
    private ITileWithType[] tiles;

    /// <summary>
    /// Unity メニューに「Tools/Custom Tile Editor」を追加する
    /// </summary>
    [MenuItem("Tools/Custom Tile Editor")]
    public static void ShowWindow()
    {
        // ウィンドウを開く（または既存のウィンドウをアクティブにする）
        GetWindow<CustomTileEditorWindow>("Custom Tile Editor");
    }

    /// <summary>
    /// エディタウィンドウのGUI描画処理（ボタン、セレクタなど）
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("Custom Tile Type Editor", EditorStyles.boldLabel);

        // タイル読み込みボタン
        if (GUILayout.Button("Load All CustomTiles & TaggedRuleTiles"))
        {
            LoadTiles();
        }

        // タイルが読み込まれている場合
        if (tiles != null && tiles.Length > 0)
        {
            // タイルタイプ選択フィールド
            newTileType = (CustomTile.TileType)EditorGUILayout.EnumPopup("New TileType", newTileType);

            // 一括設定ボタン
            if (GUILayout.Button("Set TileType for All Loaded Tiles"))
            {
                foreach (var tile in tiles)
                {
                    // Undo 対応（Ctrl+Z で戻せるように）
                    Undo.RecordObject((Object)tile, "Change Tile Type");

                    // TileType を変更
                    tile.tileType = newTileType;

                    // エディタに変更を通知
                    EditorUtility.SetDirty((Object)tile);
                }

                // アセットの変更を保存
                AssetDatabase.SaveAssets();

                Debug.Log($"Set {newTileType} to {tiles.Length} tiles.");
            }

            // ロードされたタイル数の表示
            GUILayout.Label($"{tiles.Length} tiles loaded.");
        }
        else
        {
            GUILayout.Label("No tiles loaded.");
        }
    }

    /// <summary>
    /// プロジェクト内から CustomTile と TaggedRuleTile を検索・読み込む
    /// </summary>
    private void LoadTiles()
    {
        // CustomTile アセットの GUID を取得
        var customTileGUIDs = AssetDatabase.FindAssets("t:CustomTile");

        // TaggedRuleTile アセットの GUID を取得
        var taggedRuleTileGUIDs = AssetDatabase.FindAssets("t:TaggedRuleTile");

        // GUID からアセットをロードし、null でないものを ITileWithType にキャスト
        var customTiles = customTileGUIDs
            .Select(guid => AssetDatabase.LoadAssetAtPath<CustomTile>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(tile => tile != null)
            .Cast<ITileWithType>();

        var ruleTiles = taggedRuleTileGUIDs
            .Select(guid => AssetDatabase.LoadAssetAtPath<TaggedRuleTile>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(tile => tile != null)
            .Cast<ITileWithType>();

        // 両者を結合して tiles 配列に格納
        tiles = customTiles.Concat(ruleTiles).ToArray();
    }
}
