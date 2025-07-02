using UnityEngine;

/// <summary>
/// SpawnDataSO に基づいて、プレハブをシーンにスポーンするマネージャー。
/// - スポーン情報は ScriptableObject（spawnData）から読み込む。
/// - Resources フォルダからプレハブをロードして配置。
/// - オプションで spawnParent に子として配置可能。
/// </summary>
public class SpawnManager : MonoBehaviour
{
    /// <summary>
    /// スポーンデータ（ScriptableObject）。
    /// CSVなどから事前に生成されたデータを読み込む。
    /// </summary>
    public SpawnDataSO spawnData;

    /// <summary>
    /// 生成したオブジェクトの親となる Transform。
    /// null の場合はルートに生成される。
    /// </summary>
    public Transform spawnParent;

    /// <summary>
    /// ゲーム開始時にスポーン処理を実行。
    /// </summary>
    void Start()
    {
        foreach (var entry in spawnData.entries)
        {
            switch (entry.type.ToLower())
            {
                case "enemy":
                    string enemyName = System.IO.Path.GetFileName(entry.prefabName);
                    var enemy = EnemyPool.Instance.GetFromPool(enemyName, entry.position);
                    if (enemy == null)
                    {
                        Debug.LogWarning($"Enemy取得失敗: {entry.prefabName}");
                    }
                    break;

                case "coin":
                    var coin = CoinPoolManager.Instance.GetCoin(entry.position);
                    if (coin == null)
                    {
                        Debug.LogWarning($"Coin取得失敗: {entry.prefabName}");
                    }
                    break;

                default:
                    Debug.LogWarning($"未対応のタイプ: {entry.type}");
                    break;
            }
        }
    }

}
