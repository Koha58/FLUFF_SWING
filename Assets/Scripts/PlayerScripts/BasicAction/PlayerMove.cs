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
    #region === Inspector設定・依存コンポーネント ===

    /// <summary>ワイヤーアクションの状態を管理するスクリプト</summary>
    [SerializeField] private WireActionScript wireActionScript;

    /// <summary>アニメーション制御用スクリプト</summary>
    [SerializeField] private PlayerAnimatorController animatorController;

    /// <summary>プレイヤーステータスデータ（移動速度など）</summary>
    [SerializeField] private CharacterBase characterData;

    /// <summary>地面判定用のTransform（プレイヤーの足元）</summary>
    [SerializeField] private Transform groundCheck;

    /// <summary>地面判定で判定対象とするレイヤー</summary>
    [SerializeField] private LayerMask groundLayer;

    #endregion


    #region === 内部フィールド ===

    /// <summary>プレイヤーの物理挙動を制御する Rigidbody2D</summary>
    private Rigidbody2D rb;

    /// <summary>移動入力値（-1～1）</summary>
    private float moveInput;

    /// <summary>地上での移動速度（characterDataから取得）</summary>
    private float moveSpeed;

    /// <summary>接地判定用の半径（OverlapCircleなどの判定範囲）</summary>
    private float groundCheckRadius = 0.5f;

    /// <summary>現在プレイヤーが地面に接地しているかのフラグ</summary>
    private bool isGrounded;

    /// <summary>前フレームでの接地状態（状態変化の検知に使用）</summary>
    private bool wasGrounded = false;

    /// <summary>角にハマった際に自動ジャンプするための上方向力</summary>
    private float jumpPower = 3.0f;

    /// <summary>現在接触している地面のカスタムタイル（接地判定時に更新）</summary>
    private CustomTile currentGroundTile;

    /// <summary>移動入力アクション（Input Systemの"Move"）</summary>
    private InputAction moveAction;

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

    /// <summary>
    /// 毎フレームの入力取得と接地判定、アニメーション制御
    /// </summary>
    private void Update()
    {
        // 水平方向の移動入力を取得（-1から1）
        moveInput = moveAction?.ReadValue<Vector2>().x ?? 0f;

        // 接地判定を実施（Raycastで足元をチェック）
        isGrounded = CheckGrounded();

        // 接地状態が変化したらログを出す（デバッグ用）
        if (isGrounded != wasGrounded)
        {
            Debug.Log("接地状態が変化: isGrounded = " + isGrounded);
            wasGrounded = isGrounded;
        }

        // ワイヤー接続中は移動アニメーション停止、そうでなければ入力に応じて更新
        if (wireActionScript.IsConnected)
        {
            animatorController?.ResetMoveAnimation();
        }
        else
        {
            animatorController?.UpdateMoveAnimation(moveInput);
        }
    }

    /// <summary>
    /// 物理演算更新（一定間隔で呼ばれる）
    /// 移動処理やジャンプの補助をここで実行
    /// </summary>
    private void FixedUpdate()
    {
        // ダメージアニメ再生中またはワイヤー接続中は移動不可にする
        if (animatorController.IsDamagePlaying || wireActionScript.IsConnected)
        {
            return;
        }

        if (isGrounded && !wireActionScript.IsConnected)
        {
            // linearVelocityを使って速度を設定（Y方向は保持）
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        if (!isGrounded && Mathf.Abs(rb.linearVelocity.x) < 0.1f && moveInput != 0)
        {
            // 角ハマり対策の自動ジャンプ
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, jumpPower);
        }
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
}
