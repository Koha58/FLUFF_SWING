using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private int initialPoolSize = 10;

    private Queue<EnemyController> pool = new Queue<EnemyController>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 初期プール生成
        for (int i = 0; i < initialPoolSize; i++)
        {
            var enemy = Instantiate(enemyPrefab, transform);
            enemy.gameObject.SetActive(false);
            pool.Enqueue(enemy);
        }
    }

    // プールから取得
    public EnemyController GetFromPool(Vector3 position)
    {
        EnemyController enemy;
        if (pool.Count > 0)
        {
            enemy = pool.Dequeue();
        }
        else
        {
            enemy = Instantiate(enemyPrefab, transform);
        }
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    // プールに返却
    public void ReturnToPool(EnemyController enemy)
    {
        enemy.gameObject.SetActive(false);
        pool.Enqueue(enemy);
    }
}
