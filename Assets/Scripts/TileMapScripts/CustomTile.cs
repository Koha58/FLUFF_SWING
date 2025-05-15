using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// カスタムタイルクラス。
/// Unity の Tilemap で使用するタイルを拡張し、
/// 独自の属性（タイプ）を付与できるようにする。
/// </summary>
[CreateAssetMenu(fileName = "New Custom Tile", menuName = "Tiles/Custom Tile")]
public class CustomTile : Tile
{
    /// <summary>
    /// タイルの種類を表す列挙型。
    /// - Ground : プレイヤーが接続可能な地面
    /// - Hazard : 危険エリア（将来的な用途などを想定）
    /// </summary>
    public enum TileType
    {
        Ground,  // 地面タイル
        Hazard   // 危険タイル
    }

    /// <summary>
    /// このタイルのタイプ（インスペクターから選択可能）。
    /// タイルごとに Ground や Hazard などを指定できる。
    /// </summary>
    public TileType tileType;
}
