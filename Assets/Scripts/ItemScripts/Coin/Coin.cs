using UnityEngine;

/// <summary>
/// プレイヤーが触れたときにコインとしての処理を行うクラス。
/// コインUIを更新し、オブジェクトプールへ返却する。
/// スイング中（ワイヤー使用中）でも確実に取得できるよう、
/// FixedUpdateで毎フレーム接触判定を行う設計。
/// </summary>
public class Coin : MonoBehaviour
{
    // === コイン獲得時の効果音 ===
    [Header("コイン獲得SE")]
    [SerializeField] private AudioClip coinSE;

    // === スポーン管理用 ===

    // このコインを管理している SpawnManager
    private SpawnManagerWithPool _spawnManager;

    // SpawnDataEntry に紐づく一意なID
    private int _entryId;

    // コインが既に取得済みかどうか（多重取得防止用）
    // FixedUpdateで毎フレーム判定するため必須
    private bool _consumed;

    /// <summary>
    /// SpawnManagerWithPool から生成時に呼ばれる初期化メソッド。
    /// このコインを管理する SpawnManager と、
    /// 対応する SpawnDataEntry の ID を受け取る。
    /// </summary>
    /// <param name="manager">このコインを管理する SpawnManager</param>
    /// <param name="entryId">SpawnDataEntry の ID</param>
    public void Setup(SpawnManagerWithPool manager, int entryId)
    {
        _spawnManager = manager;
        _entryId = entryId;
        _consumed = false;
    }

    /// <summary>
    /// プールから再利用されてアクティブ化された際に呼ばれる。
    /// 前回の取得状態をリセットする。
    /// </summary>
    private void OnEnable()
    {
        _consumed = false;
    }

    /// <summary>
    /// 物理演算の更新タイミングで毎フレーム呼ばれる。
    /// プレイヤーとの接触判定を行う。
    /// </summary>
    private void FixedUpdate()
    {
        // 既に取得済みの場合は何もしない
        if (_consumed) return;

        CheckForCoinOverlap();
    }

    /// <summary>
    /// コインの中心を基準に一定半径内に
    /// プレイヤーが存在するかを判定する。
    /// 該当した場合、コイン取得処理を行う。
    /// </summary>
    private void CheckForCoinOverlap()
    {
        // 半径0.5fの円範囲内にある Collider2D を取得
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        foreach (var hit in hits)
        {
            // プレイヤー以外は無視
            if (!hit.CompareTag("Player")) continue;

            // 取得済みフラグを立てて多重処理を防止
            _consumed = true;

            // コイン取得SEを再生
            AudioManager.Instance.PlaySE(coinSE);

            // コインUIの表示を更新
            PlayerCoinUI.Instance.AddCoin(1);

            // SpawnManager が設定されている場合は、
            // スポーン管理側に「取得された」ことを通知する
            // （プール返却 ＋ 範囲内リスポーン禁止）
            if (_spawnManager != null)
            {
                _spawnManager.NotifyObjectDestroyed(_entryId);
            }
            else
            {
                // 何らかの理由で Setup されていない場合は、
                // 従来どおり直接プールへ返却する
                CoinPoolManager.Instance.ReturnCoin(this.gameObject);
            }

            // 1回取得できれば十分なのでループを抜ける
            break;
        }
    }
}
