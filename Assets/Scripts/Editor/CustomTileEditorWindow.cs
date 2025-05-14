using UnityEditor;
using UnityEngine;
using System.Linq;

public class CustomTileEditorWindow : EditorWindow
{
    private CustomTile.TileType newTileType = CustomTile.TileType.Ground;
    private CustomTile[] tiles;

    [MenuItem("Tools/Custom Tile Editor")]
    public static void ShowWindow()
    {
        GetWindow<CustomTileEditorWindow>("Custom Tile Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Custom Tile Type Editor", EditorStyles.boldLabel);

        if (GUILayout.Button("Load All CustomTiles"))
        {
            LoadTiles();
        }

        if (tiles != null && tiles.Length > 0)
        {
            newTileType = (CustomTile.TileType)EditorGUILayout.EnumPopup("New TileType", newTileType);

            if (GUILayout.Button("Set TileType for All Loaded Tiles"))
            {
                foreach (var tile in tiles)
                {
                    Undo.RecordObject(tile, "Change Tile Type");
                    tile.tileType = newTileType;
                    EditorUtility.SetDirty(tile);
                }
                AssetDatabase.SaveAssets();
                Debug.Log($"Set {newTileType} to {tiles.Length} tiles.");
            }

            GUILayout.Label($"{tiles.Length} tiles loaded.");
        }
        else
        {
            GUILayout.Label("No tiles loaded.");
        }
    }

    private void LoadTiles()
    {
        string[] guids = AssetDatabase.FindAssets("t:CustomTile");
        tiles = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<CustomTile>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToArray();
    }
}
