using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// プレイヤーの周囲に SpawnDataSO のデータに基づいて
/// オブジェクト（Enemy / Coin）をスポーンし、プレイヤーから離れたらプールに返却する管理クラス。
/// オブジェクトプールを利用して効率的にスポーン管理を行う。
/// </summary>
public class SpawnManagerWithPool : MonoBehaviour
{
    /// <summary>プレイヤーの Transform。距離判定に使用</summary>
    public Transform player;

    /// <summary>プレイヤーからのスポーン有効範囲</summary>
    private float spawnRange = 40f;

    /// <summary>現在のシーン用の SpawnDataSO</summary>
    private SpawnDataSO spawnData;

    /// <summary>生成済みオブジェクトを ID で管理</summary>
    private readonly Dictionary<int, GameObject> spawnedObjects = new();

    /// <summary>
    /// Start: シーン名に応じて SpawnDataSO をロード
    /// </summary>
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Resources/SpawnData フォルダからシーン名の SpawnDataSO をロード
        spawnData = Resources.Load<SpawnDataSO>($"SpawnData/{sceneName}");

        if (spawnData == null)
        {
            Debug.LogError($"SpawnDataSO が見つかりません: Resources/SpawnData/{sceneName}");
        }
    }

    /// <summary>
    /// Update: プレイヤーの位置に応じてオブジェクトのスポーン・回収を管理
    /// </summary>
    void Update()
    {
        // プレイヤーやデータが無ければ処理しない
        if (player == null || spawnData == null) return;

        foreach (var entry in spawnData.entries)
        {
            // プレイヤーとの距離を計算
            float distance = Vector3.Distance(player.position, entry.position);

            if (distance <= spawnRange)
            {
                // 範囲内でまだ生成されていなければスポーン
                if (!spawnedObjects.ContainsKey(entry.id))
                {
                    var obj = SpawnFromPool(entry);
                    if (obj != null) spawnedObjects.Add(entry.id, obj);
                }
            }
            else
            {
                // 範囲外の場合、生成済みならプールに返却して管理から削除
                if (spawnedObjects.TryGetValue(entry.id, out GameObject obj))
                {
                    ReturnToPool(obj, entry);
                    spawnedObjects.Remove(entry.id);
                }
            }
        }
    }

    /// <summary>
    /// SpawnDataEntry に応じてオブジェクトをプールから取得してスポーン
    /// </summary>
    /// <param name="entry">SpawnDataEntry</param>
    /// <returns>生成された GameObject、取得失敗なら null</returns>
    private GameObject SpawnFromPool(SpawnDataEntry entry)
    {
        string type = entry.type.ToLower();

        if (type == "enemy")
        {
            // Enemy は EnemyPool から取得
            string enemyName = System.IO.Path.GetFileName(entry.prefabName);
            var enemy = EnemyPool.Instance.GetFromPool(enemyName, entry.position);
            return enemy ? enemy.gameObject : null;
        }
        else if (type == "coin")
        {
            // Coin は CoinPoolManager から取得
            return CoinPoolManager.Instance.GetCoin(entry.position);
        }
        else
        {
            Debug.LogWarning($"Unknown spawn type: {entry.type}");
            return null;
        }
    }

    /// <summary>
    /// オブジェクトをプールに返却
    /// </summary>
    /// <param name="obj">返却する GameObject</param>
    /// <param name="entry">対応する SpawnDataEntry</param>
    private void ReturnToPool(GameObject obj, SpawnDataEntry entry)
    {
        if (entry.type == "enemy")
        {
            // Enemy は EnemyController を経由してプールに返却
            var enemyCtrl = obj.GetComponent<EnemyController>();
            if (enemyCtrl != null)
                EnemyPool.Instance.ReturnToPool(enemyCtrl);
            else
                Destroy(obj); // 取得できなければ破棄
        }
        else if (entry.type == "coin")
        {
            // Coin は CoinPoolManager に返却
            CoinPoolManager.Instance.ReturnCoin(obj);
        }
        else
        {
            // 未知のタイプは破棄
            Destroy(obj);
        }
    }
}
