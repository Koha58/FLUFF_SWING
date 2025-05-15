using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// CustomTile をエディタ上で一括編集するためのカスタムエディタウィンドウ。
/// - プロジェクト内の CustomTile を全て読み込み
/// - 全タイルの TileType を一括変更
/// </summary>
public class CustomTileEditorWindow : EditorWindow
{
    // 新しく設定する TileType（GUI 上で選択可能）
    private CustomTile.TileType newTileType = CustomTile.TileType.Ground;

    // 読み込んだ CustomTile の配列
    private CustomTile[] tiles;

    /// <summary>
    /// メニューに「Tools/Custom Tile Editor」を追加。
    /// クリックするとウィンドウが表示される。
    /// </summary>
    [MenuItem("Tools/Custom Tile Editor")]
    public static void ShowWindow()
    {
        // エディタウィンドウを開く（またはフォーカス）
        GetWindow<CustomTileEditorWindow>("Custom Tile Editor");
    }

    /// <summary>
    /// エディタウィンドウの GUI 描画処理
    /// </summary>
    private void OnGUI()
    {
        // 見出し表示
        GUILayout.Label("Custom Tile Type Editor", EditorStyles.boldLabel);

        // タイル読み込みボタン
        if (GUILayout.Button("Load All CustomTiles"))
        {
            LoadTiles();
        }

        // タイルがロード済みかチェック
        if (tiles != null && tiles.Length > 0)
        {
            // TileType の選択 UI
            newTileType = (CustomTile.TileType)EditorGUILayout.EnumPopup("New TileType", newTileType);

            // 全タイルに対して新しいタイプを一括設定
            if (GUILayout.Button("Set TileType for All Loaded Tiles"))
            {
                foreach (var tile in tiles)
                {
                    // Undo に対応させる（Ctrl+Z対応）
                    Undo.RecordObject(tile, "Change Tile Type");

                    // タイルのタイプを変更
                    tile.tileType = newTileType;

                    // 変更をエディタに通知（インスペクタ更新など）
                    EditorUtility.SetDirty(tile);
                }

                // 変更を保存
                AssetDatabase.SaveAssets();

                Debug.Log($"Set {newTileType} to {tiles.Length} tiles.");
            }

            // 読み込んだタイル数表示
            GUILayout.Label($"{tiles.Length} tiles loaded.");
        }
        else
        {
            // タイルがロードされていない場合のメッセージ
            GUILayout.Label("No tiles loaded.");
        }
    }

    /// <summary>
    /// プロジェクト内からすべての CustomTile アセットを検索して読み込む
    /// </summary>
    private void LoadTiles()
    {
        // タイプ CustomTile の GUID を全検索
        string[] guids = AssetDatabase.FindAssets("t:CustomTile");

        // GUID からパスを取得し、実際のアセットとしてロード
        tiles = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<CustomTile>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToArray();
    }
}
