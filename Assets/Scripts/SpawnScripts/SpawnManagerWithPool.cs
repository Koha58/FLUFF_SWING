using UnityEngine;

/// <summary>
/// プレイヤーの一定範囲内だけ敵やコインをスポーン（プールから取得）する管理クラスの例。
/// SpawnDataSOを元に生成位置を持ち、プレイヤーとの距離が一定範囲内になったらプールからスポーン。
/// </summary>
public class SpawnManagerWithPool : MonoBehaviour
{
    public SpawnDataSO spawnData;       // スポーン位置や種類のリスト
    public Transform player;             // プレイヤーのTransform
    private float spawnRange = 15f;

    // スポーンしたオブジェクト管理用（インスタンス化後に保持）
    private readonly System.Collections.Generic.Dictionary<int, GameObject> spawnedObjects = new();

    void Update()
    {
        if (player == null) return;

        foreach (var entry in spawnData.entries)
        {
            float distance = Vector3.Distance(player.position, entry.position);
            Debug.Log($"entry.id={entry.id}, distance={distance}");

            if (distance <= spawnRange)
            {
                if (!spawnedObjects.ContainsKey(entry.id))
                {
                    GameObject obj = SpawnFromPool(entry);
                    if (obj != null)
                    {
                        spawnedObjects.Add(entry.id, obj);
                    }
                }
            }
            else
            {
                if (spawnedObjects.TryGetValue(entry.id, out GameObject obj))
                {
                    ReturnToPool(obj, entry);
                    spawnedObjects.Remove(entry.id);
                }
            }
        }

    }

    // スポーン処理（EnemyかCoinか判定してプールから取得）
    private GameObject SpawnFromPool(SpawnDataEntry entry)
    {
        string type = entry.type.ToLower();

        if (type == "enemy")
        {
            // prefabNameからファイル名だけ抜き出す（最後のスラッシュ以降）
            string enemyName = System.IO.Path.GetFileName(entry.prefabName);

            var enemy = EnemyPool.Instance.GetFromPool(enemyName, entry.position);
            return enemy ? enemy.gameObject : null;
        }
        else if (type == "coin")
        {
            return CoinPoolManager.Instance.GetCoin(entry.position);
        }
        else
        {
            Debug.LogWarning($"Unknown spawn type: {entry.type}");
            return null;
        }
    }


    // プールに戻す処理
    private void ReturnToPool(GameObject obj, SpawnDataEntry entry)
    {
        if (entry.type == "enemy")
        {
            var enemyCtrl = obj.GetComponent<EnemyController>();
            if (enemyCtrl != null)
            {
                Debug.Log($"ReturnToPool呼び出し: id={entry.id}, enemy.name={enemyCtrl.name}");
                EnemyPool.Instance.ReturnToPool(enemyCtrl);
            }
            else
            {
                Debug.LogWarning("ReturnToPool: EnemyControllerが見つかりません");
                Destroy(obj);
            }
        }
        else if (entry.type == "coin")
        {
            CoinPoolManager.Instance.ReturnCoin(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}
