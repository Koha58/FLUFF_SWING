using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コインのオブジェクトプールを管理するシングルトンクラス。
/// 必要なときにコインを取得し、不要になったら再利用可能な状態でプールに戻す。
/// </summary>
public class CoinPoolManager : MonoBehaviour
{
    /// <summary>
    /// シングルトンインスタンスへのアクセス用プロパティ。
    /// これによりどこからでもCoinPoolManagerの唯一のインスタンスを取得可能。
    /// </summary>
    public static CoinPoolManager Instance { get; private set; }

    /// <summary>
    /// プールするコインのプレハブをインスペクターで設定する。
    /// </summary>
    [SerializeField] private GameObject coinPrefab;

    /// <summary>
    /// ゲーム開始時に生成しておくコインの初期数。
    /// </summary>
    [SerializeField] private int initialPoolSize = 20;

    /// <summary>
    /// 使用していない（非アクティブな）コインを保管するキュー。
    /// 新たにコインが必要なときはここから取り出し、不要になったら戻す。
    /// </summary>
    private Queue<GameObject> pool = new Queue<GameObject>();

    /// <summary>
    /// Awakeはオブジェクト生成時に呼ばれる初期化メソッド。
    /// シングルトンのセットアップと初期プール生成を行う。
    /// </summary>
    private void Awake()
    {
        // シングルトンの重複防止処理
        if (Instance == null)
            Instance = this; // 最初のインスタンスとして登録
        else
        {
            Destroy(gameObject); // すでに存在するなら自分を破棄
            return;
        }

        // 初期プールの生成
        for (int i = 0; i < initialPoolSize; i++)
        {
            // コインを生成し、CoinPoolManagerの子に設定することで
            // Hierarchyが整理される（見やすくなる）
            var coin = Instantiate(coinPrefab, transform);

            coin.SetActive(false); // 生成直後は非アクティブにする
            pool.Enqueue(coin);    // プールに追加
        }
    }

    /// <summary>
    /// プールからコインを取得し、指定した位置に配置してアクティブ化する。
    /// プールに空きがなければ新規生成し親を設定する。
    /// </summary>
    /// <param name="position">表示したいワールド座標</param>
    /// <returns>使用可能なコインのGameObject</returns>
    public GameObject GetCoin(Vector3 position)
    {
        // プールから取り出すか、新規に生成する
        GameObject coin = pool.Count > 0 ? pool.Dequeue() : Instantiate(coinPrefab, transform);

        coin.transform.position = position; // 位置設定
        coin.SetActive(true);                // 表示状態に切り替え

        return coin; // 使用可能なコインを返す
    }

    /// <summary>
    /// 使用済みのコインを非アクティブにしてプールに戻す。
    /// </summary>
    /// <param name="coin">再利用対象のコインGameObject</param>
    public void ReturnCoin(GameObject coin)
    {
        coin.SetActive(false); // 表示を消す
        pool.Enqueue(coin);    // プールに戻す（キューに追加）
    }
}
