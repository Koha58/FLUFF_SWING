using UnityEngine;

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
