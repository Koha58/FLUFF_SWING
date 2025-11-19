using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

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

    void Update()
    {
        if (player == null || spawnData == null) return;

        foreach (var entry in spawnData.entries)
        {
            float distanceToSpawnPoint = Vector3.Distance(player.position, entry.position);
            bool inRangeOfSpawnPoint = distanceToSpawnPoint <= spawnRange;

            bool isVisibleFromCamera = IsVisibleFromCamera(entry.position);

            // 🎯 生成判定（どちらも SpawnDataEntry の position を使う）
            if ((inRangeOfSpawnPoint || isVisibleFromCamera) && !spawnedObjects.ContainsKey(entry.id))
            {
                // 💡 1つ目の obj を宣言
                var newObj = SpawnFromPool(entry); // <-- 変数名を 'newObj' などに変更
                if (newObj != null) spawnedObjects.Add(entry.id, newObj);
            }

            // 🎯 回収判定（Coin と Enemy で分ける）
            // 💡 2つ目の obj を宣言 (名前を変更)
            if (spawnedObjects.TryGetValue(entry.id, out GameObject targetObj)) // <-- 変数名を 'targetObj' などに変更
            {
                bool shouldDespawn = false;

                if (entry.type.Equals("coin", StringComparison.OrdinalIgnoreCase))
                {
                    // Coin (固定) → 元のSpawn位置で判定
                    shouldDespawn = !inRangeOfSpawnPoint && !IsVisibleFromCamera(entry.position);
                }
                else if (entry.type.Equals("enemy", StringComparison.OrdinalIgnoreCase))
                {
                    // Enemy (移動) → 現在位置で判定！
                    // 💡 targetObj を使用
                    float currentDistance = Vector3.Distance(player.position, targetObj.transform.position);
                    bool enemyInRange = currentDistance <= spawnRange;
                    // 💡 targetObj を使用
                    bool enemyVisible = IsVisibleFromCamera(targetObj.transform.position);

                    shouldDespawn = !enemyInRange && !enemyVisible;
                }

                if (shouldDespawn)
                {
                    // 💡 targetObj を使用
                    ReturnToPool(targetObj, entry);
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

    bool IsVisibleFromCamera(Vector3 position)
    {
        var viewportPos = Camera.main.WorldToViewportPoint(position);
        return viewportPos.z > 0 &&
               viewportPos.x > 0 && viewportPos.x < 1 &&
               viewportPos.y > 0 && viewportPos.y < 1;
    }

}
