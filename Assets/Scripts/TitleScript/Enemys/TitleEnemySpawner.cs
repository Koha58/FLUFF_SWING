using System;
using UnityEngine;

/// <summary>
/// タイトル画面用の敵スポーナー。
/// 
/// ・プレイヤーの進行距離を基準に、一定間隔で敵を生成する
/// ・地上敵 / 空中敵を確率で切り替える
/// ・当たり判定は一切考慮しない（完全に演出用）
/// ・敵の削除は DestroyBehind に任せる
/// </summary>
public class TitleEnemySpawner : MonoBehaviour
{
    /// <summary>
    /// 出現候補となるPrefabと、その出やすさ（重み）をまとめた構造体
    /// 
    /// weight は「確率」ではなく「比率」。
    /// 例：
    ///  A: weight=5
    ///  B: weight=1
    /// → Aが5倍出やすい
    /// </summary>
    [Serializable]
    public class WeightedPrefab
    {
        [Tooltip("出現させたい敵Prefab")]
        public GameObject prefab;

        [Tooltip("出やすさ（比率）。大きいほど抽選に当たりやすい")]
        [Min(1)]
        public int weight = 1;
    }

    // =========================================================
    // Player Auto Find
    // =========================================================

    [Header("Player Auto Find")]
    [Tooltip("自動検索に使うPlayerタグ名")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("未設定なら Playerタグのオブジェクトを自動取得する")]
    [SerializeField] private Transform player;

    // =========================================================
    // Spawn X（横方向の生成ルール）
    // =========================================================

    [Header("Spawn X")]
    [Tooltip("プレイヤーよりどれくらい前に敵を出すか（ワールド距離）")]
    [SerializeField] private Vector2 spawnAhead = new Vector2(15f, 25f);

    [Tooltip("次の敵が出るまでに、プレイヤーが進む距離")]
    [SerializeField] private Vector2 spawnInterval = new Vector2(6f, 12f);

    // =========================================================
    // Ground Enemy（地上敵）
    // =========================================================

    [Header("Ground Spawn")]
    [Tooltip("地上敵の生成Y座標（ワールド）")]
    [SerializeField] private float groundY = 0f;

    [Tooltip("地上敵の候補リスト")]
    [SerializeField] private WeightedPrefab[] groundEnemies;

    // =========================================================
    // Air Enemy（空中敵）
    // =========================================================

    [Header("Air Spawn")]
    [Tooltip("空中敵の生成Y範囲（初期位置のみ）")]
    [SerializeField] private Vector2 airYRange = new Vector2(2.5f, 5.5f);

    [Tooltip("空中敵の候補リスト")]
    [SerializeField] private WeightedPrefab[] airEnemies;

    // =========================================================
    // Air Enemy Motion（空中敵の動き）
    // =========================================================

    [Header("Air Patterns")]
    [Tooltip("空中敵が取り得る移動パターン")]
    [SerializeField]
    private AirEnemyMover.Pattern[] airPatterns =
    {
        AirEnemyMover.Pattern.SineUpDown,
        AirEnemyMover.Pattern.ZigZag,
        AirEnemyMover.Pattern.Circle
    };

    [Tooltip("上下ホバーの振れ幅（小さいほど控えめ）")]
    [SerializeField] private Vector2 airAmpRange = new Vector2(0.4f, 1.2f);

    [Tooltip("上下ホバーの速さ（小さいほどゆっくり）")]
    [SerializeField] private Vector2 airFreqRange = new Vector2(0.8f, 2.0f);

    [Tooltip("横方向への流れ速度（0なら上下のみ）")]
    [SerializeField] private Vector2 airHSpeedRange = new Vector2(0.0f, 1.5f);

    // =========================================================
    // Mix（地上 / 空中の比率）
    // =========================================================

    [Header("Mix")]
    [Tooltip("空中敵が選ばれる確率（0〜1）")]
    [Range(0, 1)]
    [SerializeField] private float airChance = 0.45f;

    // 次に敵を出すX座標（内部状態）
    private float nextSpawnX;

    // =========================================================
    // Unity Lifecycle
    // =========================================================

    void Awake()
    {
        // Inspectorで指定されていなければ Playerタグから自動取得
        if (player == null)
        {
            var go = GameObject.FindWithTag(playerTag);
            if (go != null) player = go.transform;
        }
    }

    void Start()
    {
        // Playerが見つからなければ動作不能なので停止
        if (player == null)
        {
            Debug.LogError(
                $"TitleEnemySpawner: player が見つかりません（Tag='{playerTag}' を確認）",
                this
            );
            enabled = false;
            return;
        }

        // 最初のスポーン位置を設定
        nextSpawnX =
            player.position.x +
            UnityEngine.Random.Range(spawnInterval.x, spawnInterval.y);
    }

    void Update()
    {
        // Playerが後から有効化された場合の保険
        if (player == null)
        {
            var go = GameObject.FindWithTag(playerTag);
            if (go != null)
            {
                player = go.transform;
                nextSpawnX =
                    player.position.x +
                    UnityEngine.Random.Range(spawnInterval.x, spawnInterval.y);
            }
            return;
        }

        float px = player.position.x;

        // プレイヤーが nextSpawnX を超えたら敵を1体生成
        if (px >= nextSpawnX)
        {
            SpawnOne(px);

            // 次のスポーン位置を再設定
            nextSpawnX =
                px +
                UnityEngine.Random.Range(spawnInterval.x, spawnInterval.y);
        }
    }

    // =========================================================
    // Spawn Logic
    // =========================================================

    /// <summary>
    /// 敵を1体生成する
    /// ・空中 or 地上は airChance によって決定
    /// </summary>
    private void SpawnOne(float px)
    {
        bool spawnAir = UnityEngine.Random.value < airChance;

        // プレイヤーの前方に生成
        float sx = px + UnityEngine.Random.Range(spawnAhead.x, spawnAhead.y);

        if (spawnAir && airEnemies != null && airEnemies.Length > 0)
        {
            // 空中敵
            float sy = UnityEngine.Random.Range(airYRange.x, airYRange.y);

            var prefab = PickWeighted(airEnemies);
            if (prefab == null) return;

            var go = Instantiate(prefab, new Vector3(sx, sy, 0f), Quaternion.identity);

            // 空中敵の動きをランダム化
            var mover = go.GetComponent<AirEnemyMover>();
            if (mover != null)
            {
                mover.Randomize(
                    airPatterns,
                    airAmpRange,
                    airFreqRange,
                    airHSpeedRange
                );
            }
        }
        else if (groundEnemies != null && groundEnemies.Length > 0)
        {
            // 地上敵
            var prefab = PickWeighted(groundEnemies);
            if (prefab == null) return;

            Instantiate(prefab, new Vector3(sx, groundY, 0f), Quaternion.identity);
        }
    }

    // =========================================================
    // Utility
    // =========================================================

    /// <summary>
    /// weight（重み）に基づいてPrefabを1つ抽選する
    /// </summary>
    private GameObject PickWeighted(WeightedPrefab[] list)
    {
        int total = 0;

        // 重みの合計を計算
        foreach (var e in list)
            if (e.prefab != null)
                total += Mathf.Max(1, e.weight);

        if (total <= 0) return null;

        // 0〜total の乱数を引く
        int r = UnityEngine.Random.Range(0, total);

        foreach (var e in list)
        {
            if (e.prefab == null) continue;

            r -= Mathf.Max(1, e.weight);
            if (r < 0)
                return e.prefab;
        }

        // 念のための保険
        for (int i = 0; i < list.Length; i++)
            if (list[i].prefab != null)
                return list[i].prefab;

        return null;
    }
}
