using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// タグ付きのルールベースタイル（RuleTile）
/// RuleTile を拡張し、CustomTile.TileType を保持できるようにしたクラス。
/// エディタツールから CustomTile と同様に一括で tileType を編集可能にするため、
/// ITileWithType インターフェースを実装している。
/// </summary>
[CreateAssetMenu(menuName = "Tiles/Tagged RuleTile")]
[System.Serializable]
public class TaggedRuleTile : RuleTile, ITileWithType
{
    // このタイルのタイプ（Ground, Hazard など）
    [SerializeField]
    private CustomTile.TileType _tileType = CustomTile.TileType.Ground;

    /// <summary>
    /// ITileWithType で要求される tileType プロパティ。
    /// カスタムエディタなどで共通の方法でアクセスできるようにする。
    /// </summary>
    public CustomTile.TileType tileType
    {
        get => _tileType;
        set => _tileType = value;
    }
}
