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

    private bool isConnectedToMovingObject = false;
    private GameObject connectedObject = null;

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
        {
            UpdateNeedleFollowTarget();
            UpdateBezierWireLine();
        }
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
        if (IsConnected) return;

        // 1. マウスのスクリーン座標をゲーム内のワールド座標（Vector3）に変換
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // 2. 判定対象とするレイヤーを "Ground" のみに限定するためのマスクを取得
        int groundLayer = LayerMask.GetMask("Ground");

        // 3. マウス位置にコライダー（Groundレイヤー）が存在するか「点判定」を行う
        // Physics2D.Raycast(地点, 方向, 距離, レイヤー)
        // 方向を zero、距離を 0 にすることで、その地点に重なっているものだけを検出する
        RaycastHit2D initialHit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, groundLayer);

        // 4. Groundレイヤーのオブジェクトにヒットしなかった場合は、ここで処理を終了（針を投げない）
        if (initialHit.collider == null) return;

        // 5. ヒットしたオブジェクトの参照と、実際にクリックされた座標（ヒット地点）を保持
        GameObject hitObj = initialHit.collider.gameObject;
        Vector2 connectPoint = initialHit.point;

        // 6. ヒットしたオブジェクトが「動く床（Rigidbody2Dあり）」か「タイルマップ」かを判定
        // 後のステップで、それぞれの性質に合わせた接続位置の補正を行うために取得しておく
        Rigidbody2D hitRb = hitObj.GetComponent<Rigidbody2D>();
        Tilemap tilemap = hitObj.GetComponent<Tilemap>() ?? hitObj.GetComponentInParent<Tilemap>();

        Vector2 finalConnectPoint = connectPoint;
        const float offset = 0.01f; // 針の埋まりを防ぐためのオフセット量

        // 🔹① Tilemapの場合
        if (tilemap != null)
        {
            Vector3Int cellPos = tilemap.WorldToCell(connectPoint);
            TileBase tile = tilemap.GetTile(cellPos);

            if (tile is CustomTile c && c.tileType == CustomTile.TileType.Ground ||
                tile is ITileWithType t && t.tileType == CustomTile.TileType.Ground)
            {
                // Tilemapは専用の探索ロジックで補正
                finalConnectPoint = FindSurfaceAlongPlayerDirectionTilemap(connectPoint);
            }
        }
        // 🔹② Tilemap以外のコライダー (静的 or 動的)
        else
        {
            // 正確な法線を取得するために、コライダーに沿ったRaycastを実行する
            // プレイヤーからヒット点へ向かう方向のRaycast
            Vector2 directionToHit = (connectPoint - (Vector2)transform.position).normalized;

            // ヒット点の少し手前（0.05f）から、ヒット点に向けてRaycast（距離0.1f）を飛ばすことで、
            // 安定したヒットポイントと法線を取得する。
            RaycastHit2D surfaceHit = Physics2D.Raycast(connectPoint - directionToHit * 0.05f,
                                                        directionToHit,
                                                        0.1f);

            if (surfaceHit.collider != null && surfaceHit.collider.gameObject == hitObj)
            {
                // Raycastで得られた正確な表面位置と法線でオフセットを適用
                finalConnectPoint = surfaceHit.point + surfaceHit.normal * offset;
            }
            else
            {
                // 安定したRaycastが失敗した場合（非常に薄いコライダーなど）、
                // 最初の点判定の法線を使用してオフセットを試みる（最後の手段）
                finalConnectPoint = connectPoint + initialHit.normal * offset;
            }
        }

        // 接続判定
        // すでにLayerMaskで絞っているため、タグ判定は補助的に
        if (hitObj.CompareTag("WireConnectable") || hitObj.layer == LayerMask.NameToLayer("Ground"))
        {
            TryConnectWire(finalConnectPoint, hitObj);
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
        // クールタイム判定
        if (Time.time - _lastCutTime < cutCooldown) return;
        _lastCutTime = Time.time;

        // 既にアニメーション遷移コルーチンが実行中の場合は、二重実行を防ぐ
        if (currentNeedleCoroutine != null)
        {
            StopCoroutine(currentNeedleCoroutine);
            currentNeedleCoroutine = null;
        }

        // 【1】 物理的な切断
        distanceJoint.enabled = false;
        targetObject = null;
        lineRenderer.enabled = false;
        SetNeedleVisible(false);

        // アニメーションフラグリセット
        animatorController.ResetWireFlags();

        // アニメーション処理をコルーチンで実行し、FixedUpdateを待つ
        // 攻撃処理との競合を避けるため、アニメーション遷移を物理フレームの後に遅延させる。
        // currentNeedleCoroutineをワイヤー切断後の遷移管理にも流用する
        currentNeedleCoroutine = StartCoroutine(CutWireAndTransitionCo());
    }

    /// <summary>
    /// ワイヤー切断後のアニメーション遷移をFixedUpdate後に実行し、攻撃入力との競合を防ぐ。
    /// </summary>
    private IEnumerator CutWireAndTransitionCo()
    {
        // 物理計算 (FixedUpdate) の実行完了を待つ
        // これにより、isGrounded の最終的な状態が確定し、
        // 同時に入力された攻撃処理が先に実行される機会を与える。
        yield return new WaitForFixedUpdate();

        // 確実な接地判定を取得
        bool isGroundedNow = playerMove != null && playerMove.IsGrounded;

        // 確実な状態遷移を PlayerAnimatorController に委ねる
        // PlayerAnimatorController.OnWireCut はアニメーターのパラメータを直接上書きするため、
        // 攻撃ステートになっていたとしても、そこから強制的に Idle/Landing に遷移させる。
        animatorController.OnWireCut(lastSwingDirectionX, isGroundedNow);

        // コルーチン終了
        currentNeedleCoroutine = null;
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
        SetNeedleVisible(true);
        needle.transform.position = transform.position;

        while (Vector2.Distance(needle.transform.position, targetPosition) > NeedleStopDistance)
        {
            Vector2 direction = (targetPosition - (Vector2)needle.transform.position).normalized;
            needle.transform.up = -direction;
            needle.transform.position = Vector2.MoveTowards(needle.transform.position, targetPosition, needleSpeed * Time.deltaTime);
            yield return null;
        }

        // ◆ 接続初期位置を記録
        needle.transform.position = targetPosition;
        targetObject = hitObject;
        _hookedPosition = targetPosition;

        AudioManager.Instance?.PlaySE(wireSE);
        if (lineRenderer != null)
            lineRenderer.enabled = true;

        // =============================
        // ◆ DistanceJoint2D 接続処理
        // =============================
        if (distanceJoint != null)
        {
            distanceJoint.enabled = false;

            Rigidbody2D hitRb = hitObject.GetComponent<Rigidbody2D>();

            if (hitRb != null) // 🔹動く床に接続
            {
                distanceJoint.connectedBody = hitRb;

                // ワールド座標のヒット位置を、Rigidbodyのローカル座標に変換
                distanceJoint.connectedAnchor = hitRb.transform.InverseTransformPoint(targetPosition);

                // 後で針位置を更新できるよう保存
                isConnectedToMovingObject = true;
                connectedObject = hitObject;
            }
            else // Tilemapなどの静的オブジェクト
            {
                distanceJoint.connectedBody = null;
                distanceJoint.connectedAnchor = _hookedPosition;

                isConnectedToMovingObject = false;
                connectedObject = null;
            }

            distanceJoint.maxDistanceOnly = true;
            distanceJoint.distance = fixedWireLength;
            distanceJoint.enabled = true;
        }

        // =============================
        // ◆ スイング初速
        // =============================
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = playerGravityScale;
            rb.linearDamping = rigidbodyLinearDamping;
            rb.angularDamping = rigidbodyAngularDamping;

            Vector2 dir = (_hookedPosition - (Vector2)transform.position).normalized;
            Vector2 tangent = new Vector2(-dir.y, dir.x);
            tangent = (lastSwingDirectionX >= 0) ? tangent : -tangent;
            rb.linearVelocity = tangent * swingInitialSpeed;
        }

        // =============================
        // ◆ アニメーション
        // =============================
        Vector2 dirForAnimation = (_hookedPosition - (Vector2)transform.position).normalized;
        lastSwingDirectionX = dirForAnimation.x;
        animatorController.PlayGrappleSwingAnimation(dirForAnimation.x);

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

        // 点判定で接続可能な地形を探す (Groundレイヤーのみ)
        RaycastHit2D initialHit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, PreviewLineRaycastDistance, LayerMask.GetMask("Ground"));

        if (initialHit.collider != null)
        {
            GameObject hitObj = initialHit.collider.gameObject;
            Vector2 connectPoint = initialHit.point;
            Vector2 adjustedTarget = connectPoint;
            const float offset = 0.01f; // 針の埋まりを防ぐためのオフセット量

            Tilemap tilemap = hitObj.GetComponent<Tilemap>() ?? hitObj.GetComponentInParent<Tilemap>();

            // ① Tilemapの場合
            if (tilemap != null)
            {
                // Tilemapの特殊な補正ロジック
                adjustedTarget = FindSurfaceAlongPlayerDirectionTilemap(connectPoint);
            }
            // ② Tilemap以外のコライダー (静的 or 動的)
            else
            {
                // 安定した法線を取得するためのRaycastを改めて実行
                // プレイヤーからヒット点へ向かう方向のRaycast
                Vector2 directionToHit = (connectPoint - (Vector2)transform.position).normalized;

                // ヒット点の少し手前（0.05f）から、ヒット点に向けてRaycast（距離0.1f）を飛ばす
                RaycastHit2D surfaceHit = Physics2D.Raycast(connectPoint - directionToHit * 0.05f,
                                                            directionToHit,
                                                            0.1f,
                                                            LayerMask.GetMask("Ground"));

                if (surfaceHit.collider != null && surfaceHit.collider.gameObject == hitObj)
                {
                    // Raycastで得られた正確な表面位置と法線でオフセットを適用
                    adjustedTarget = surfaceHit.point + surfaceHit.normal * offset;
                }
                else
                {
                    // Raycast失敗時
                    adjustedTarget = connectPoint + initialHit.normal * offset;
                }
            }

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

    private void UpdateNeedleFollowTarget()
    {
        if (!IsConnected || !isConnectedToMovingObject || connectedObject == null) return;

        // Rigidbodyを取得
        Rigidbody2D connectedRb = connectedObject.GetComponent<Rigidbody2D>();
        if (connectedRb == null) return;

        // RigidbodyのTransformを使ってローカルアンカーをワールド座標に変換
        Vector2 localAnchor = distanceJoint.connectedAnchor;
        Vector2 newHookedPosition = connectedRb.transform.TransformPoint(localAnchor);

        // 針位置の更新
        needle.transform.position = newHookedPosition;
        _hookedPosition = newHookedPosition;  // LineRendererなどで使用する位置も更新

        // distanceJoint.connectedAnchor はローカル座標であり、一度設定したら動く床に固定されているため、
        // ここで更新する必要はない。
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

    /// <summary>
    /// ゲームオーバー専用：ワイヤーの物理・描画だけを即時解除する。
    /// ※アニメ遷移（OnWireCut）を絶対に発生させない
    /// </summary>
    public void ForceClearWireOnlyForGameOver()
    {
        // 走ってるコルーチン全部止める（ThrowNeedle / CutWireAndTransitionCo など）
        StopAllCoroutines();
        currentNeedleCoroutine = null;

        // 物理的な切断
        if (distanceJoint != null) distanceJoint.enabled = false;
        targetObject = null;

        // 動く床追従情報もクリア
        isConnectedToMovingObject = false;
        connectedObject = null;

        // 描画OFF
        if (lineRenderer != null) lineRenderer.enabled = false;
        SetNeedleVisible(false);

        // 予測線OFF
        if (previewLineRenderer != null)
        {
            previewLineRenderer.enabled = false;
            previewLineRenderer.positionCount = 0;
        }

        // ※重要：animatorController.ResetWireFlags() も呼ばない（アニメ側には触れない）
    }


    #endregion
}
