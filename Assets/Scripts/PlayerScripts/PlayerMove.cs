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
    #region 依存スクリプト・構成要素

    // ワイヤーアクションの状態（接続状態など）を管理するスクリプト
    [SerializeField] private WireActionScript wireActionScript;

    // アニメーション制御スクリプト
    [SerializeField] private PlayerAnimatorController animatorController;

    // ステータス管理スクリプト
    [SerializeField] private CharacterBase characterData;

    // プレイヤーの物理挙動を制御する Rigidbody2D
    private Rigidbody2D rb;

    // 入力アクション（"Move"）
    private InputAction moveAction;

    #endregion


    #region 移動関連

    // 地上での左右移動スピード
    private float moveSpeed;

    // プレイヤーの左右入力値（-1 ～ 1）
    private float moveInput;

    #endregion


    #region 接地判定関連

    [Header("Ground Check Settings")]

    // 地面チェック用の基準点（プレイヤー足元）
    [SerializeField] private Transform groundCheck;

    // 地面と判定するレイヤー（LayerMask）
    [SerializeField] private LayerMask groundLayer;

    // 接地判定に使用する円の半径（OverlapCircle用）
    private float groundCheckRadius = 0.5f;

    // 現在プレイヤーが地面に接地しているかどうか
    private bool isGrounded;

    // 前のフレームでの接地状態
    private bool wasGrounded = false;

    // 角にハマった際に自動ジャンプするための上方向力
    private float jumpPower = 3.0f;

    #endregion


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Input System から "Move" アクションを取得
        moveAction = InputSystem.actions.FindAction("Move");
        moveAction?.Enable(); // 入力受付を有効化

        // characterData から moveSpeedを取得
        moveSpeed = characterData.moveSpeed;
    }

    private void Update()
    {
        // 入力の取得（A/Dキーや左スティックによる水平方向の入力）
        moveInput = moveAction?.ReadValue<Vector2>().x ?? 0f;

        // 接地判定を実施
        isGrounded = CheckGrounded();

        // 接地状態の変化をログ出力
        if (isGrounded != wasGrounded)
        {
            Debug.Log("接地状態が変化: isGrounded = " + isGrounded);
            wasGrounded = isGrounded;
        }

        // ワイヤーに接続中は移動アニメーション停止
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
    /// 接地状態を判定するためのメソッド
    /// Raycastで下方向に線を飛ばし、地面との衝突を確認する
    /// </summary>
    /// <returns>地面に接しているかどうか</returns>
    private bool CheckGrounded()
    {
        Vector3 checkPos = groundCheck.position;

        // 下方向にRaycastを飛ばし、一定距離内で地面レイヤーに当たるか調査
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, groundCheckRadius, groundLayer);

        // 当たっていて、Tilemapが存在する場合、地面と見なす
        return hit.collider != null && hit.collider.GetComponent<Tilemap>() != null;
    }

    /// <summary>
    /// 物理演算の処理（FixedUpdateは一定間隔で呼ばれる）
    /// </summary>
    private void FixedUpdate()
    {
        // ワイヤーに接続中は一切の移動を無効にする
        if (wireActionScript.IsConnected)
        {
            return;
        }

        // 接地していて、かつワイヤーに接続されていないときのみ移動可能
        if (isGrounded && !wireActionScript.IsConnected)
        {
            // 左右移動を反映（Y方向の速度は保持）
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        if (!isGrounded && Mathf.Abs(rb.linearVelocity.x) < 0.1f && moveInput != 0)
        {
            // 角にハマってるようなときに自動で小ジャンプ
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, jumpPower);
        }

    }
}
