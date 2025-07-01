using UnityEngine;

/// <summary>
/// 1つのスポーン情報を表すクラス（敵・アイテムなど）。
/// ScriptableObject内で使用されるデータ構造。
/// </summary>
[System.Serializable]
public class SpawnDataEntry
{
    public int id;

    /// <summary>
    /// 種別（例: "Enemy", "Item", "Coin" など）。
    /// </summary>
    public string type;

    /// <summary>
    /// 使用するプレハブ名（Resourcesフォルダ内で読み込む用）。
    /// </summary>
    public string prefabName;

    /// <summary>
    /// スポーン位置（ワールド座標）。
    /// </summary>
    public Vector3 position;
}

/// <summary>
/// 複数のスポーン情報（SpawnDataEntry）を保持するScriptableObject。
/// エディタやCSVインポートなどでデータ管理を容易にする。
/// </summary>
[CreateAssetMenu(fileName = "SpawnDataSO", menuName = "Data/SpawnData")]
public class SpawnDataSO : ScriptableObject
{
    /// <summary>
    /// スポーンデータの一覧（シーン上での配置に利用）。
    /// </summary>
    public SpawnDataEntry[] entries;
}
