using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コインのオブジェクトプールを管理するシングルトンクラス。
/// 必要なときにコインを取得し、不要になったら再利用可能な状態でプールに戻す。
/// </summary>
public class CoinPoolManager : MonoBehaviour
{
    /// <summary>
    /// シングルトンインスタンスへのアクセス用。
    /// </summary>
    public static CoinPoolManager Instance { get; private set; }

    /// <summary>
    /// プールするコインのプレハブ。
    /// </summary>
    [SerializeField] private GameObject coinPrefab;

    /// <summary>
    /// 起動時に生成するコインの初期数。
    /// </summary>
    [SerializeField] private int initialPoolSize = 20;

    /// <summary>
    /// 非アクティブなコインを保持するキュー。
    /// </summary>
    private Queue<GameObject> pool = new Queue<GameObject>();

    /// <summary>
    /// シングルトンの初期化と初期プール生成。
    /// </summary>
    private void Awake()
    {
        // シングルトンの重複防止
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初期プールの生成
        for (int i = 0; i < initialPoolSize; i++)
        {
            var coin = Instantiate(coinPrefab);
            coin.SetActive(false);   // 最初は非アクティブ
            pool.Enqueue(coin);      // プールに追加
        }
    }

    /// <summary>
    /// プールからコインを取得して、指定位置に配置・アクティブ化する。
    /// </summary>
    /// <param name="position">表示したいワールド座標</param>
    /// <returns>使用可能なコインのGameObject</returns>
    public GameObject GetCoin(Vector3 position)
    {
        // プールに残りがあれば再利用、なければ新規生成
        GameObject coin = pool.Count > 0 ? pool.Dequeue() : Instantiate(coinPrefab);
        coin.transform.position = position;
        coin.SetActive(true);
        return coin;
    }

    /// <summary>
    /// 使用済みコインを非アクティブにし、プールに戻す。
    /// </summary>
    /// <param name="coin">再利用対象のコイン</param>
    public void ReturnCoin(GameObject coin)
    {
        coin.SetActive(false);
        pool.Enqueue(coin);
    }
}
