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
        // すでにInstanceが存在したらこのオブジェクトを破棄し、唯一のInstanceを維持する
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 各敵の種類ごとにプールを作成し、初期数分インスタンスを生成して非アクティブにする
        foreach (var entry in enemyPrefabs)
        {
            Queue<EnemyController> queue = new Queue<EnemyController>();
            for (int i = 0; i < entry.initialPoolSize; i++)
            {
                var enemy = Instantiate(entry.prefab, transform);
                enemy.gameObject.SetActive(false);  // 初期は非アクティブで待機
                enemy.name = entry.enemyName;       // 敵の名前を設定（管理しやすくするため）
                queue.Enqueue(enemy);
            }
            poolDict[entry.enemyName] = queue;
        }
    }

    /// <summary>
    /// 指定した敵の種類のオブジェクトをプールから取得し、指定位置に配置して有効化する
    /// </summary>
    /// <param name="enemyName">敵の種類名</param>
    /// <param name="position">配置位置</param>
    /// <returns>取得したEnemyController</returns>
    public EnemyController GetFromPool(string enemyName, Vector3 position)
    {
        // プールに指定の敵種類がない場合はエラーを出してnullを返す
        if (!poolDict.ContainsKey(enemyName))
        {
            Debug.LogError($"EnemyPoolに'{enemyName}'のプールが存在しません！");
            return null;
        }

        var queue = poolDict[enemyName];
        EnemyController enemy;

        if (queue.Count > 0)
        {
            // キューに空きがあればそこから取得
            enemy = queue.Dequeue();
        }
        else
        {
            // キューが空の場合は新たに生成する
            var entry = enemyPrefabs.Find(e => e.enemyName == enemyName);
            if (entry == null)
            {
                Debug.LogError($"EnemyPrefabEntryが見つかりません: {enemyName}");
                return null;
            }
            enemy = Instantiate(entry.prefab, transform);
            enemy.name = entry.enemyName;
        }

        // 位置を設定し有効化して返す
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