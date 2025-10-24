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
    #region === 定数 ===

    // ワイヤー描画関連
    private const int WireSegmentCount = 10;              // ワイヤーのベジェ曲線分割数（滑らかな描画のため）
    private const float NeedleStopDistance = 0.01f;      // 針が目標座標に到達したとみなす距離閾値
    private const float SwingAnimationStopDelay = 0.2f;  // ワイヤー切断後、スイングアニメーションを停止するまでの遅延

    // タイル判定関連
    private const int MaxTileProbeSteps = 50;            // プレイヤー方向へタイル探索する最大ステップ数
    private const float TileProbeStepSize = 0.1f;        // タイル探索時の1ステップ距離
    private const float GroundCheckThreshold = 0.05f;    // "ほぼ接地"と判定する距離閾値

    // ワイヤー線の制御点
    private const float BezierControlOffsetY = 1.5f;     // ベジェ曲線の中間制御点のY方向オフセット

    // PreviewLineRenderer（ワイヤー接続予測線）関連
    private const int PreviewLinePositions = 2;          // 予測線の頂点数
    private const int PreviewLineStartIndex = 0;         // 予測線の始点インデックス
    private const int PreviewLineEndIndex = 1;           // 予測線の終点インデックス
    private const float PreviewLineRaycastDistance = 0f; // Raycast距離（0で点判定）

    // ワイヤー初期化関連
    private const float ResetSwingDirectionX = 0f;       // 切断時のスイング方向初期値

    #endregion

    #region === SerializeField ===

    [Header("針とワイヤー関連")]
    [SerializeField] private Transform needlePivot;     // ワイヤー始点（プレイヤー手の位置）
    [SerializeField] private GameObject needle;         // ワイヤー針オブジェクト
    [SerializeField] private PlayerMove playerMove;     // プレイヤー移動制御コンポーネント

    [Header("プレイヤー・アニメーション関連")]
    [SerializeField] private WireInputHandler inputHandler;           // 左右クリックなどの入力管理
    [SerializeField] private Transform rightHandTransform;            // プレイヤー右手位置
    [SerializeField] private PlayerAnimatorController animatorController; // アニメーション制御

    [Header("ワイヤー設定")]
    [SerializeField] private PlayerWireConfig config;   // ワイヤー物理・描画設定
    [SerializeField] private float cutCooldown = 0.1f;  // ワイヤー切断のクールタイム（秒）

    [Header("ライン描画")]
    [SerializeField] private LineRenderer previewLineRenderer; // 接続可能予測線描画用

    [Header("SE")]
    [SerializeField] private AudioClip wireSE;         // ワイヤー接続時の音

    #endregion

    #region === プライベートフィールド ===

    private LineRenderer lineRenderer;                 // 実際にワイヤーを描画するLineRenderer
    private Renderer needleRenderer;                   // 針の描画ON/OFF用Renderer
    private Coroutine currentNeedleCoroutine;          // 発射中の針コルーチンを管理

    private DistanceJoint2D distanceJoint;            // プレイヤーと接続点を固定するDistanceJoint2D
    private GameObject targetObject = null;           // ワイヤー接続対象のGameObject
    private Vector2 _hookedPosition;                  // ワイヤー接続座標（固定点）

    // ワイヤーやプレイヤー物理設定をキャッシュ
    private float needleSpeed;                        // 針の移動速度
    private float fixedWireLength;                    // ワイヤー固定長
    private float playerGravityScale;                 // プレイヤーの重力倍率
    private float rigidbodyLinearDamping;             // 線形減衰
    private float rigidbodyAngularDamping;            // 回転減衰
    private float swingInitialSpeed;                  // スイング初速

    private float lastSwingDirectionX = ResetSwingDirectionX; // 最後にスイングした方向X
    private float _lastCutTime = -999f;                        // ワイヤー切断の最終時間（クールタイム用）

    #endregion

    #region === プロパティ ===
    public bool IsConnected => distanceJoint.enabled; // ワイヤー接続中か
    public Vector2 HookedPosition => _hookedPosition; // ワイヤー接続座標を外部公開
    #endregion

    #region === Unityライフサイクル ===

    private void Awake()
    {
        // コンポーネント取得
        lineRenderer = GetComponent<LineRenderer>();
        distanceJoint = GetComponent<DistanceJoint2D>();
        needleRenderer = needle.GetComponent<Renderer>();
        animatorController = GetComponent<PlayerAnimatorController>();

        // 設定データからパラメータ読み込み
        needleSpeed = config.needleSpeed;
        fixedWireLength = config.fixedWireLength;
        playerGravityScale = config.playerGravityScale;
        rigidbodyLinearDamping = config.linearDamping;
        rigidbodyAngularDamping = config.angularDamping;
        swingInitialSpeed = config.swingInitialSpeed;
    }

    private void OnEnable()
    {
        // 入力イベント登録
        inputHandler.OnLeftClick += HandleLeftClick;
        inputHandler.OnRightClick += HandleRightClick;

        // 初期状態にリセット
        ResetState();
    }

    private void Update()
    {
        // マウス予測線を毎フレーム更新
        UpdatePreviewLine();

        // ワイヤー接続中はベジェ曲線で描画
        if (IsConnected)
            UpdateBezierWireLine();
    }

    private void OnDisable()
    {
        // 入力イベント解除
        inputHandler.OnLeftClick -= HandleLeftClick;
        inputHandler.OnRightClick -= HandleRightClick;
    }

    #endregion

    #region === ワイヤー操作 ===

    /// <summary>左クリック時のワイヤー接続処理</summary>
    private void HandleLeftClick()
    {
        if (IsConnected) return; // 接続中は何もしない

        // マウス位置をワールド座標に変換
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // マウス座標にTilemapがあるか判定
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        if (hit.collider == null) return;

        // Tilemap取得（親も含める）
        Tilemap tilemap = hit.collider.GetComponent<Tilemap>() ?? hit.collider.GetComponentInParent<Tilemap>();
        if (tilemap == null) return;

        // クリック地点のセルを取得
        Vector3Int cellPos = tilemap.WorldToCell(hit.point);
        TileBase tile = tilemap.GetTile(cellPos);

        // 地面タイプならワイヤー接続を試みる
        if ((tile is CustomTile customTile && customTile.tileType == CustomTile.TileType.Ground) ||
            (tile is ITileWithType tileWithType && tileWithType.tileType == CustomTile.TileType.Ground))
        {
            // プレイヤー方向に沿って接地面を補正
            Vector2 adjustedTarget = FindSurfaceAlongPlayerDirectionTilemap(hit.point);

            // ワイヤー接続開始
            TryConnectWire(adjustedTarget, hit.collider.gameObject);
        }
    }

    /// <summary>右クリック時のワイヤー切断処理</summary>
    private void HandleRightClick()
    {
        if (IsConnected)
            CutWire(); // ワイヤー切断
    }

    /// <summary>ワイヤー切断処理</summary>
    public void CutWire()
    {
        // 着地中は切断を無効化
        if (animatorController.CurrentState == PlayerAnimatorController.PlayerState.Landing)
            return;

        // クールタイム判定
        if (Time.time - _lastCutTime < cutCooldown) return;
        _lastCutTime = Time.time;

        // DistanceJoint2Dを無効化してワイヤーを切断
        distanceJoint.enabled = false;
        targetObject = null;
        lineRenderer.enabled = false;
        SetNeedleVisible(false);

        // 発射中の針コルーチン停止
        if (currentNeedleCoroutine != null)
        {
            StopCoroutine(currentNeedleCoroutine);
            currentNeedleCoroutine = null;
        }

        // アニメーションフラグリセット
        animatorController.ResetWireFlags();

        // 切断後アニメーション遷移
        StartCoroutine(HandleWireCutTransition());
    }

    /// <summary>ワイヤー切断後のアニメーション遷移</summary>
    private IEnumerator HandleWireCutTransition()
    {
        // 物理更新後にアニメーション更新
        yield return new WaitForFixedUpdate();

        // 接地判定
        bool groundedNow = playerMove != null && playerMove.IsGrounded;
        bool almostGroundedNow = playerMove != null && playerMove.IsAlmostGrounded(GroundCheckThreshold);

        if (groundedNow || almostGroundedNow)
            animatorController.ForceIdle(lastSwingDirectionX); // 接地中はIdle
        else
        {
            animatorController.ForceLanding(lastSwingDirectionX); // 空中ならLanding
            yield return new WaitForSeconds(SwingAnimationStopDelay); // スイング停止待機
            if (playerMove != null && playerMove.IsGrounded)
                animatorController.ForceIdle(lastSwingDirectionX); // 着地後Idle
        }
    }

    #endregion

    #region === 針発射・接続 ===

    /// <summary>ワイヤー接続を試みる</summary>
    private void TryConnectWire(Vector2 targetPos, GameObject hitObject)
    {
        if (IsConnected || currentNeedleCoroutine != null) return;

        // 針表示ON
        SetNeedleVisible(true);

        // 発射コルーチン開始
        currentNeedleCoroutine = StartCoroutine(ThrowNeedle(targetPos, hitObject));
    }

    /// <summary>針を目標座標まで飛ばしてワイヤー接続する</summary>
    private IEnumerator ThrowNeedle(Vector2 targetPosition, GameObject hitObject)
    {
        SetNeedleVisible(true);                    // 針を表示
        needle.transform.position = transform.position; // 初期位置をプレイヤー位置に設定

        // 針を目標位置まで移動
        while (Vector2.Distance(needle.transform.position, targetPosition) > NeedleStopDistance)
        {
            Vector2 direction = (targetPosition - (Vector2)needle.transform.position).normalized; // 移動方向
            needle.transform.up = -direction; // 針の向きを進行方向に設定
            needle.transform.position = Vector2.MoveTowards(needle.transform.position, targetPosition, needleSpeed * Time.deltaTime); // 移動
            yield return null; // 次フレームまで待機
        }

        // 到達後座標を調整
        needle.transform.position = targetPosition;
        targetObject = hitObject;
        _hookedPosition = targetPosition;

        // 接続音再生
        AudioManager.Instance?.PlaySE(wireSE);

        // ワイヤー描画ON
        if (lineRenderer != null)
            lineRenderer.enabled = true;

        // DistanceJoint2D設定
        if (distanceJoint != null)
        {
            distanceJoint.enabled = false;
            distanceJoint.connectedBody = null;
            distanceJoint.connectedAnchor = _hookedPosition;
            distanceJoint.maxDistanceOnly = true;
            distanceJoint.distance = fixedWireLength;
            distanceJoint.enabled = true;
        }

        // プレイヤーにスイング初速を付与
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = playerGravityScale;
            rb.linearDamping = rigidbodyLinearDamping;
            rb.angularDamping = rigidbodyAngularDamping;

            // スイング方向計算（接続点との接線方向）
            Vector2 dir = (_hookedPosition - (Vector2)transform.position).normalized;
            Vector2 tangent = new Vector2(-dir.y, dir.x);
            tangent = (lastSwingDirectionX >= 0) ? tangent : -tangent;
            rb.linearVelocity = tangent * swingInitialSpeed;
        }

        // アニメーション再生
        Vector2 dirForAnimation = (_hookedPosition - (Vector2)transform.position).normalized;
        lastSwingDirectionX = dirForAnimation.x;
        animatorController.PlayGrappleSwingAnimation(dirForAnimation.x);

        // コルーチン終了
        currentNeedleCoroutine = null;
    }

    #endregion

    #region === ライン描画 ===

    /// <summary>接続中のワイヤーをベジェ曲線で描画</summary>
    private void UpdateBezierWireLine()
    {
        if (lineRenderer == null || !distanceJoint.enabled) return;

        lineRenderer.positionCount = WireSegmentCount + 1; // 線分数+1
        Vector3 start = rightHandTransform.position;       // ワイヤー始点
        Vector3 end = needlePivot.position;               // ワイヤー終点（針位置）
        Vector3 controlPoint = (start + end) / 2 + Vector3.down * BezierControlOffsetY; // 中間制御点

        // ベジェ曲線の各頂点計算
        for (int i = 0; i <= WireSegmentCount; i++)
        {
            float t = i / (float)WireSegmentCount; // 0~1の補間値
            Vector3 point = Mathf.Pow(1 - t, 2) * start + 2 * (1 - t) * t * controlPoint + Mathf.Pow(t, 2) * end; // 二次ベジェ
            lineRenderer.SetPosition(i, point);
        }
    }

    /// <summary>マウス位置に基づくワイヤー接続予測線描画</summary>
    private void UpdatePreviewLine()
    {
        // 発射中は予測線を非表示
        if (needleRenderer != null && needleRenderer.enabled)
        {
            if (previewLineRenderer != null)
                previewLineRenderer.enabled = false;
            return;
        }

        // マウスワールド座標取得
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // 点判定で接続可能な地形を探す
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, PreviewLineRaycastDistance, LayerMask.GetMask("Ground"));

        if (hit.collider != null)
        {
            // プレイヤー方向に沿って接地面補正
            Vector2 adjustedTarget = FindSurfaceAlongPlayerDirectionTilemap(hit.point);

            // 予測線描画
            if (previewLineRenderer != null)
            {
                previewLineRenderer.positionCount = PreviewLinePositions;
                previewLineRenderer.SetPosition(PreviewLineStartIndex, transform.position);
                previewLineRenderer.SetPosition(PreviewLineEndIndex, adjustedTarget);
                previewLineRenderer.enabled = true;
            }
        }
        else
        {
            if (previewLineRenderer != null)
                previewLineRenderer.enabled = false;
        }
    }

    #endregion

    #region === アニメーション・補助 ===

    /// <summary>スイング停止アニメーションを遅延して再生</summary>
    private IEnumerator DelayedStopSwingAnimation(float directionX)
    {
        yield return new WaitForSeconds(SwingAnimationStopDelay); // 遅延
        animatorController.StopSwingAnimation(directionX);
    }

    #endregion

    #region === ユーティリティ ===

    /// <summary>針の表示切替</summary>
    private void SetNeedleVisible(bool visible)
    {
        if (needleRenderer != null)
            needleRenderer.enabled = visible;
    }

    /// <summary>マウススクリーン座標をワールド座標に変換</summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z; // カメラZ位置補正
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    /// <summary>
    /// プレイヤー方向に沿ってクリック位置から地面を探す
    /// Tilemap用の微調整
    /// </summary>
    private Vector2 FindSurfaceAlongPlayerDirectionTilemap(Vector2 clickPosition)
    {
        Vector2 playerPos = transform.position;
        Vector2 directionToPlayer = (playerPos - clickPosition).normalized; // プレイヤー方向
        Vector2 probePosition = clickPosition;
        Vector2 lastInsidePosition = clickPosition; // 最後にタイル内にあった座標
        bool wasInside = true;
        bool foundSurface = false;

        // Tilemap取得
        RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);
        if (hit.collider == null) return clickPosition;

        Tilemap tilemap = hit.collider.GetComponent<Tilemap>() ?? hit.collider.GetComponentInParent<Tilemap>();
        if (tilemap == null) return clickPosition;

        // タイル探索
        for (int i = 0; i < MaxTileProbeSteps; i++)
        {
            Vector3Int cellPos = tilemap.WorldToCell(probePosition);
            TileBase tile = tilemap.GetTile(cellPos);
            bool isInside = (tile != null);

            // タイル境界を見つけたら終了
            if (wasInside && !isInside)
            {
                foundSurface = true;
                break;
            }

            // 最後にタイル内にあった座標を更新
            if (isInside) lastInsidePosition = probePosition;

            // 次ステップ
            probePosition += directionToPlayer * TileProbeStepSize;
            wasInside = isInside;
        }

        return foundSurface ? lastInsidePosition : clickPosition;
    }

    /// <summary>ワイヤー状態を初期化</summary>
    public void ResetState()
    {
        CutWire();               // ワイヤー切断
        SetNeedleVisible(false); // 針非表示

        // 予測線を非表示
        if (previewLineRenderer != null)
        {
            previewLineRenderer.enabled = false;
            previewLineRenderer.positionCount = 0;
        }

        // 発射中コルーチン停止
        if (currentNeedleCoroutine != null)
            StopCoroutine(currentNeedleCoroutine);

        currentNeedleCoroutine = null;
        lastSwingDirectionX = ResetSwingDirectionX;
    }

    #endregion
}
