using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Custom Tile", menuName = "Tiles/Custom Tile")]
public class CustomTile : Tile
{
    public enum TileType
    {
        Ground,
        Hazard
    }

    public TileType tileType;
}
