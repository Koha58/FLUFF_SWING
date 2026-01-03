using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵キャラクターのオブジェクトプール管理クラス
/// 敵の種類ごとにプールを用意し、使いまわしでパフォーマンスを向上させる
/// シングルトンとして動作し、どこからでもアクセス可能
/// </summary>
public class EnemyPool : MonoBehaviour
{
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static EnemyPool Instance { get; private set; }

    /// <summary>
    /// 敵の種類ごとのプレハブ情報を格納するクラス
    /// プール生成時に使用し、敵名、プレハブ、初期プールサイズを設定する
    /// </summary>
    [System.Serializable]
    public class EnemyPrefabEntry
    {
        public string enemyName;           // 敵の種類名（例："Bird", "BlueRabbit"）
        public EnemyController prefab;     // その敵のプレハブ
        public int initialPoolSize = 10;   // 初期プール数（最初に生成して待機させる数）
    }

    // インスペクターで設定する敵プレハブのリスト
    [SerializeField]
    private List<EnemyPrefabEntry> enemyPrefabs = new();

    // 敵の種類名をキーにした、EnemyControllerのキュー（プール）辞書
    private Dictionary<string, Queue<EnemyController>> poolDict = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 初期化はしない。必要になった時に初回生成する（遅延生成）
    }


    /// <summary>
    /// 指定した敵の種類のオブジェクトをプールから取得し、指定位置に配置して有効化する
    /// </summary>
    /// <param name="enemyName">敵の種類名</param>
    /// <param name="position">配置位置</param>
    /// <returns>取得したEnemyController</returns>
    public EnemyController GetFromPool(string enemyName, Vector3 position)
    {
        // 存在しないなら新規でプールを作成（遅延生成）
        if (!poolDict.ContainsKey(enemyName))
        {
            var entry = enemyPrefabs.Find(e => e.enemyName == enemyName);
            if (entry == null)
            {
                Debug.LogError($"EnemyPrefabEntryが見つかりません: {enemyName}");
                return null;
            }

            poolDict[enemyName] = new Queue<EnemyController>();
        }

        var queue = poolDict[enemyName];
        EnemyController enemy;

        if (queue.Count > 0)
        {
            enemy = queue.Dequeue();
        }
        else
        {
            var entry = enemyPrefabs.Find(e => e.enemyName == enemyName);
            if (entry == null)
            {
                Debug.LogError($"EnemyPrefabEntryが見つかりません: {enemyName}");
                return null;
            }
            enemy = Instantiate(entry.prefab, transform);
            enemy.name = entry.enemyName;
        }

        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
        return enemy;
    }


    /// <summary>
    /// 敵オブジェクトをプールに戻し、非アクティブにする
    /// </summary>
    /// <param name="enemy">戻すEnemyController</param>
    public void ReturnToPool(EnemyController enemy)
    {
        // 非アクティブ化してプールに戻す
        enemy.gameObject.SetActive(false);

        if (!poolDict.ContainsKey(enemy.name))
        {
            // プールに存在しない種類の敵なら破棄する（例外対応）
            Debug.LogError($"EnemyPoolに'{enemy.name}'のプールがありません。");
            Destroy(enemy.gameObject);
            return;
        }

        // キューに戻す
        poolDict[enemy.name].Enqueue(enemy);
    }
}