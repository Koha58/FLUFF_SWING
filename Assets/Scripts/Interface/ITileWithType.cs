/// <summary>
/// タイルに共通する TileType 情報を提供するためのインターフェース。
/// CustomTile や TaggedRuleTile に実装させることで、
/// エディタ拡張などで両者を共通の型として扱えるようにする。
/// </summary>
public interface ITileWithType
{
    /// <summary>
    /// タイルのタイプ（例: Ground, Hazard など）を取得または設定する。
    /// </summary>
    CustomTile.TileType tileType { get; set; }
}
