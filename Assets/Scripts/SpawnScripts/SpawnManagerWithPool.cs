using UnityEngine;

/// <summary>
/// プレイヤーの一定範囲内だけ敵やコインをスポーン（プールから取得）する管理クラスの例。
/// SpawnDataSOを元に生成位置を持ち、プレイヤーとの距離が一定範囲内になったらプールからスポーン。
/// </summary>
public class SpawnManagerWithPool : MonoBehaviour
{
    public SpawnDataSO spawnData;       // スポーン位置や種類のリストが入ったScriptableObject
    public Transform player;             // プレイヤーのTransform（位置取得用）
    private float spawnRange = 15f;      // プレイヤーからこの距離以内でスポーン・管理を行う

    // スポーンしたオブジェクト管理用（IDをキーにして管理）
    private readonly System.Collections.Generic.Dictionary<int, GameObject> spawnedObjects = new();

    /// <summary>
    /// 毎フレーム呼ばれ、プレイヤーとの距離に応じてスポーン・回収を管理する。
    /// </summary>
    void Update()
    {
        if (player == null) return; // プレイヤーが設定されていなければ処理しない

        foreach (var entry in spawnData.entries)
        {
            // プレイヤーとエントリーの位置の距離を計算
            float distance = Vector3.Distance(player.position, entry.position);
            Debug.Log($"entry.id={entry.id}, distance={distance}");

            // 一定範囲内ならスポーン（まだスポーンしていなければ）
            if (distance <= spawnRange)
            {
                if (!spawnedObjects.ContainsKey(entry.id))
                {
                    GameObject obj = SpawnFromPool(entry); // プールから取得して生成
                    if (obj != null)
                    {
                        spawnedObjects.Add(entry.id, obj); // 管理辞書に登録
                    }
                }
            }
            else
            {
                // 範囲外ならスポーン済みならプールに戻して管理から削除
                if (spawnedObjects.TryGetValue(entry.id, out GameObject obj))
                {
                    ReturnToPool(obj, entry);
                    spawnedObjects.Remove(entry.id);
                }
            }
        }
    }

    /// <summary>
    /// スポーン処理。SpawnDataEntryのtypeにより敵かコインか判別し、
    /// 対応するオブジェクトプールから取得して配置する。
    /// </summary>
    /// <param name="entry">スポーン情報</param>
    /// <returns>生成（取得）したゲームオブジェクト</returns>
    private GameObject SpawnFromPool(SpawnDataEntry entry)
    {
        string type = entry.type.ToLower();

        if (type == "enemy")
        {
            // prefabNameからファイル名だけ抜き出す（例: "Enemies/Goblin" → "Goblin"）
            string enemyName = System.IO.Path.GetFileName(entry.prefabName);

            // EnemyPoolから取得し、EnemyControllerのゲームオブジェクトを返す
            var enemy = EnemyPool.Instance.GetFromPool(enemyName, entry.position);
            return enemy ? enemy.gameObject : null;
        }
        else if (type == "coin")
        {
            // CoinPoolManagerからコインを取得
            return CoinPoolManager.Instance.GetCoin(entry.position);
        }
        else
        {
            Debug.LogWarning($"Unknown spawn type: {entry.type}");
            return null;
        }
    }

    /// <summary>
    /// 生成済みオブジェクトをプールに戻す処理。
    /// entryのtypeに応じて敵かコインか判別し、対応したプールのReturn処理を呼ぶ。
    /// </summary>
    /// <param name="obj">プールに戻すゲームオブジェクト</param>
    /// <param name="entry">対応するスポーン情報</param>
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
                Destroy(obj); // EnemyControllerが無ければ破棄
            }
        }
        else if (entry.type == "coin")
        {
            CoinPoolManager.Instance.ReturnCoin(obj);
        }
        else
        {
            // 不明なタイプは破棄
            Destroy(obj);
        }
    }
}
