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

        // 1) マウスのワールド座標
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // 2) Ground上の「表面ヒット(point/normal)」を安定して取得
        //    取れなければ接続しない
        if (!TryGetSurfacePointOnGround(mouseWorldPos, out RaycastHit2D surfaceHit))
            return;

        // 3) 接続点は表面+法線方向に少しだけ逃がす（めり込み防止）
        const float offset = 0.01f;
        Vector2 finalConnectPoint = surfaceHit.point + surfaceHit.normal * offset;

        // 4) 対象オブジェクト
        GameObject hitObj = surfaceHit.collider.gameObject;

        // 5) 既存の接続条件（必要なら）
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

    /// <summary>
    /// 敵にワイヤーを掴まれた際、プレイヤーの物理挙動を一時停止する
    /// </summary>
    /// <param name="grabPosition">敵がワイヤーを掴んだ位置</param>
    public void GrabWire(Vector2 grabPosition)
    {
        if (!IsConnected) return;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 1. 速度をゼロにして物理的な慣性を止める
            rb.linearVelocity = Vector2.zero;
            // 2. 敵が掴んでいる間、重力の影響を受けないようにする（任意）
            rb.gravityScale = 0f;
        }

        // 必要であれば、DistanceJointの距離をその時の距離に固定し直す
        distanceJoint.distance = Vector2.Distance(transform.position, _hookedPosition);
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

        // 重力を元に戻す
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // GrabWire で 0 にされた重力を、設定データ(config)の値に戻す
            rb.gravityScale = playerGravityScale;
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

    /// <summary>
    /// ワイヤー接続を試みる
    /// ・すでに接続中、または針発射中の場合は無視
    /// ・針の射出と同時にライン描画を初期化する
    /// </summary>
    private void TryConnectWire(Vector2 targetPos, GameObject hitObject)
    {
        // すでにワイヤー接続中、または針が飛んでいる最中なら処理しない
        if (IsConnected || currentNeedleCoroutine != null) return;

        // ===== ラインレンダラー初期化 =====
        // 発射直後にワイヤーが「手元から伸びる」ように見せるため、
        // すべての頂点を右手位置に揃えておく
        if (lineRenderer != null)
        {
            // 投擲中もワイヤーを表示したい場合はここで有効化
            lineRenderer.enabled = true;

            // 全頂点を右手位置にセット（初期状態）
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, rightHandTransform.position);
            }
        }

        // 針を表示し、射出開始
        SetNeedleVisible(true);

        // 針の移動＆接続処理をコルーチンで開始
        // ※ currentNeedleCoroutine を保持することで多重発射を防ぐ
        currentNeedleCoroutine = StartCoroutine(
            ThrowNeedle(targetPos, hitObject)
        );
    }

    /// <summary>
    /// 針を目標座標まで飛ばし、命中したオブジェクトにワイヤー接続する
    /// ・静止オブジェクト／動く床 両対応
    /// ・動く床の場合はローカル座標で接続点を保持する
    /// ・射出〜接続までの描画チラつきを防止する
    /// </summary>
    private IEnumerator ThrowNeedle(Vector2 initialTargetPosition, GameObject hitObject)
    {
        // ===== 射出初期化 =====

        // 射出フレームで針を「即座に」右手位置へ移動
        // 　→ TryConnectWire 側のライン初期化とズレないようにするため
        needle.transform.position = rightHandTransform.position;

        // 針の表示を有効化
        SetNeedleVisible(true);

        // 初フレームからワイヤー描画を更新
        // 　→ 1フレーム目の「線が消える／跳ねる」現象を完全に防ぐ
        UpdateBezierWireLine();
        if (lineRenderer != null) lineRenderer.enabled = true;

        // ===== 接続対象の判定 =====

        // 命中オブジェクトの Rigidbody（動く床判定）
        Rigidbody2D targetRb = hitObject.GetComponent<Rigidbody2D>();

        // 動く床用：床ローカル空間での接続点
        Vector2 localTargetPos = Vector2.zero;

        if (targetRb != null)
        {
            // ヒット時のワールド座標を床ローカル座標に変換
            // 　→ 床が動いても接続点がズレない
            localTargetPos = targetRb.transform.InverseTransformPoint(initialTargetPosition);
        }

        // ===== 針の飛行処理 =====

        float elapsed = 0f;

        // 最大2秒で到達しなければ中断（安全装置）
        while (elapsed < 2.0f)
        {
            elapsed += Time.deltaTime;

            // 現在のターゲット座標を取得
            // ・動く床：ローカル → ワールドへ再変換
            // ・静止物：初期ヒット位置をそのまま使用
            Vector2 currentTargetPos = (targetRb != null)
                ? (Vector2)targetRb.transform.TransformPoint(localTargetPos)
                : initialTargetPosition;

            float distance = Vector2.Distance(needle.transform.position, currentTargetPos);

            // 到達判定
            // ・十分近づいた
            // ・次フレームで追い越す距離
            if (distance <= 0.1f || distance < needleSpeed * Time.deltaTime)
            {
                _hookedPosition = currentTargetPos;
                break;
            }

            // ===== 針の移動 =====

            // ターゲットへの進行方向
            Vector2 direction = (currentTargetPos - (Vector2)needle.transform.position).normalized;

            // 針の向きを進行方向に合わせる（先端が向く）
            needle.transform.up = -direction;

            // 一定速度でターゲットに向かって移動
            needle.transform.position = Vector2.MoveTowards(
                needle.transform.position,
                currentTargetPos,
                needleSpeed * Time.deltaTime
            );

            // 飛行中も毎フレーム描画更新
            // 　→ ベジェ曲線ワイヤーを常に滑らかに表示
            UpdateBezierWireLine();

            yield return null;
        }

        // ===== ワイヤー接続処理 =====

        if (distanceJoint != null)
        {
            // 再設定前に一度無効化
            distanceJoint.enabled = false;

            if (targetRb != null)
            {
                // 【動く床】
                // Rigidbody に直接接続
                distanceJoint.connectedBody = targetRb;

                // アンカーは床ローカル座標で指定
                distanceJoint.connectedAnchor = localTargetPos;

                isConnectedToMovingObject = true;
                connectedObject = hitObject;
            }
            else
            {
                // 【静止オブジェクト】
                // ワールド座標アンカーを使用
                distanceJoint.connectedBody = null;
                distanceJoint.connectedAnchor = _hookedPosition;

                isConnectedToMovingObject = false;
            }

            // ワイヤー長を固定
            distanceJoint.distance = fixedWireLength;

            // ここで物理的にワイヤーが接続される
            distanceJoint.enabled = true;
        }

        // ===== 接続成功後の共通処理 =====

        // 接続対象を保持
        targetObject = hitObject;

        // 針を最終接続位置に固定
        needle.transform.position = _hookedPosition;

        // 接続完了状態のワイヤーを最終更新
        UpdateBezierWireLine();

        // 効果音再生
        AudioManager.Instance?.PlaySE(wireSE);

        // スイング用物理設定＆初速付与
        ApplySwingPhysics();

        // コルーチン終了通知
        currentNeedleCoroutine = null;
    }


    /// <summary>
    /// スイング開始時の物理設定と初速付与
    /// </summary>
    private void ApplySwingPhysics()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // 重力・減衰設定（スイング用）
        rb.gravityScale = playerGravityScale;
        rb.linearDamping = rigidbodyLinearDamping;
        rb.angularDamping = rigidbodyAngularDamping;

        // フック方向（中心→接続点）
        Vector2 dir = (_hookedPosition - (Vector2)transform.position).normalized;

        // 円運動の接線方向を算出
        Vector2 tangent = new Vector2(-dir.y, dir.x);

        // 直前の入力方向に応じてスイング方向を決定
        tangent = (lastSwingDirectionX >= 0) ? tangent : -tangent;

        // スイング初速を付与
        rb.linearVelocity = tangent * swingInitialSpeed;

        // スイングアニメーション再生
        animatorController.PlayGrappleSwingAnimation(dir.x);

        // 次回用に方向を保存
        lastSwingDirectionX = dir.x;
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
    /// マウス位置にある Ground の「表面ヒット(point/normal)」を安定して取得する。
    /// OverlapPointで対象Colliderを決めてから、手元→マウス方向にRaycastして表面を取る。
    /// </summary>
    private bool TryGetSurfacePointOnGround(Vector2 mouseWorld, out RaycastHit2D surfaceHit)
    {
        int mask = LayerMask.GetMask("Ground");

        // 1) その点に重なっているGroundコライダーを取得
        Collider2D col = Physics2D.OverlapPoint(mouseWorld, mask);
        if (col == null)
        {
            surfaceHit = default;
            return false;
        }

        // 2) 手元(推奨)からマウスへ向けてRaycastし、表面point/normalを取得
        Vector2 origin = (Vector2)rightHandTransform.position;
        Vector2 dir = (mouseWorld - origin);
        float dist = dir.magnitude;
        if (dist <= 0.0001f)
        {
            surfaceHit = default;
            return false;
        }
        dir /= dist;

        const float startBack = 0.05f;
        Vector2 rayOrigin = origin - dir * startBack;

        surfaceHit = Physics2D.Raycast(rayOrigin, dir, dist + startBack + 0.2f, mask);

        if (surfaceHit.collider == null) return false;

        // OverlapPointで拾ったColliderと違うものをRaycastが拾ったら不採用（奥のGround拾い防止）
        if (surfaceHit.collider != col) return false;

        return true;
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
