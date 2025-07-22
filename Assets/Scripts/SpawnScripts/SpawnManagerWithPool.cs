using UnityEngine;

/// <summary>
/// プレイヤーの一定範囲内だけ敵やコインをスポーン（プールから取得）する管理クラス。
/// - Resources から SpawnDataSO を読み込んで使用。
/// - プレイヤーの位置に応じて、範囲内のものだけスポーンし、
///   範囲外になったらプールに戻す（もしくは破棄）。
/// </summary>
public class SpawnManagerWithPool : MonoBehaviour
{
    /// <summary>
    /// Resources内のSpawnDataSOのパス（拡張子なし）。
    /// </summary>
    public string spawnDataResourcePath = "SpawnData/SpawnDataSO";

    /// <summary>
    /// プレイヤーのTransform。位置取得に使用。
    /// </summary>
    public Transform player;

    /// <summary>
    /// プレイヤーからの距離。この範囲内のスポーン情報のみ管理・生成対象にする。
    /// </summary>
    private float spawnRange = 15f;

    /// <summary>
    /// 実行時にResourcesから読み込んだスポーンデータ。
    /// </summary>
    private SpawnDataSO spawnData;

    /// <summary>
    /// スポーン済みのオブジェクト管理用。
    /// IDをキーにし、現在スポーン中のGameObjectを保持する。
    /// </summary>
    private readonly System.Collections.Generic.Dictionary<int, GameObject> spawnedObjects = new();

    /// <summary>
    /// 起動時に一度呼ばれ、SpawnDataSOをResourcesから読み込む。
    /// </summary>
    void Start()
    {
        spawnData = Resources.Load<SpawnDataSO>(spawnDataResourcePath);

        if (spawnData == null)
        {
            Debug.LogError($"SpawnDataSOがResourcesから読み込めません: {spawnDataResourcePath}");
        }
    }

    /// <summary>
    /// 毎フレーム呼ばれ、プレイヤーとの距離に応じてスポーン・回収を管理する。
    /// 範囲内ならスポーンし、範囲外ならプールに返却または破棄。
    /// </summary>
    void Update()
    {
        // プレイヤー・データが揃っていなければ何もしない
        if (player == null || spawnData == null) return;

        foreach (var entry in spawnData.entries)
        {
            // プレイヤーからスポーン位置までの距離を計算
            float distance = Vector3.Distance(player.position, entry.position);

            // 距離が範囲内ならスポーン管理
            if (distance <= spawnRange)
            {
                // まだスポーンしていなければスポーンする
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
                // 範囲外ならスポーン済みなら回収する
                if (spawnedObjects.TryGetValue(entry.id, out GameObject obj))
                {
                    ReturnToPool(obj, entry);
                    spawnedObjects.Remove(entry.id);
                }
            }
        }
    }

    /// <summary>
    /// SpawnDataEntryの内容に基づいて、プールからオブジェクトを取得してスポーンする。
    /// </summary>
    /// <param name="entry">スポーンデータのエントリ</param>
    /// <returns>スポーンされたGameObject（失敗時はnull）</returns>
    private GameObject SpawnFromPool(SpawnDataEntry entry)
    {
        string type = entry.type.ToLower();

        if (type == "enemy")
        {
            // prefabNameからファイル名だけ抽出
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

    /// <summary>
    /// スポーン済みオブジェクトをプールに戻す（もしくは破棄）する処理。
    /// </summary>
    /// <param name="obj">対象のGameObject</param>
    /// <param name="entry">対応するSpawnDataEntry</param>
    private void ReturnToPool(GameObject obj, SpawnDataEntry entry)
    {
        if (entry.type == "enemy")
        {
            var enemyCtrl = obj.GetComponent<EnemyController>();
            if (enemyCtrl != null)
            {
                // Enemyの場合はEnemyPoolに返却
                EnemyPool.Instance.ReturnToPool(enemyCtrl);
            }
            else
            {
                // コンポーネントがなければ破棄
                Destroy(obj);
            }
        }
        else if (entry.type == "coin")
        {
            // Coinの場合はCoinPoolManagerに返却
            CoinPoolManager.Instance.ReturnCoin(obj);
        }
        else
        {
            // 未対応タイプは破棄
            Destroy(obj);
        }
    }
}
