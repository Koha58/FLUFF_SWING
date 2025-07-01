using UnityEngine;

/// <summary>
/// 敵が爆弾を投げる専用クラス
/// - AnimatorのイベントからThrow()が呼ばれて爆弾を生成し、プレイヤー方向に投げる
/// - 投げる爆弾はbombPrefabで指定
/// - ダメージ量は敵のステータス（status）から取得
/// - 投げる力は minThrowForce 〜 maxThrowForce の範囲でランダム
/// </summary>
public class EnemyThrowAttack : MonoBehaviour
{
    /// <summary>
    /// 投げる爆弾のプレハブ。
    /// </summary>
    [SerializeField] private GameObject bombPrefab;

    /// <summary>
    /// 敵のステータス情報。
    /// </summary>
    [SerializeField] private CharacterStatus status;

    /// <summary>
    /// プレイヤーのTransform。
    /// </summary>
    [SerializeField] private Transform player;

    /// <summary>
    /// 投げる力の最小値（内部固定値）。
    /// </summary>
    private float minThrowForce = 4f;

    /// <summary>
    /// 投げる力の最大値（内部固定値）。
    /// </summary>
    private float maxThrowForce = 8f;

    private void Start()
    {
        // playerが未設定なら、"Player" タグ付きオブジェクトを探す
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Player tag のオブジェクトが見つかりません！");
            }
        }
    }

    /// <summary>
    /// プレイヤーのTransformと敵のステータスをセットする初期化メソッド。
    /// </summary>
    public void Initialize(Transform playerTransform, CharacterStatus enemyStatus)
    {
        player = playerTransform;
        status = enemyStatus;
    }

    /// <summary>
    /// アニメーションイベントから呼ばれるメソッド。
    /// 爆弾を生成し、プレイヤー方向に投げる。
    /// </summary>
    public void Throw()
    {
        Debug.Log("Throw() called.");

        if (bombPrefab == null)
        {
            Debug.LogWarning("bombPrefab is null!");
            return;
        }
        if (player == null)
        {
            Debug.LogWarning("player is null!");
            return;
        }
        if (status == null)
        {
            Debug.LogWarning("status is null!");
            return;
        }

        // --- 🎯 Playerの方向に向き調整 ---
        Vector2 dirToPlayer = player.position - transform.position;

        // X座標だけで判定：右なら -1, 左なら +1 (PlayerのSpriteの基準が左のため)
        if (dirToPlayer.x > 0f)
        {
            // Playerが右側 → 右を向く
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            // Playerが左側 → 左を向く
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // ---------------------------------

        // ランダムな投げる力を決定
        float randomForce = Random.Range(minThrowForce, maxThrowForce);

        GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Bomb bombScript = bomb.GetComponent<Bomb>();

        if (bombScript != null)
        {
            Vector2 dir = dirToPlayer.normalized;
            bombScript.Launch(dir.x, randomForce, status.attack);
            Debug.Log($"Bomb launched with random force: {randomForce}");
        }
        else
        {
            Debug.LogWarning("Bomb prefab does not have Bomb script!");
        }
    }

}
