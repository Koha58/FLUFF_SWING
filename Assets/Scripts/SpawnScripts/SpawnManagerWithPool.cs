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
    private float spawnRange = 70f;

    /// <summary>現在のシーン用の SpawnDataSO</summary>
    private SpawnDataSO spawnData;

    /// <summary>生成済みオブジェクトを ID で管理</summary>
    private readonly Dictionary<int, GameObject> spawnedObjects = new();

    // 倒されたが、まだ範囲内にいるため再スポーンをブロックすべきID
    private readonly HashSet<int> defeatedButInRangeIds = new();

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
            // プレイヤーとスポーンポイントの距離を判定（固定）
            float distanceToSpawnPoint = Vector3.Distance(player.position, entry.position);
            bool inRangeOfSpawnPoint = distanceToSpawnPoint <= spawnRange; // スポーンポイントが範囲内か

            // --- 🎯 生成判定 ---
            // defeatedButInRangeIds に含まれていないことを確認
            if (inRangeOfSpawnPoint && !spawnedObjects.ContainsKey(entry.id) && !defeatedButInRangeIds.Contains(entry.id))
            {
                var newObj = SpawnFromPool(entry);
                if (newObj != null) spawnedObjects.Add(entry.id, newObj);
            }

            // --- 🎯 回収判定 ---
            // 1. 既にスポーンされている敵の回収 (プレイヤーが遠ざかった場合)
            if (spawnedObjects.TryGetValue(entry.id, out GameObject targetObj))
            {
                bool shouldDespawn = false;

                if (entry.type.Equals("coin", StringComparison.OrdinalIgnoreCase))
                {
                    // Coin (固定) → スポーンポイントが範囲外なら回収
                    shouldDespawn = !inRangeOfSpawnPoint;
                }
                else if (entry.type.Equals("enemy", StringComparison.OrdinalIgnoreCase))
                {
                    shouldDespawn = !inRangeOfSpawnPoint;
                }

                if (shouldDespawn)
                {
                    ReturnToPool(targetObj, entry);
                    // 回収したら、次のスポーンに備えてIDを削除
                    spawnedObjects.Remove(entry.id);
                }
            }

            // 2. 倒された敵のブロック解除判定
            if (defeatedButInRangeIds.Contains(entry.id))
            {
                // スポーンポイントが範囲外になったら、ブロックを解除（再スポーン可能にする）
                if (!inRangeOfSpawnPoint)
                {
                    defeatedButInRangeIds.Remove(entry.id);
                    Debug.Log($"[SpawnManager] Entry ID {entry.id} respawn block lifted.");
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

            if (enemy != null)
            {
                var enemyCtrl = enemy.GetComponent<EnemyController>();
                if (enemyCtrl != null)
                {
                    // 1. スポーンマネージャーの参照とIDを渡す
                    enemyCtrl.Setup(this, entry.id);

                    // 2. パトロール敵なら、ここでPatrolStartXを設定する
                    // PatrolMoveStateSO.Enter に依存せず、スポーン位置を起点とする
                    if (enemyCtrl.Type == EnemyType.Patrol)
                    {
                        enemyCtrl.PatrolStartX = entry.position.x;
                        enemyCtrl.Direction = -1; // 初期方向も強制
                    }
                }

                return enemy.gameObject;
            }
            return null;
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

    /// <summary>
    /// オブジェクトが倒された（または永続的にプールに返却された）ことを通知し、
    /// SpawnManagerの管理対象から削除する。これにより、再度スポーン可能になる。
    /// </summary>
    public void NotifyObjectDestroyed(int entryId)
    {
        // spawnedObjects から削除（この ID は倒されたため追跡不要）
        if (spawnedObjects.Remove(entryId))
        {
            // 倒されたIDをブロックリストに追加
            defeatedButInRangeIds.Add(entryId);
            Debug.Log($"[SpawnManager] Enemy ID {entryId} defeated. Blocking respawn until player leaves range.");
        }
    }

}
