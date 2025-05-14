using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// プレイヤーがTilemap上のGroundタイルに対して、ワイヤー（LineRenderer + DistanceJoint2D）を使って接続・スイングできるアクションを制御するスクリプト。
/// マウス左クリックで接続、右クリックでワイヤーを切断、針オブジェクトが目標地点へ移動してからワイヤーを張る。
/// </summary>
public class WireActionScript : MonoBehaviour
{
    [Header("ワイヤー先端に使う針オブジェクト")]
    [SerializeField] private GameObject needle;

    private GameObject targetObject = null;

    // LineRenderer コンポーネント（ワイヤーの見た目用）
    private LineRenderer lineRenderer => GetComponent<LineRenderer>();

    // DistanceJoint2D コンポーネント（物理的な接続）
    private DistanceJoint2D distanceJoint => GetComponent<DistanceJoint2D>();

    // 現在の針移動コルーチン
    private Coroutine currentNeedleCoroutine;

    // === 定数（マジックナンバー排除） ===

    // ワイヤー接続時、現在地点からの距離に掛ける係数（90%）
    private const float DISTANCE_ATTENUATION_RATIO = 0.9f; // 少し短くして安定性を上げる

    // 距離差がこの値未満だと再接続を無視する（無駄な接続防止）
    private const float RECONNECT_DISTANCE_THRESHOLD = 0.3f;

    // 針が到達したと判定するしきい値（目的地との距離）
    private const float NEEDLE_STOP_DISTANCE = 0.01f;

    // 針が移動する速度（1フレームあたり）
    private const float NEEDLE_SPEED = 0.2f;

    // DistanceJoint2D に設定する距離の係数（少し余裕を持たせて揺れやすく）
    private const float JOINT_DISTANCE_RATIO = 0.6f;

    // プレイヤーに与えるスイング開始時の力
    private const float SWING_FORCE = 300f;

    // Rigidbody2D の重力倍率（スイングの挙動に影響）
    private const float PLAYER_GRAVITY_SCALE = 3f;

    // スイング中の減速を抑える設定
    private const float RIGIDBODY_LINEAR_DAMPING = 0f;
    private const float RIGIDBODY_ANGULAR_DAMPING = 0f;

    // LineRendererの頂点数（常に始点と終点の2つ）
    private const int LINE_RENDERER_POINT_COUNT = 2;

    // 固定ワイヤー長さ（ワイヤーの長さを一定に保つ）
    private const float FIXED_WIRE_LENGTH = 5f; // ワイヤーの長さを5に固定

    void Update()
    {
        HandleLeftClick();   // 接続処理
        HandleRightClick();  // 切断処理
        UpdateLine();        // 線の見た目更新
    }

    /// <summary>
    /// マウス左クリック時の処理。対象がGroundタイルであればワイヤーを接続。
    /// </summary>
    private void HandleLeftClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        if (hit.collider == null) return;

        Tilemap tilemap = hit.collider.GetComponent<Tilemap>() ?? hit.collider.GetComponentInParent<Tilemap>();
        if (tilemap == null) return;

        Vector3Int cellPos = tilemap.WorldToCell(hit.point);
        TileBase tile = tilemap.GetTile(cellPos);

        if (tile is CustomTile customTile && customTile.tileType == CustomTile.TileType.Ground)
        {
            TryConnectWire(hit.point, hit.collider.gameObject);
        }
    }

    /// <summary>
    /// マウス右クリックでワイヤーを切断する
    /// </summary>
    private void HandleRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            CutWire();
        }
    }

    /// <summary>
    /// LineRendererの位置をプレイヤーと接続先に合わせて更新
    /// </summary>
    private void UpdateLine()
    {
        if (distanceJoint.enabled && lineRenderer.positionCount >= LINE_RENDERER_POINT_COUNT)
        {
            lineRenderer.SetPosition(0, transform.position);                          // プレイヤーの位置
            lineRenderer.SetPosition(1, distanceJoint.connectedAnchor);              // 接続先
        }
    }

    /// <summary>
    /// ワイヤーを接続する処理。一定距離以上離れていないと再接続しない。
    /// </summary>
    private void TryConnectWire(Vector2 targetPos, GameObject hitObject)
    {
        // 固定の距離を設定して再接続を防止
        float newDistance = FIXED_WIRE_LENGTH;

        // 既存の接続距離と新しい距離が近すぎる場合は再接続を防止
        if (distanceJoint.enabled && distanceJoint.connectedAnchor != Vector2.zero)
        {
            float currentDistance = Vector2.Distance(transform.position, distanceJoint.connectedAnchor);
            if (Mathf.Abs(newDistance - currentDistance) < RECONNECT_DISTANCE_THRESHOLD)
            {
                Debug.Log("距離が近いため再接続をスキップ");
                return;
            }
        }

        // 既存の針コルーチンを止める
        if (currentNeedleCoroutine != null)
            StopCoroutine(currentNeedleCoroutine);

        // 新しい針の発射処理開始
        currentNeedleCoroutine = StartCoroutine(ThrowNeedle(targetPos, hitObject));
    }

    /// <summary>
    /// ワイヤーを解除し、LineRendererを非表示にする。
    /// </summary>
    private void CutWire()
    {
        distanceJoint.enabled = false;
        lineRenderer.positionCount = 0;
        targetObject = null;
        Debug.Log("ワイヤーを切断しました");
    }

    /// <summary>
    /// 針を目的地に飛ばし、ワイヤーを張るコルーチン。
    /// </summary>
    private IEnumerator ThrowNeedle(Vector2 targetPosition, GameObject hitObject)
    {
        // 針を目的地に向かって移動させる
        while (Vector2.Distance(needle.transform.position, targetPosition) > NEEDLE_STOP_DISTANCE)
        {
            needle.transform.position = Vector2.MoveTowards(needle.transform.position, targetPosition, NEEDLE_SPEED);
            yield return null;
        }

        // 針到達後の処理
        needle.transform.position = targetPosition;
        targetObject = hitObject;

        DrawLine();

        // 固定の距離を設定
        distanceJoint.distance = FIXED_WIRE_LENGTH;

        // 最大距離のみを制限する設定を有効化
        distanceJoint.maxDistanceOnly = true;
        distanceJoint.connectedAnchor = targetPosition;
        distanceJoint.connectedBody = null;
        distanceJoint.enabled = true;

        // スイング開始時の初速を設定
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = PLAYER_GRAVITY_SCALE;
        rb.linearDamping = RIGIDBODY_LINEAR_DAMPING;
        rb.angularDamping = RIGIDBODY_ANGULAR_DAMPING;

        // 接続点に向かって横方向に初速を与える（揺れ始めのきっかけ）
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        Vector2 tangent = new Vector2(-dir.y, dir.x); // 接続方向に垂直な方向
        rb.AddForce(tangent * SWING_FORCE);
    }

    /// <summary>
    /// LineRenderer の線を引く（針が対象に到達したタイミングで）
    /// </summary>
    private void DrawLine()
    {
        if (targetObject == null) return;

        lineRenderer.positionCount = LINE_RENDERER_POINT_COUNT;
        lineRenderer.SetPosition(0, transform.position);                  // プレイヤー位置
        lineRenderer.SetPosition(1, targetObject.transform.position);     // 針の位置
    }

    /// <summary>
    /// カメラを考慮してマウス位置をワールド座標に変換
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
