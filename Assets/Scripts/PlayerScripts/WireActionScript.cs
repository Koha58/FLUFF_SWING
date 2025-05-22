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
    [SerializeField] private Transform needlePivot;  // 針孔位置用の子オブジェクト
    [SerializeField] private GameObject needle;      // 針オブジェクト

    // アニメーション を管理するスクリプト
    [SerializeField] private PlayerAnimatorController animatorController;

    // needleの表示制御用Rendererをキャッシュ
    private Renderer needleRenderer;

    // 接続対象のオブジェクト
    private GameObject targetObject = null;

    // ワイヤーの見た目を担当する LineRenderer コンポーネント
    private LineRenderer lineRenderer => GetComponent<LineRenderer>();

    // プレイヤーを接続する物理ジョイント（距離固定）
    private DistanceJoint2D distanceJoint => GetComponent<DistanceJoint2D>();

    // 現在進行中の針移動コルーチン（複数同時に動かさないため管理）
    private Coroutine currentNeedleCoroutine;

    public bool IsConnected => distanceJoint.enabled; // 接続状態を外部から確認できるようにする

    #region 定数

    // --- ワイヤーの動作に関する定数 ---

    private const float NeedleStopDistance = 0.01f;      // 針が目的地に到達したと判定する距離
    private const float NeedleSpeed = 0.15f;              // 針の飛ぶ速度
    private const float FixedWireLength = 3.5f;          // ワイヤーの長さ（固定）

    // --- プレイヤー挙動関連 ---

    private const float SwingForce = 300f;                // スイング開始時にプレイヤーへ加える力
    private const float PlayerGravityScale = 3f;         // ワイヤー接続時のプレイヤーの重力スケール
    private const float RigidbodyLinearDamping = 0f;     // プレイヤーの空気抵抗（直線減衰）
    private const float RigidbodyAngularDamping = 0f;    // プレイヤーの回転減衰

    // --- ライン描画関連 ---

    private const int LineRendererPointCount = 2;       // ラインレンダラーが使用する頂点数（開始点と終了点）
    private const int LinePointNone = 0;                 // ラインを非表示にする際の頂点数
    private const int LineStartIndex = 0;                // ラインの始点インデックス
    private const int LineEndIndex = 1;                  // ラインの終点インデックス

    // --- 内部状態保持 ---

    private float lastSwingDirectionX = 0f;                // 最後にスイングした方向（X軸）を記録

    #endregion


    private void Awake()
    {
        // needleのRendererを取得（SpriteRendererでもMeshRendererでもRendererならこれで取れる）
        needleRenderer = needle.GetComponent<Renderer>();

        // 初期はneedleを非表示にしておく
        SetNeedleVisible(false);

        // AnimatorControllerを取得
        animatorController = GetComponent<PlayerAnimatorController>();
    }

    private void Start()
    {
        // 初動はワイヤーを接続しない
        CutWire(); // AnimatorControllerの取得が確実に終わってから呼ぶ
    }

    void Update()
    {
        HandleLeftClick();   // 左クリック：接続処理
        HandleRightClick();  // 右クリック：切断処理
        UpdateLine();        // 常にワイヤーの見た目を更新
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
    /// 左クリック時の接続処理。
    /// クリック位置が Ground タイルであれば、ワイヤーを接続する。
    /// </summary>
    private void HandleLeftClick()
    {
        //distanceJoint.enabled = false;
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
    /// ワイヤーの見た目（LineRenderer）を更新する。
    /// </summary>
    private void UpdateLine()
    {
        // ジョイントが有効かつ LineRenderer が最低限の点数を持っている場合のみ更新
        if (distanceJoint.enabled && lineRenderer.positionCount >= LineRendererPointCount)
        {
            // 始点はプレイヤー（自分）
            lineRenderer.SetPosition(LineStartIndex, transform.position);

            // 終点はジョイントの接続アンカー（接続座標）
            lineRenderer.SetPosition(LineEndIndex, distanceJoint.connectedAnchor);
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

        SetNeedleVisible(true);

        // 新しい針を飛ばすコルーチンを開始
        currentNeedleCoroutine = StartCoroutine(ThrowNeedle(targetPos, hitObject));
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

        // 最後に記録されたスイング方向（X成分）を使ってアニメーション停止処理を行う
        animatorController.StopSwingAnimation(lastSwingDirectionX);

        // デバッグログ出力
        Debug.Log("ワイヤーを切断しました");
    }

    /// <summary>
    /// 針をターゲット位置まで移動し、到達したらワイヤー接続を行うコルーチン。
    /// </summary>
    private IEnumerator ThrowNeedle(Vector2 targetPosition, GameObject hitObject)
    {
        SetNeedleVisible(true);

        // 針の初期位置をプレイヤー位置にセット（針移動開始位置）
        needle.transform.position = transform.position;

        while (Vector2.Distance(needle.transform.position, targetPosition) > NeedleStopDistance)
        {
            // 針の現在位置からターゲットへの単位ベクトルを計算
            Vector2 direction = (targetPosition - (Vector2)needle.transform.position).normalized;

            // 針の向きをターゲット方向に回転させる（デフォルト下向きの針画像に合わせて調整）
            // ここで、針の「up」方向をターゲットの逆方向に向けることで
            // 針の下向き（先端）がターゲット方向を向くようにしている
            needle.transform.up = -direction; // ← ここを修正

            // 針をターゲット方向に少しずつ移動
            needle.transform.position = Vector2.MoveTowards(needle.transform.position, targetPosition, NeedleSpeed);

            yield return null;
        }

        // 針をぴったりターゲット位置に配置
        needle.transform.position = targetPosition;

        // 接続対象オブジェクトを保持
        targetObject = hitObject;

        // 針孔の世界座標を取得
        Vector3 needlePivotWorldPos = needlePivot.position;

        // ワイヤーの見た目を描画
        DrawLine(needlePivotWorldPos);

        // DistanceJoint2D をセットアップ（座標接続）
        distanceJoint.enabled = false; // 安全のため一旦無効化
        distanceJoint.connectedBody = null; // Body ではなく座標接続
        distanceJoint.connectedAnchor = needlePivotWorldPos; // 針孔位置をセット
        distanceJoint.maxDistanceOnly = true; // 最大距離のみ有効
        distanceJoint.distance = FixedWireLength; // 距離を固定
        distanceJoint.enabled = true; // 再度有効化

        // プレイヤーの Rigidbody 設定変更（空気抵抗など調整）
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = PlayerGravityScale;
        rb.linearDamping = RigidbodyLinearDamping;
        rb.angularDamping = RigidbodyAngularDamping;

        // スイング初速を加える
        // 接続方向の法線（垂直方向）を計算し、その方向に力を加えることでスイングを開始
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        Vector2 tangent = new Vector2(-dir.y, dir.x); // 接続線に対する垂直ベクトル
        rb.AddForce(tangent * SwingForce);

        // 現在のワイヤーの発射方向（X軸）を記録しておく（スイング終了時のアニメーション制御用）
        lastSwingDirectionX = dir.x;

        // スイング用アニメーションを発射方向に応じて再生
        animatorController.PlayGrappleSwingAnimation(dir.x);
    }

    /// <summary>
    /// ワイヤーの見た目を LineRenderer で描画。
    /// </summary>
    private void DrawLine(Vector3 lineEndPos)
    {
        if (targetObject == null) return; // 接続対象が無ければ描画しない

        // LineRenderer の点数をセット
        lineRenderer.positionCount = LineRendererPointCount;

        // 始点はプレイヤー
        lineRenderer.SetPosition(LineStartIndex, transform.position);

        // 終点はターゲットオブジェクトの位置
        lineRenderer.SetPosition(LineEndIndex, lineEndPos);  // 針孔の位置に合わせる
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
