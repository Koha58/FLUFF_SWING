using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// プレイヤーがワイヤーを使って地形（Tilemap）に接続し、スイング移動を行うアクションを制御するクラス。
/// 左クリックで接続可能地点を指定し、針を飛ばして接続。
/// 右クリックでワイヤーを切断。
/// 接続中は固定長のワイヤーで物理的に接続され、接続直後にスイング開始の力が加わる。
/// </summary>
public class WireActionScript : MonoBehaviour
{
    [SerializeField] private GameObject needle;

    // 接続対象のオブジェクト
    private GameObject targetObject = null;

    // ワイヤーの見た目を担当する LineRenderer コンポーネント
    private LineRenderer lineRenderer => GetComponent<LineRenderer>();

    // プレイヤーを接続する物理ジョイント（距離固定）
    private DistanceJoint2D distanceJoint => GetComponent<DistanceJoint2D>();

    // 現在進行中の針移動コルーチン（複数同時に動かさないため管理）
    private Coroutine currentNeedleCoroutine;

    #region 定数
    private const float NEEDLE_STOP_DISTANCE = 0.01f;  // 針停止の判定距離
    private const float NEEDLE_SPEED = 0.2f;           // 針の移動速度
    private const float SWING_FORCE = 300f;            // スイング開始時に加える力
    private const float PLAYER_GRAVITY_SCALE = 3f;     // 接続時の重力スケール
    private const float RIGIDBODY_LINEAR_DAMPING = 0f; // 空気抵抗
    private const float RIGIDBODY_ANGULAR_DAMPING = 0f;// 回転減衰
    private const int LINE_RENDERER_POINT_COUNT = 2;   // ラインの点数
    private const float FIXED_WIRE_LENGTH = 3.5f;      // ワイヤーの固定長さ
    private const int LINE_START_INDEX = 0;            // ラインの始点インデックス
    private const int LINE_END_INDEX = 1;              // ラインの終点インデックス
    private const int LINE_POINT_NONE = 0;             // ライン非表示時の点数
    #endregion

    void Update()
    {
        HandleLeftClick();   // 左クリック：接続処理
        HandleRightClick();  // 右クリック：切断処理
        UpdateLine();        // 常にワイヤーの見た目を更新
    }

    /// <summary>
    /// 左クリック時の接続処理。
    /// クリック位置が Ground タイルであれば、ワイヤーを接続する。
    /// </summary>
    private void HandleLeftClick()
    {
        // 左クリックが押されていなければ何もしない
        if (!Input.GetMouseButtonDown(0)) return;

        // マウスのワールド座標を取得
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // マウス座標で2Dレイキャスト（その座標にオブジェクトが存在するか確認）
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        if (hit.collider == null) return; // ヒットしなければ何もしない

        // ヒットしたオブジェクトから Tilemap を取得（TilemapCollider2D の場合も想定し親も確認）
        Tilemap tilemap = hit.collider.GetComponent<Tilemap>() ?? hit.collider.GetComponentInParent<Tilemap>();
        if (tilemap == null) return; // Tilemap でなければ何もしない

        // ヒットした位置のタイル座標を取得
        Vector3Int cellPos = tilemap.WorldToCell(hit.point);

        // 該当のタイルを取得
        TileBase tile = tilemap.GetTile(cellPos);

        // Ground タイプのカスタムタイルなら接続処理を行う
        if (tile is CustomTile customTile && customTile.tileType == CustomTile.TileType.Ground)
        {
            TryConnectWire(hit.point, hit.collider.gameObject);
        }
    }

    /// <summary>
    /// 右クリック時、ワイヤーを切断する。
    /// </summary>
    private void HandleRightClick()
    {
        // 右クリックが押されたらワイヤーを切断
        if (Input.GetMouseButtonDown(1))
        {
            CutWire();
        }
    }

    /// <summary>
    /// ワイヤーの見た目（LineRenderer）を更新する。
    /// </summary>
    private void UpdateLine()
    {
        // ジョイントが有効かつ LineRenderer が最低限の点数を持っている場合のみ更新
        if (distanceJoint.enabled && lineRenderer.positionCount >= LINE_RENDERER_POINT_COUNT)
        {
            // 始点はプレイヤー（自分）
            lineRenderer.SetPosition(LINE_START_INDEX, transform.position);

            // 終点はジョイントの接続アンカー（接続座標）
            lineRenderer.SetPosition(LINE_END_INDEX, distanceJoint.connectedAnchor);
        }
    }

    /// <summary>
    /// ワイヤー接続要求。
    /// 同じ地点に接続済みの場合は処理をスキップ。
    /// 針を飛ばすコルーチンを開始。
    /// </summary>
    private void TryConnectWire(Vector2 targetPos, GameObject hitObject)
    {
        // 既に接続中なら同じターゲットか確認
        if (distanceJoint.enabled && distanceJoint.connectedAnchor != Vector2.zero)
        {
            bool isSameTarget = (Vector2.Distance(distanceJoint.connectedAnchor, targetPos) < 0.01f);
            if (isSameTarget)
            {
                // 同じ場所ならスキップ（無駄な接続を避ける）
                Debug.Log("同じ場所に既に接続中のためスキップ");
                return;
            }
        }

        // 既存の針コルーチンを停止（複数同時起動防止）
        if (currentNeedleCoroutine != null)
            StopCoroutine(currentNeedleCoroutine);

        // 新しい針を飛ばすコルーチンを開始
        currentNeedleCoroutine = StartCoroutine(ThrowNeedle(targetPos, hitObject));
    }

    /// <summary>
    /// ワイヤーを切断。
    /// </summary>
    private void CutWire()
    {
        // ジョイントを無効化
        distanceJoint.enabled = false;

        // ワイヤーの見た目も非表示
        lineRenderer.positionCount = LINE_POINT_NONE;

        // 接続対象もリセット
        targetObject = null;

        Debug.Log("ワイヤーを切断しました");
    }

    /// <summary>
    /// 針をターゲット位置まで移動し、到達したらワイヤー接続を行うコルーチン。
    /// </summary>
    private IEnumerator ThrowNeedle(Vector2 targetPosition, GameObject hitObject)
    {
        // プレイヤー位置
        Vector2 playerPosition = transform.position;

        // プレイヤー → ターゲット 方向
        Vector2 directionToTarget = (targetPosition - playerPosition).normalized;

        // プレイヤーから見て反対方向（ターゲット方向の逆）
        Vector2 directionOpposite = -directionToTarget;

        // 針をプレイヤー位置にセット
        needle.transform.position = playerPosition;

        // 針をターゲットとは逆方向に一定距離離す（演出用、必要なら）
        float initialOffset = 0.5f; // 任意の距離
        needle.transform.position = playerPosition + directionOpposite * initialOffset;

        // 針をターゲット方向に移動（見た目上は逆からターゲットに向かってくる）
        while (Vector2.Distance(needle.transform.position, targetPosition) > NEEDLE_STOP_DISTANCE)
        {
            needle.transform.position = Vector2.MoveTowards(needle.transform.position, targetPosition, NEEDLE_SPEED);
            yield return null;
        }

        // 針をぴったりターゲット位置に配置
        needle.transform.position = targetPosition;

        // あとは従来通りワイヤー接続
        targetObject = hitObject;
        DrawLine();

        distanceJoint.enabled = false;
        distanceJoint.connectedBody = null;
        distanceJoint.connectedAnchor = targetPosition;
        distanceJoint.maxDistanceOnly = true;
        distanceJoint.distance = FIXED_WIRE_LENGTH;
        distanceJoint.enabled = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = PLAYER_GRAVITY_SCALE;
        rb.linearDamping = RIGIDBODY_LINEAR_DAMPING;
        rb.angularDamping = RIGIDBODY_ANGULAR_DAMPING;

        Vector2 dir = (targetPosition - playerPosition).normalized;
        Vector2 tangent = new Vector2(-dir.y, dir.x);
        rb.AddForce(tangent * SWING_FORCE);
    }


    /// <summary>
    /// ワイヤーの見た目を LineRenderer で描画。
    /// </summary>
    private void DrawLine()
    {
        if (targetObject == null) return; // 接続対象が無ければ描画しない

        // LineRenderer の点数をセット
        lineRenderer.positionCount = LINE_RENDERER_POINT_COUNT;

        // 始点はプレイヤー
        lineRenderer.SetPosition(LINE_START_INDEX, transform.position);

        // 終点はターゲットオブジェクトの位置
        lineRenderer.SetPosition(LINE_END_INDEX, targetObject.transform.position);
    }

    /// <summary>
    /// マウス位置をワールド座標で取得。
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        // マウス座標をスクリーン座標から取得
        Vector3 mousePosition = Input.mousePosition;

        // カメラの位置補正（2DカメラなのでZ軸を調整）
        mousePosition.z = -Camera.main.transform.position.z;

        // スクリーン座標からワールド座標に変換
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
