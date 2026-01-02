using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 平坦な地面を無限に生成するためのタイルマップ制御クラス。
/// 
/// ・プレイヤーの位置を基準に、前方に地面を生成／後方を削除する
/// ・地面／草／装飾（待ち針）をレイヤー別Tilemapで管理
/// ・当たり判定や物理は一切考慮しない（見た目専用）
/// 
/// 主な用途：
/// ・タイトル画面
/// ・エンドレスランの背景生成
/// </summary>
public class FlatInfiniteGround : MonoBehaviour
{
    // =========================================================
    // Player Reference
    // =========================================================

    [Header("Player")]
    [Tooltip("基準となるプレイヤーTransform")]
    [SerializeField] private Transform player;

    // =========================================================
    // Tilemaps
    // =========================================================

    [Header("Tilemaps")]
    [Tooltip("地面用Tilemap")]
    [SerializeField] private Tilemap groundMap;

    [Tooltip("草など、地面上レイヤー用Tilemap")]
    [SerializeField] private Tilemap grassMap;

    [Header("Pins (Layer Random)")]
    [Tooltip("装飾を前面に表示するTilemap")]
    [SerializeField] private Tilemap pinsFrontMap;

    [Tooltip("装飾を背面に表示するTilemap")]
    [SerializeField] private Tilemap pinsBackMap;

    // =========================================================
    // Tiles
    // =========================================================

    [Header("Tiles")]
    [Tooltip("地面タイル")]
    [SerializeField] private TileBase groundTile;

    [Tooltip("草タイル（地面の上に重ねる）")]
    [SerializeField] private TileBase grassTile;

    // =========================================================
    // Pins (Decorations)
    // =========================================================

    [Header("Pins (地面に刺さる装飾)")]
    [Tooltip("待ち針などの装飾タイル（複数指定可）")]
    [SerializeField] private TileBase[] pinTiles;

    [Tooltip("1セルごとの出現確率")]
    [Range(0, 1)]
    [SerializeField] private float pinChance = 0.08f;

    [Tooltip("装飾を置くY位置（セル座標）")]
    [SerializeField] private int pinY = 0;

    [Tooltip("装飾の最小間隔（セル）")]
    [SerializeField] private int pinXSpacingMin = 6;

    [Tooltip("装飾の最大間隔（セル）")]
    [SerializeField] private int pinXSpacingMax = 14;

    [Tooltip("装飾が前面レイヤーに出る確率")]
    [Range(0, 1)]
    [SerializeField] private float pinFrontChance = 0.5f;

    // =========================================================
    // Ground Shape
    // =========================================================

    [Header("Ground")]
    [Tooltip("地面の上面Y（セル座標）")]
    [SerializeField] private int groundTopY = -4;

    [Tooltip("地面の縦マス数（2なら「地面/地面」）")]
    [Min(1)]
    [SerializeField] private int groundHeight = 2;

    // =========================================================
    // Generation Range
    // =========================================================

    [Header("Range (columns)")]
    [Tooltip("プレイヤー前方に生成するセル数")]
    [SerializeField] private int ahead = 120;

    [Tooltip("プレイヤー後方に残すセル数")]
    [SerializeField] private int behind = 40;

    // =========================================================
    // Internal State
    // =========================================================

    /// <summary>
    /// 生成済みの最右端X（セル）
    /// </summary>
    private int generatedToX;

    /// <summary>
    /// 削除済みの最右端X（セル）
    /// </summary>
    private int clearedToX;

    /// <summary>
    /// 次に装飾を置いてよいX位置
    /// （連続配置を防ぐため）
    /// </summary>
    private int nextPinAllowedX;

    // =========================================================
    // Unity Lifecycle
    // =========================================================

    void Start()
    {
        // プレイヤーの現在セルXを取得
        int px = groundMap.WorldToCell(player.position).x;

        // 初期状態では少し後ろから生成開始
        generatedToX = px - 10;
        clearedToX = px - behind - 20;

        // 装飾の配置開始地点
        nextPinAllowedX = px;

        // 初期生成
        GenerateUpTo(px + ahead);
    }

    void Update()
    {
        int px = groundMap.WorldToCell(player.position).x;

        // 前方生成
        GenerateUpTo(px + ahead);

        // 後方削除
        ClearUpTo(px - behind);
    }

    // =========================================================
    // Generation
    // =========================================================

    /// <summary>
    /// 指定Xセルまで地面・草・装飾を生成する
    /// </summary>
    private void GenerateUpTo(int targetX)
    {
        for (int x = generatedToX; x <= targetX; x++)
        {
            // 地面（縦に groundHeight 分敷く）
            for (int i = 0; i < groundHeight; i++)
            {
                int y = groundTopY - i;
                groundMap.SetTile(new Vector3Int(x, y, 0), groundTile);
            }

            // 草（地面最上段と同じセルに重ねる）
            grassMap.SetTile(new Vector3Int(x, groundTopY, 0), grassTile);

            // 装飾（待ち針など）
            TryPlacePin(x);

            // 次の生成位置を更新
            generatedToX = x + 1;
        }
    }

    /// <summary>
    /// 装飾（待ち針）をランダム条件で配置する
    /// </summary>
    private void TryPlacePin(int x)
    {
        if (pinTiles == null || pinTiles.Length == 0) return;

        // 間隔制御
        if (x < nextPinAllowedX) return;

        // 出現確率
        if (Random.value >= pinChance) return;

        var tile = pinTiles[Random.Range(0, pinTiles.Length)];

        // 前面／背面レイヤーをランダムで決定
        bool isFront = Random.value < pinFrontChance;
        Tilemap targetMap = isFront ? pinsFrontMap : pinsBackMap;

        if (targetMap != null)
        {
            targetMap.SetTile(new Vector3Int(x, pinY, 0), tile);
        }

        // 次に置けるXを更新
        nextPinAllowedX =
            x + Random.Range(pinXSpacingMin, pinXSpacingMax + 1);
    }

    // =========================================================
    // Cleanup
    // =========================================================

    /// <summary>
    /// 指定Xセルまで、後方のタイルを削除する
    /// </summary>
    private void ClearUpTo(int targetX)
    {
        for (int x = clearedToX; x <= targetX; x++)
        {
            // 地面削除
            for (int i = 0; i < groundHeight; i++)
            {
                int y = groundTopY - i;
                groundMap.SetTile(new Vector3Int(x, y, 0), null);
            }

            // 草削除
            grassMap.SetTile(new Vector3Int(x, groundTopY, 0), null);

            // 装飾削除（前後両レイヤー）
            if (pinsFrontMap != null)
                pinsFrontMap.SetTile(new Vector3Int(x, pinY, 0), null);

            if (pinsBackMap != null)
                pinsBackMap.SetTile(new Vector3Int(x, pinY, 0), null);

            // 次の削除位置を更新
            clearedToX = x + 1;
        }
    }
}
