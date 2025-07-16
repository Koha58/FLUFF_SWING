using System.Collections;
using TMPro;
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
    #region 針とワイヤー関連

    // 針孔位置用の子オブジェクト。ワイヤーの始点となる。
    [SerializeField] private Transform needlePivot;

    // 針オブジェクト。プレイヤーから発射され、地面に刺さる。
    [SerializeField] private GameObject needle;

    // LineRenderer はこのスクリプトが直接操作する。ワイヤーの描画に使用。
    private LineRenderer lineRenderer;

    // 針のRendererキャッシュ（表示切り替え用）
    private Renderer needleRenderer;

    // 針が刺さった位置（外部から読み取り専用）。ワイヤーの終点となる。
    private Vector2 _hookedPosition;
    public Vector2 HookedPosition => _hookedPosition;

    #endregion

    #region 接続・物理関連

    // 物理的なワイヤーの役割を果たすコンポーネント
    private DistanceJoint2D distanceJoint;

    // 接続対象のオブジェクト（針が刺さったTilemapなど）
    private GameObject targetObject = null;

    // 接続状態を外部から確認できるようにする
    public bool IsConnected => distanceJoint.enabled;

    #endregion

    #region プレイヤー・アニメーション関連

    // プレイヤーからの入力を受け取るハンドラ
    [SerializeField] private WireInputHandler inputHandler;

    // アニメーションの始点として使用するプレイヤーの右手位置
    [SerializeField] private Transform rightHandTransform;

    // アニメーション制御スクリプト
    [SerializeField] private PlayerAnimatorController animatorController;

    private const float SwingAnimationStopDelay = 0.2f;

    /// <summary>
    /// ワイヤー切断後、スイングアニメーションを少し遅延させて停止させるコルーチン。
    /// </summary>
    private IEnumerator DelayedStopSwingAnimation(float directionX)
    {
        yield return new WaitForSeconds(SwingAnimationStopDelay);
        animatorController.StopSwingAnimation(directionX);
    }

    #endregion

    #region ライン描画・予測線

    // 予測線（点線）
    [SerializeField] private LineRenderer previewLineRenderer;

    // ワイヤーのカーブ頂点数
    private const int WireSegmentCount = 10;

    #endregion

    #region コルーチン管理

    // 現在進行中の針移動コルーチン
    private Coroutine currentNeedleCoroutine;

    #endregion

    #region 内部状態保持・定数

    [SerializeField] private PlayerWireConfig config;
    private float lastSwingDirectionX = 0f;
    private const float NeedleStopDistance = 0.01f;
    private float needleSpeed;
    private float fixedWireLength;
    private float playerGravityScale;
    private float rigidbodyLinearDamping;
    private float rigidbodyAngularDamping;
    private float swingInitialSpeed;

    #endregion


    private void Awake()
    {
        // 必要なコンポーネントを取得
        lineRenderer = GetComponent<LineRenderer>();
        distanceJoint = GetComponent<DistanceJoint2D>();
        needleRenderer = needle.GetComponent<Renderer>();
        animatorController = GetComponent<PlayerAnimatorController>();

        // configから各種パラメータを取得
        needleSpeed = config.needleSpeed;
        fixedWireLength = config.fixedWireLength;
        playerGravityScale = config.playerGravityScale;
        rigidbodyLinearDamping = config.linearDamping;
        rigidbodyAngularDamping = config.angularDamping;
        swingInitialSpeed = config.swingInitialSpeed;
    }

    private void Start()
    {
        // Start()では何もしない（ライフサイクルに合わせてOnEnableで初期化する）
    }

    void Update()
    {
        // 接続前にプレイヤーからマウス位置までの予測ライン（点線）を描画
        UpdatePreviewLine();

        // ワイヤー接続中、カーブワイヤーのラインを更新して表示
        if (IsConnected)
        {
            UpdateBezierWireLine();
        }
    }

    private void OnEnable()
    {
        // イベント登録
        inputHandler.OnLeftClick += HandleLeftClick;
        inputHandler.OnRightClick += HandleRightClick;

        // オブジェクトが有効になった時点で状態をリセット
        ResetState();
    }

    private void OnDisable()
    {
        // イベント解除
        inputHandler.OnLeftClick -= HandleLeftClick;
        inputHandler.OnRightClick -= HandleRightClick;
    }

    /// <summary>
    /// スクリプトの状態を完全にリセットし、初期状態に戻す
    /// </summary>
    public void ResetState()
    {
        // 既存のワイヤーを切断
        CutWire();

        // 針の見た目を確実に非表示に
        SetNeedleVisible(false);

        // 予測線も確実に非表示に
        if (previewLineRenderer != null)
        {
            previewLineRenderer.enabled = false;
            previewLineRenderer.positionCount = 0;
        }

        // 実行中の針発射コルーチンがあれば停止し、参照をクリア
        if (currentNeedleCoroutine != null)
        {
            StopCoroutine(currentNeedleCoroutine);
        }
        currentNeedleCoroutine = null;

        // 最後にスイングした方向もリセット
        lastSwingDirectionX = 0f;
    }

    /// <summary>
    /// 左クリックの入力を処理し、ワイヤーを接続する
    /// </summary>
    private void HandleLeftClick()
    {
        // 接続中なら無視
        if (IsConnected)
            return;

        // マウスのワールド座標を取得
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // マウス座標で2Dレイキャスト
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        if (hit.collider == null) return;

        // ヒットしたオブジェクトから Tilemap を取得
        Tilemap tilemap = hit.collider.GetComponent<Tilemap>() ?? hit.collider.GetComponentInParent<Tilemap>();
        if (tilemap == null) return;

        // ヒットした位置のタイル座標を取得
        Vector3Int cellPos = tilemap.WorldToCell(hit.point);

        // 該当のタイルを取得
        TileBase tile = tilemap.GetTile(cellPos);

        // Ground タイプのカスタムタイルなら接続処理を行う
        if (tile is CustomTile customTile && customTile.tileType == CustomTile.TileType.Ground)
        {
            Vector2 adjustedTarget = FindSurfaceAlongPlayerDirectionTilemap(hit.point);
            TryConnectWire(adjustedTarget, hit.collider.gameObject);
        }
        else if (tile is ITileWithType tileWithType && tileWithType.tileType == CustomTile.TileType.Ground)
        {
            Vector2 adjustedTarget = FindSurfaceAlongPlayerDirectionTilemap(hit.point);
            TryConnectWire(adjustedTarget, hit.collider.gameObject);
        }
    }

    /// <summary>
    /// 右クリックの入力を処理し、ワイヤーを切断する
    /// </summary>
    private void HandleRightClick()
    {
        if (IsConnected)
        {
            CutWire();
        }
    }

    /// <summary>
    /// ワイヤー接続を試みる
    /// </summary>
    private void TryConnectWire(Vector2 targetPos, GameObject hitObject)
    {
        // 接続中 or 針を飛ばしている最中は無視
        if (IsConnected || currentNeedleCoroutine != null)
            return;

        SetNeedleVisible(true);
        currentNeedleCoroutine = StartCoroutine(ThrowNeedle(targetPos, hitObject));
    }

    /// <summary>
    /// ワイヤーを切断し、物理的な接続と描画を解除する
    /// </summary>
    public void CutWire()
    {
        // DistanceJoint2D を無効化してワイヤー接続を解除
        if (distanceJoint != null)
        {
            distanceJoint.enabled = false;
        }
        targetObject = null;

        // LineRendererを無効化
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }

        SetNeedleVisible(false);

        // 実行中の針発射コルーチンがあれば停止し、参照をクリア
        if (currentNeedleCoroutine != null)
        {
            StopCoroutine(currentNeedleCoroutine);
            currentNeedleCoroutine = null;
        }

        // アニメーションの停止処理
        animatorController.CancelPendingIdleTransition();
        animatorController.StartCoroutine(DelayedStopSwingAnimation(lastSwingDirectionX));
        animatorController?.UpdateJumpState(lastSwingDirectionX);

        Debug.Log("ワイヤーを切断しました");
    }

    /// <summary>
    /// 針を目標位置まで飛ばし、ワイヤーを接続するコルーチン
    /// </summary>
    private IEnumerator ThrowNeedle(Vector2 targetPosition, GameObject hitObject)
    {
        SetNeedleVisible(true);
        // 針の発射位置をプレイヤーの位置に設定
        needle.transform.position = transform.position;

        // 針が目標に到達するまで移動
        while (Vector2.Distance(needle.transform.position, targetPosition) > NeedleStopDistance)
        {
            Vector2 direction = (targetPosition - (Vector2)needle.transform.position).normalized;
            // 針の向きを進行方向に合わせる
            needle.transform.up = -direction;
            needle.transform.position = Vector2.MoveTowards(needle.transform.position, targetPosition, needleSpeed * Time.deltaTime);
            yield return null;
        }

        // 針が目標に刺さった位置を固定
        needle.transform.position = targetPosition;
        targetObject = hitObject;
        _hookedPosition = targetPosition;

        // LineRendererを有効化
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }

        // DistanceJoint2Dの設定
        if (distanceJoint != null)
        {
            distanceJoint.enabled = false; // 一時的に無効化
            distanceJoint.connectedBody = null;
            distanceJoint.connectedAnchor = _hookedPosition;
            distanceJoint.maxDistanceOnly = true;
            distanceJoint.distance = fixedWireLength;
            distanceJoint.enabled = true; // 再び有効化して接続完了
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 物理パラメータをスイング用に調整
            rb.gravityScale = playerGravityScale;
            rb.linearDamping = rigidbodyLinearDamping;
            rb.angularDamping = rigidbodyAngularDamping;

            // スイングの初期速度を計算し、適用
            Vector2 dir = (_hookedPosition - (Vector2)transform.position).normalized;
            Vector2 tangent = new Vector2(-dir.y, dir.x);
            tangent = (lastSwingDirectionX >= 0) ? tangent : -tangent;

            // Rigidbody2D.velocity を Rigidbody2D.linearVelocity に修正
            rb.linearVelocity = tangent * swingInitialSpeed;
        }

        // アニメーションを再生
        Vector2 dirForAnimation = (_hookedPosition - (Vector2)transform.position).normalized;
        lastSwingDirectionX = dirForAnimation.x;
        animatorController.PlayGrappleSwingAnimation(dirForAnimation.x);
        currentNeedleCoroutine = null;
    }

    /// <summary>
    /// ワイヤー接続中にベジエ曲線を描画するメソッド。
    /// </summary>
    private void UpdateBezierWireLine()
    {
        // ワイヤーが未接続なら描画しない
        if (lineRenderer == null || !distanceJoint.enabled) return;

        lineRenderer.positionCount = WireSegmentCount + 1;

        Vector3 start = rightHandTransform.position; // プレイヤーの手を始点にする
        Vector3 end = needlePivot.position;          // 地面に刺さった針孔の位置

        // ベジェ曲線の制御点（中間の膨らみ）
        Vector3 controlPoint = (start + end) / 2 + Vector3.down * 1.5f;

        for (int i = 0; i <= WireSegmentCount; i++)
        {
            float t = i / (float)WireSegmentCount;
            Vector3 point = Mathf.Pow(1 - t, 2) * start + 2 * (1 - t) * t * controlPoint + Mathf.Pow(t, 2) * end;
            lineRenderer.SetPosition(i, point);
        }
    }


    /// <summary>
    /// ワイヤー発射前の予測線を描画するメソッド。
    /// </summary>
    private void UpdatePreviewLine()
    {
        // ワイヤー接続中は予測ライン非表示
        if (needleRenderer != null && needleRenderer.enabled)
        {
            if (previewLineRenderer != null)
            {
                previewLineRenderer.enabled = false;
            }
            return;
        }

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        // 地面レイヤーとの衝突判定（ゼロ距離Raycast）
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, LayerMask.GetMask("Ground"));

        if (hit.collider != null)
        {
            Vector2 adjustedTarget = FindSurfaceAlongPlayerDirectionTilemap(hit.point);
            if (previewLineRenderer != null)
            {
                previewLineRenderer.positionCount = 2;
                previewLineRenderer.SetPosition(0, transform.position); // 予測線の始点
                previewLineRenderer.SetPosition(1, adjustedTarget); // 予測線の終点
                previewLineRenderer.enabled = true;
            }
        }
        else
        {
            if (previewLineRenderer != null)
            {
                previewLineRenderer.enabled = false;
            }
        }
    }

    /// <summary>
    /// 針の表示/非表示を切り替える
    /// </summary>
    private void SetNeedleVisible(bool visible)
    {
        if (needleRenderer != null)
            needleRenderer.enabled = visible;
    }

    /// <summary>
    /// マウスのスクリーン座標をワールド座標に変換する
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    /// <summary>
    /// プレイヤーからクリック位置に向かって、地面の正確な表面を探索する。
    /// Tilemapの内側から外側への境界を見つけることで、ワイヤーの刺さる位置を正確にする。
    /// </summary>
    private Vector2 FindSurfaceAlongPlayerDirectionTilemap(Vector2 clickPosition)
    {
        Vector2 playerPos = transform.position;
        Vector2 directionToPlayer = (playerPos - clickPosition).normalized;
        Vector2 probePosition = clickPosition;
        Vector2 lastInsidePosition = clickPosition;
        bool wasInside = true;
        bool foundSurface = false;

        // Tilemap を取得（コライダーから探索）
        RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);
        if (hit.collider == null) return clickPosition;

        Tilemap tilemap = hit.collider.GetComponent<Tilemap>() ?? hit.collider.GetComponentInParent<Tilemap>();
        if (tilemap == null) return clickPosition;

        // Tilemap 内から外への境界を探索
        for (int i = 0; i < 50; i++)
        {
            Vector3Int cellPos = tilemap.WorldToCell(probePosition);
            TileBase tile = tilemap.GetTile(cellPos);
            bool isInside = (tile != null);

            if (wasInside && !isInside)
            {
                foundSurface = true;
                break;
            }

            if (isInside)
                lastInsidePosition = probePosition;

            probePosition += directionToPlayer * 0.1f;
            wasInside = isInside;
        }

        if (foundSurface)
        {
            return lastInsidePosition;
        }
        else
        {
            return clickPosition;
        }
    }
}