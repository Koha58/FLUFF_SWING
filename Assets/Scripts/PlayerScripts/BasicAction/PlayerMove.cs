using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/// <summary>
/// プレイヤーの左右移動および接地判定を管理するクラス。
/// - 地面にいるときのみ移動可能
/// - ワイヤー接続中は移動を無効化
/// - 地面との接触判定は Raycast により実施
/// </summary>
[RequireComponent(typeof(Rigidbody2D))] // Rigidbody2D がアタッチされていない場合、自動で追加される
public class PlayerMove : MonoBehaviour
{
    #region === Inspector参照（外部コンポーネント・設定） ===

    /// <summary>
    /// ワイヤーアクションの状態を管理するスクリプト
    /// （接続中かどうかの判定など）
    /// </summary>
    [SerializeField] private WireActionScript wireActionScript;

    /// <summary>
    /// プレイヤーのアニメーション制御クラス
    /// </summary>
    [SerializeField] private PlayerAnimatorController animatorController;

    /// <summary>
    /// キャラクターステータス（移動速度などの定数データ）
    ///</summary>
    [SerializeField] private CharacterBase characterData;

    /// <summary>
    /// 接地判定用のTransform（プレイヤーの足元）
    /// </summary>
    [SerializeField] private Transform groundCheck;

    /// <summary>
    /// 地面として判定するレイヤー
    /// </summary>
    [SerializeField] private LayerMask groundLayer;

    #endregion


    #region === 物理・入力 ===

    /// <summary>
    /// プレイヤーの物理挙動を制御する Rigidbody2D
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// 移動入力値（-1 ～ 1）
    /// </summary>
    private float moveInput;

    /// <summary>
    /// 地上での移動速度（CharacterBase から取得）
    /// </summary>
    private float moveSpeed;

    /// <summary>
    /// Input System の Move アクション
    /// </summary>
    private InputAction moveAction;

    #endregion


    #region === 接地判定 ===

    /// <summary>
    /// 接地判定に使用する Raycast の距離（半径）
    /// </summary>
    private float groundCheckRadius = 0.5f;

    /// <summary>
    /// 現在フレームで地面に接地しているか
    /// </summary>
    private bool isGrounded;

    /// <summary>
    /// 前フレームの接地状態
    /// （着地・離地の瞬間検知用）
    /// </summary>
    private bool wasGrounded;

    /// <summary>
    /// 現在接触している地面タイル（CustomTile）
    /// null の場合は通常地面
    /// </summary>
    private CustomTile currentGroundTile;

    #endregion


    #region === 移動補助・状態管理 ===

    /// <summary>
    /// 角に引っかかった際の救済用ジャンプ力
    /// </summary>
    private float jumpPower = 3.0f;

    /// <summary>
    /// Start 初期化完了フラグ
    /// （初期フレームでの誤判定防止）
    /// </summary>
    private bool isInitialized = false;

    #endregion



    #region === Unityイベントメソッド ===

    /// <summary>
    /// コンポーネント初期化処理
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Input SystemからMoveアクションを取得して有効化
        moveAction = InputSystem.actions.FindAction("Move");
        moveAction?.Enable();

        // ステータスから移動速度を取得
        moveSpeed = characterData.moveSpeed;
    }

    private void Start()
    {
        // 1. 最初の接地状態をチェックし、wasGroundedを初期化する
        // これにより、ゲーム開始時の最初のフレームで「空中から着地した」という誤判定を防ぐ
        wasGrounded = CheckGrounded();

        Debug.Log($"PlayerMove initialized. Initial wasGrounded: {wasGrounded}");

        // 2. 初期化完了フラグを立てる（これで次フレームからUpdateが有効になる）
        isInitialized = true;
    }

    /// <summary>
    /// 毎フレームの入力取得と接地判定、アニメーション制御
    /// </summary>
    private void Update()
    {
        // 初期化が完了していない場合は、着地判定を無視する
        // Start() 処理が完了するまで Update の主要ロジックをブロックします。
        if (!isInitialized)
        {
            // ただし、Input取得は続けても問題ない
            moveInput = moveAction?.ReadValue<Vector2>().x ?? 0f;
            return;
        }

        // 水平方向の移動入力を取得（-1から1）
        moveInput = moveAction?.ReadValue<Vector2>().x ?? 0f;

        // 接地判定を実施（Raycastで足元をチェック）
        isGrounded = CheckGrounded();

        // 現在Landingアニメーション再生中なら、再トリガーしない
        bool isLanding = animatorController?.CurrentState == PlayerAnimatorController.PlayerState.Landing;

        // ワイヤー接続中でないことを確認してから Landing を強制開始
        if (isGrounded && !wasGrounded)
        {
            // Landingアニメーション再生中は、リトリガーしないことでコルーチンのリセットを防ぐ
            if (isLanding)
            {
                Debug.Log("Landing中に再接地を検知したが、アニメーションをリセットしません。");
            }
            // Landing中でなければ、通常通り Landing を開始する
            // ※ ワイヤー接続中は着地扱いにしないため除外
            else if (!wireActionScript.IsConnected)
            {
                // デバッグ用ログ：空中 → 接地した「瞬間」を検知
                Debug.Log("着地瞬間: Landingアニメーションを強制開始");

                // 現在のスプライトの向きから着地方向を決定
                // （localScale.x > 0 : 右向き / < 0 : 左向き）
                float directionX = transform.localScale.x > 0 ? 1f : -1f;

                // 次の Landing アニメーション中に
                // 着地SEを「1回だけ」鳴らすことを許可する
                // （空中フレームでの誤発火・多重再生防止用）
                animatorController?.AllowLandingSEOnce();

                // Landing アニメーションを強制的に開始
                // ・Jump / Wire / 空中状態から必ず Landing に遷移させる
                // ・Idle への自動遷移は AnimatorController 側で制御
                animatorController?.ForceLanding(directionX);
            }

        }

        // 接地状態が変化したらログを出す（デバッグ用）
        if (isGrounded != wasGrounded)
        {
            Debug.Log("接地状態が変化: isGrounded = " + isGrounded);
            wasGrounded = isGrounded;
        }

        // ワイヤー接続中は移動アニメーション停止、そうでなければ入力に応じて更新
        if (wireActionScript.IsConnected)
        {
            // ワイヤー接続中はIdle/Runアニメーションへの遷移を確実にブロック
            // アニメーションをWireに固定する処理は WireActionScript または SetPlayerState の優先度制御に任せるのが理想的
            animatorController?.ResetMoveAnimation(); // ← この呼び出しは、もしRun/Idleに戻るなら不要、またはWireステート維持のための処理が必要です。
        }
        else
        {
            // Landing アニメーション実行中は Run/Idle への上書きを避ける
            if (animatorController?.CurrentState != PlayerAnimatorController.PlayerState.Landing)
            {
                animatorController?.UpdateMoveAnimation(moveInput);
            }
        }
    }


    private void FixedUpdate()
    {
        // --- 優先度1: 最優先の強制停止 ---
        // ワイヤー接続中は、このクラスでの物理移動（rb.linearVelocityの操作）を完全に停止し、
        // ワイヤーアクション側の制御に任せる。
        if (wireActionScript.IsConnected)
        {
            return;
        }

        // ダメージアニメ再生中も移動を停止（ワイヤー中でないことを確認してから）
        if (animatorController.IsDamagePlaying)
        {
            // 移動を止め、縦速度は維持
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // --- 優先度2: アニメーション状態による移動制限 ---

        // 攻撃中は移動入力をゼロにする（FixedUpdateの後半で速度を0にするために）
        if (animatorController.IsAttacking)
        {
            // 攻撃中の移動を許可しないため、moveInputを一時的に0に上書き
            // ただし、この後の CanMoveNow() のチェックに依存するため、ここでは return せず moveInput = 0f のみ行う
            moveInput = 0f;
        }

        // ここでアニメーション状態をチェックして移動を制御
        if (!CanMoveNow())
        {
            // Landing, Attack など、移動が禁止されているステートの場合
            // 移動を止める（横速度を0に）
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // --- 優先度3: 通常の移動処理（CanMoveNowがTrueの場合のみ） ---

        // 既にワイヤー接続中でないことは上のチェックで確認済み
        if (isGrounded)
        {
            // 通常の地上移動処理
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        // isGrounded == false (空中) で、移動入力をしているのに横速度がほぼ0の場合
        if (!isGrounded && Mathf.Abs(rb.linearVelocity.x) < 0.1f && moveInput != 0)
        {
            // 角ハマり対策の自動ジャンプ（空中でのわずかな加速も兼ねる）
            // 速度を上書きするのではなく、横方向の加速を加える形にするのが一般的だが、
            // このコードでは速度を直接設定しています。現在の仕様を尊重します。
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, jumpPower);
        }
    }

    /// <summary>
    /// 現在のアニメーション状態から「移動して良いか」を判定する。
    /// </summary>
    private bool CanMoveNow()
    {
        if (animatorController == null) return true;

        var state = animatorController.CurrentState;

        if (state == PlayerAnimatorController.PlayerState.Landing && !isGrounded)
            return true;

        // ❌ 移動禁止ステート一覧
        switch (state)
        {
            case PlayerAnimatorController.PlayerState.MeleeAttack:
            case PlayerAnimatorController.PlayerState.RangedAttack:
            case PlayerAnimatorController.PlayerState.Landing:
            case PlayerAnimatorController.PlayerState.Damage:
            case PlayerAnimatorController.PlayerState.Goal:
                return false;
        }

        return true; // それ以外（Idle, Run, Jumpなど）は移動OK
    }

    #endregion


    #region === 接地判定関連メソッド ===

    /// <summary>
    /// Raycastを使ってプレイヤー足元の地面を判定し、地面に接しているかを返す。
    /// また接触しているカスタムタイルを更新する。
    /// </summary>
    /// <returns>地面に接地していればtrue、そうでなければfalse</returns>
    private bool CheckGrounded()
    {
        Vector3 checkPos = groundCheck.position;

        // 下方向にRaycastを飛ばして地面レイヤーに当たるか判定
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, groundCheckRadius, groundLayer);

        if (hit.collider != null)
        {
            // --- 接地判定成功 ---
            // 接触したコライダーのタグで判定を追加
            if (hit.collider.CompareTag("WireConnectable")) // 🎯 タグが "WireConnectable" の場合
            {
                // Tilemap 以外の地面
                currentGroundTile = null; // CustomTileではないためnullにリセット
                return true;
            }

            // Tilemapを取得し、当たったポイントのタイル情報を取得する
            Tilemap tilemap = hit.collider.GetComponent<Tilemap>();

            if (tilemap != null)
            {
                // ワールド座標をタイル座標に変換
                Vector3Int cell = tilemap.WorldToCell(hit.point);

                // タイルを取得し、CustomTileかどうか判定
                TileBase tile = tilemap.GetTile(cell);
                if (tile is CustomTile customTile)
                {
                    currentGroundTile = customTile;
                }
                else
                {
                    currentGroundTile = null;
                }

                return true;
            }
        }

        currentGroundTile = null;
        return false;
    }

    /// <summary>
    /// 地面までの距離を返す（Raycast結果の距離）
    /// </summary>
    public float DistanceToGround
    {
        get
        {
            Vector3 checkPos = groundCheck.position;
            RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, groundCheckRadius * 2f, groundLayer);
            return hit.collider != null ? hit.distance : Mathf.Infinity;
        }
    }

    /// <summary>
    /// 「ほぼ接地」判定。距離が閾値以内ならtrue
    /// </summary>
    public bool IsAlmostGrounded(float threshold = 0.08f)
    {
        // Groundedがtrueなら常にtrueを返す（フレーム遅延対策）
        if (isGrounded) return true;
        return DistanceToGround < threshold;
    }

    #endregion


    #region === プロパティ（外部参照用） ===

    /// <summary>
    /// 現在接触しているタイルの種類（地面の種類を判別可能）
    /// nullの場合は地面なし
    /// </summary>
    public CustomTile.TileType? CurrentGroundType => currentGroundTile?.tileType;

    /// <summary>
    /// 現在接触しているタイルのインスタンス
    /// </summary>
    public CustomTile CurrentGroundTile => currentGroundTile;

    /// <summary>
    /// プレイヤーが現在地面に接地しているかどうか
    /// </summary>
    public bool IsGrounded => isGrounded;

    #endregion


    #region === 動く床に接地しているか判定 ===

    // 動く床に乗っているかどうかの判定フラグ
    public bool IsOnMoveFloor { get; private set; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("WireConnectable"))
        {
            IsOnMoveFloor = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("WireConnectable"))
        {
            IsOnMoveFloor = false;
        }
    }

    #endregion
}
