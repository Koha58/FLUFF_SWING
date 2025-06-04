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
    #region 針とワイヤー関連

    // 針孔位置用の子オブジェクト
    [SerializeField] private Transform needlePivot;

    // 針オブジェクト
    [SerializeField] private GameObject needle;

    // ワイヤーの見た目制御用（カスタムクラス）
    [SerializeField] private CurvedWireRenderer curvedWireRenderer;

    // ワイヤーの見た目を担当する LineRenderer コンポーネント
    private LineRenderer lineRenderer => GetComponent<LineRenderer>();

    // 針のRendererキャッシュ（表示切り替え用）
    private Renderer needleRenderer;

    #endregion

    #region 接続・物理関連

    // プレイヤーを接続する物理ジョイント（距離固定）
    private DistanceJoint2D distanceJoint => GetComponent<DistanceJoint2D>();

    // 接続対象のオブジェクト
    private GameObject targetObject = null;

    // 接続状態を外部から確認できるようにする
    public bool IsConnected => distanceJoint.enabled;

    #endregion

    #region プレイヤー・アニメーション関連

    // プレイヤーの右手位置
    [SerializeField] private Transform rightHandTransform;

    // アニメーション制御スクリプト
    [SerializeField] private PlayerAnimatorController animatorController;

    #endregion

    #region ライン描画・予測線

    // 予測線（点線）
    [SerializeField] private LineRenderer previewLineRenderer;

    #endregion

    #region コルーチン管理

    // 現在進行中の針移動コルーチン（複数同時に動かさないため）
    private Coroutine currentNeedleCoroutine;

    #endregion

    #region 内部状態保持・定数

    // プレイヤーのワイヤー関連の設定をまとめた設定データ
    // インスペクターから調整可能
    [SerializeField] private PlayerWireConfig config;

    // 最後にスイングした方向（X軸）を記録
    private float lastSwingDirectionX = 0f;

    // 針が目的地に到達したと判定する距離
    private const float NeedleStopDistance = 0.01f;

    // 針の飛ぶ速度
    private float needleSpeed;

    // ワイヤーの長さ（固定）
    private float fixedWireLength;

    // ワイヤー接続時のプレイヤーの重力スケール
    private float playerGravityScale;

    // 空気抵抗（直線減衰）
    private float rigidbodyLinearDamping;

    // 回転減衰
    private float rigidbodyAngularDamping;

    // ラインを非表示にする際の頂点数
    private const int LinePointNone = 0;

    // スイング開始時の初速
    private float swingInitialSpeed;

    #endregion


    private void Awake()
    {
        // needleのRendererを取得（SpriteRendererでもMeshRendererでもRendererならこれで取れる）
        needleRenderer = needle.GetComponent<Renderer>();

        // 初期はneedleを非表示にしておく
        SetNeedleVisible(false);

        // AnimatorControllerを取得（Inspectorで設定されていない場合の保険としてGetComponent）
        animatorController = GetComponent<PlayerAnimatorController>();

        // カーブしたワイヤーの描画用コンポーネントを取得（同一GameObjectにアタッチされている想定）
        curvedWireRenderer = GetComponent<CurvedWireRenderer>();

        // configから各種ワイヤー関連パラメータを取得して初期化
        needleSpeed = config.needleSpeed;                   // 針の飛ぶ速度
        fixedWireLength = config.fixedWireLength;           // ワイヤーの固定長さ
        playerGravityScale = config.playerGravityScale;     // ワイヤー接続時のプレイヤー重力スケール
        rigidbodyLinearDamping = config.linearDamping;      // 空気抵抗（直線減衰）
        rigidbodyAngularDamping = config.angularDamping;    // 回転減衰
        swingInitialSpeed = config.swingInitialSpeed;       // スイング開始時の初速
    }

    private void Start()
    {
        // 初動はワイヤーを接続しない
        CutWire(); // AnimatorControllerの取得が確実に終わってから呼ぶ

        // カーブワイヤー描画が存在する場合は、初期状態で非表示にしておく
        if (curvedWireRenderer != null)
        {
            curvedWireRenderer.SetVisible(false);
        }
    }

    void Update()
    {
        // 左クリック入力を処理（針を飛ばして接続）
        HandleLeftClick();

        // 右クリック入力を処理（接続を解除）
        HandleRightClick();

        // 接続前にプレイヤーからマウス位置までの予測ライン（点線）を描画
        UpdatePreviewLine();

        // ワイヤー接続中、カーブワイヤーのラインを更新して表示
        UpdateCurvedWireLine();
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
            // クリック位置をそのまま使わず、表面を探す関数を呼び出す
            Vector2 adjustedTarget = FindSurfaceAlongPlayerDirectionTilemap(hit.point);

            // 見つかれば接続
            TryConnectWire(adjustedTarget, hit.collider.gameObject);
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
    /// ワイヤー接続要求。
    /// 接続中またはクールタイム中は処理をスキップ。
    /// 針を飛ばすコルーチンを開始。
    /// </summary>
    private void TryConnectWire(Vector2 targetPos, GameObject hitObject)
    {
        // 接続中 or クールタイム中は無視
        if (IsConnected)
            return;

        // 既存の針コルーチンを停止（複数同時起動防止）
        if (currentNeedleCoroutine != null)
            StopCoroutine(currentNeedleCoroutine);

        SetNeedleVisible(true);

        // 新しい針を飛ばすコルーチンを開始
        currentNeedleCoroutine = StartCoroutine(ThrowNeedle(targetPos, hitObject));
    }

    /// <summary>
    /// ワイヤーを切断。
    /// </summary>
    private void CutWire()
    {
        // DistanceJoint2D を無効化してワイヤー接続を解除
        distanceJoint.enabled = false;

        // LineRenderer の点数を 0 にしてワイヤーの見た目を消す
        lineRenderer.positionCount = LinePointNone;

        // ワイヤーの接続先（ターゲット）をリセット
        targetObject = null;

        // 針（needle）の見た目も非表示にする
        SetNeedleVisible(false);

        // ワイヤーの可視化をOFFにする
        curvedWireRenderer.SetVisible(false);

        // 最後に記録されたスイング方向（X成分）を使ってアニメーション停止処理を行う
        animatorController.StopSwingAnimation(lastSwingDirectionX);

        // ワイヤー接続後、即時にワイヤーが切断された場合にJumpアニメーションをループさせないため
        // Jumpアニメーションの停止処理を行う
        animatorController?.UpdateJumpState(lastSwingDirectionX);

        // デバッグログ出力
        Debug.Log("ワイヤーを切断しました");
    }

    /// <summary>
    /// 針をターゲット位置まで移動させ、到達後にワイヤーを接続してスイング状態に遷移するコルーチン。
    /// </summary>
    /// <param name="targetPosition">針が向かうワイヤー接続先のワールド座標</param>
    /// <param name="hitObject">接続対象となったオブジェクト（環境側）</param>
    private IEnumerator ThrowNeedle(Vector2 targetPosition, GameObject hitObject)
    {
        // 針を表示
        SetNeedleVisible(true);

        // プレイヤーの位置から針を飛ばし始める
        needle.transform.position = transform.position;

        // 針が目標位置に到達するまで直線移動させる
        while (Vector2.Distance(needle.transform.position, targetPosition) > NeedleStopDistance)
        {
            // 移動方向を算出し、針の向きを調整（先端を進行方向に向ける）
            Vector2 direction = (targetPosition - (Vector2)needle.transform.position).normalized;
            needle.transform.up = -direction;

            // 一定速度で針を移動
            needle.transform.position = Vector2.MoveTowards(needle.transform.position, targetPosition, needleSpeed);

            yield return null; // 次フレームまで待機
        }

        // 到達したら位置を最終確定し、接続対象を記録
        needle.transform.position = targetPosition;
        targetObject = hitObject;

        // ワイヤーの見た目（カーブ線）を表示する
        curvedWireRenderer.SetVisible(true);

        // 針孔（needlePivot）のワールド座標を取得
        Vector3 needlePivotWorldPos = needlePivot.position;

        // ワイヤー接続のためのDistanceJoint2Dを構成
        distanceJoint.enabled = false; // 一度無効化してから設定
        distanceJoint.connectedBody = null; // 静的な位置接続にするためBodyは指定しない
        distanceJoint.connectedAnchor = needlePivotWorldPos; // 接続先のワールド座標
        distanceJoint.maxDistanceOnly = true; // 最大距離を超えないように制限（バネ的に伸びない）
        distanceJoint.distance = fixedWireLength; // 固定の長さに設定
        distanceJoint.enabled = true; // 接続を有効化

        // プレイヤーの物理挙動を調整
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = playerGravityScale;
        rb.linearDamping = rigidbodyLinearDamping;
        rb.angularDamping = rigidbodyAngularDamping;

        // 初速を与えてスイング開始（接線方向に加速）
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        Vector2 tangent = new Vector2(-dir.y, dir.x); // 接線ベクトルを計算
        tangent = (lastSwingDirectionX >= 0) ? tangent : -tangent; // 前回のスイング方向に応じて反転
        rb.linearVelocity = tangent * swingInitialSpeed;

        // 現在のスイング方向（左右）を記録
        lastSwingDirectionX = dir.x;

        // スイング用のアニメーションを再生
        animatorController.PlayGrappleSwingAnimation(dir.x);

        // コルーチンの参照をクリア（多重発射防止などの管理用）
        currentNeedleCoroutine = null;
    }

    /// <summary>
    /// ワイヤーの予測ラインを更新するメソッド。
    /// マウスカーソルの位置に地形がある場合、プレイヤーからその地点までのラインを描画。
    /// 針（needleRenderer）が表示中は、ラインを非表示にする。
    /// </summary>
    private void UpdatePreviewLine()
    {
        // 針（needle）が表示されている場合は、予測ラインを非表示にして処理を中断
        if (needleRenderer.enabled == true)
        {
            previewLineRenderer.enabled = false;
            return;
        }

        // マウスカーソルのワールド座標を取得
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // マウス位置に地面レイヤー（"Ground"）が存在するか調べる
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, LayerMask.GetMask("Ground"));

        if (hit.collider != null)
        {
            // プレイヤーの方向に合わせて補正された地形の表面座標を取得
            Vector2 adjustedTarget = FindSurfaceAlongPlayerDirectionTilemap(hit.point);

            // ラインの始点と終点を設定（プレイヤー → 対象地点）
            previewLineRenderer.positionCount = 2;
            previewLineRenderer.SetPosition(0, transform.position);
            previewLineRenderer.SetPosition(1, adjustedTarget);

            // ライン表示を有効化
            previewLineRenderer.enabled = true;
        }
        else
        {
            // 地面が見つからなかった場合、ライン非表示
            previewLineRenderer.enabled = false;
        }
    }

    /// <summary>
    /// CurvedWireRendererを使って、プレイヤーの手元から接続先までのワイヤーを
    /// ベジエ曲線（2次）で滑らかに描画する
    /// </summary>
    private void UpdateCurvedWireLine()
    {
        // ワイヤーが未接続なら描画しない
        if (!distanceJoint.enabled) return;

        int segmentCount = 10; // 曲線の分割数（多いほど滑らか）
        lineRenderer.positionCount = segmentCount + 1;

        // 曲線の始点：プレイヤーの右手位置
        Vector3 start = rightHandTransform.position;

        // 曲線の終点：DistanceJoint2Dの接続先（ワールド座標）
        Vector3 end = distanceJoint.connectedAnchor;

        // 制御点：開始点と終了点の中間に下方向へオフセットを加えることで曲線にたるみを持たせる
        Vector3 controlPoint = (start + end) / 2 + Vector3.down * 1.5f;

        // ベジエ曲線を描画するため、segmentCount + 1 個の頂点を計算
        for (int i = 0; i <= segmentCount; i++)
        {
            // tは0〜1の間で均等に分割された補間係数（0: start, 1: end）
            float t = i / (float)segmentCount;

            // 2次ベジエ曲線の式：
            // B(t) = (1 - t)^2 * P0 + 2(1 - t)t * P1 + t^2 * P2
            // P0: 始点, P1: 制御点（コントロールポイント）, P2: 終点
            Vector3 point = Mathf.Pow(1 - t, 2) * start +           // 始点の寄与
                            2 * (1 - t) * t * controlPoint +        // 制御点の寄与
                            Mathf.Pow(t, 2) * end;                  // 終点の寄与

            // 計算した位置をLineRendererの対応するインデックスに設定
            lineRenderer.SetPosition(i, point);
        }
    }

    /// <summary>
    /// needleの表示/非表示切り替え（Rendererのenabled制御）
    /// </summary>
    private void SetNeedleVisible(bool visible)
    {
        if (needleRenderer != null)
            needleRenderer.enabled = visible;
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

    /// <summary>
    /// Tilemap専用：クリック位置からプレイヤー方向に戻り、壁表面を探す。
    /// </summary>
    private Vector2 FindSurfaceAlongPlayerDirectionTilemap(Vector2 clickPosition)
    {
        // プレイヤーとクリック位置のベクトルを取得
        Vector2 playerPos = transform.position;
        Vector2 directionToPlayer = (playerPos - clickPosition).normalized;

        // 探索開始位置をクリック位置に設定
        Vector2 probePosition = clickPosition;
        Vector2 lastInsidePosition = clickPosition;

        // 状態管理用変数
        bool wasInside = true;
        bool foundSurface = false;

        // 最初にクリック位置でTilemapを取得（クリック地点のTilemap限定）
        RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);
        if (hit.collider == null) return clickPosition; // Tilemap以外にクリックならそのまま返す

        Tilemap tilemap = hit.collider.GetComponent<Tilemap>() ?? hit.collider.GetComponentInParent<Tilemap>();
        if (tilemap == null) return clickPosition; // Tilemapじゃない場合もそのまま

        // プレイヤー方向へ50回分、一定距離ずつ進んでチェック
        for (int i = 0; i < 50; i++)
        {
            // 現在位置のTile座標を取得
            Vector3Int cellPos = tilemap.WorldToCell(probePosition);

            // 該当位置にタイルがあるか確認
            TileBase tile = tilemap.GetTile(cellPos);

            bool isInside = (tile != null);

            // 「タイル内」→「タイル外」に変わった瞬間が壁の表面
            if (wasInside && !isInside)
            {
                foundSurface = true;
                break;
            }

            // タイル内なら、その位置を記録（最後にタイル内だった場所）
            if (isInside)
                lastInsidePosition = probePosition;

            // プレイヤー方向に少し進める
            probePosition += directionToPlayer * 0.1f;

            // 状態更新
            wasInside = isInside;
        }

        if (foundSurface)
        {
            Debug.Log($"Tilemap表面検出:{lastInsidePosition}");
            return lastInsidePosition;
        }
        else
        {
            Debug.Log($"Tilemap表面見つからず:{clickPosition}");
            return clickPosition;
        }
    }

}
