using UnityEngine;

/// <summary>
/// SpawnDataSO に基づいて、プレハブをシーンにスポーンするマネージャー。
/// - スポーン情報は Resources フォルダから読み込む。
/// - プレハブも Resources からロードして配置。
/// </summary>
public class SpawnManager : MonoBehaviour
{
    /// <summary>
    /// 生成したオブジェクトの親となる Transform。
    /// null の場合はシーンのルートに生成される。
    /// </summary>
    public Transform spawnParent;

    /// <summary>
    /// ゲーム開始時に一度だけ呼ばれるメソッド。
    /// Resources フォルダから SpawnDataSO を読み込み、
    /// そこに記録されたスポーン情報に基づいてオブジェクトを生成する。
    /// </summary>
    void Start()
    {
        // Resources/SpawnData フォルダから SpawnDataSO を読み込む
        var spawnData = Resources.Load<SpawnDataSO>("SpawnData/SpawnDataSO");

        // データが存在しない場合はエラーを出力して処理を中断
        if (spawnData == null)
        {
            Debug.LogError("SpawnDataSOがResources/SpawnDataに存在しません！");
            return;
        }

        // スポーンデータ内の各エントリに対して処理を行う
        foreach (var entry in spawnData.entries)
        {
            // エントリの種類（type）に応じて処理を分岐
            switch (entry.type.ToLower())
            {
                case "enemy":
                    // prefabName からファイル名だけ抽出（例："Enemies/Goblin" → "Goblin"）
                    string enemyName = System.IO.Path.GetFileName(entry.prefabName);
                    // EnemyPoolから該当の敵をプールから取得し、指定位置に生成
                    var enemy = EnemyPool.Instance.GetFromPool(enemyName, entry.position);
                    enemy.ResetEnemy();
                    if (enemy == null)
                    {
                        // 取得に失敗した場合は警告ログを出す
                        Debug.LogWarning($"Enemy取得失敗: {entry.prefabName}");
                    }
                    else if (spawnParent != null)
                    {
                        // 親Transformが設定されている場合はそこに子として配置
                        enemy.transform.parent = spawnParent;
                    }
                    break;

                case "coin":
                    // CoinPoolManagerからコインをプールから取得し、指定位置に生成
                    var coin = CoinPoolManager.Instance.GetCoin(entry.position);
                    if (coin == null)
                    {
                        // 取得に失敗した場合は警告ログを出す
                        Debug.LogWarning($"Coin取得失敗: {entry.prefabName}");
                    }
                    else if (spawnParent != null)
                    {
                        // 親Transformが設定されている場合はそこに子として配置
                        coin.transform.parent = spawnParent;
                    }
                    break;

                default:
                    // 未対応のタイプの場合は警告ログを出す
                    Debug.LogWarning($"未対応のタイプ: {entry.type}");
                    break;
            }
        }
    }
}
