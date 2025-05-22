using UnityEngine;

/// <summary>
/// プレイヤーのアニメーションを制御するクラス。
/// プレイヤーの状態（移動、ジャンプ、スイングなど）に応じてAnimatorのパラメータを更新する。
/// スプライトの向き変更（左右反転）もここで行う。
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator _animator;

    // --- Animatorパラメータ名 ---
    private static class AnimatorParams
    {
        public const string IsRunning = "isRunning";       // 走っている状態
        public const string IsJumping = "isJumping";       // ジャンプ中状態
        public const string IsSwinging = "isSwinging";     // 振り子のようにぶら下がっている状態（ワイヤーでのスイング）
        public const string IsStaying = "isStaying";       // 静止状態
        public const string JustGrappled = "justGrappled"; // ワイヤーに掴まった直後の演出状態
    }

    // --- 定数 ---
    private const float MoveThreshold = 0.05f;             // 微小な入力を無視するための閾値
    private const float MoveDelayTime = 0.1f;              // 停止とみなすまでの待機時間
    private const float GrappleTransitionTime = 0.3f;      // ワイヤーに掴まった状態を維持する時間
    private const float FlipThreshold = 0.01f;             // 向きを変える最低入力値

    // --- 移動状態管理 ---
    private bool _isMoving = false;     // プレイヤーが移動中かどうかの状態
    private float _moveStopTimer = 0f; // 移動停止を判定するためのタイマー

    // --- ワイヤー掴まり状態管理 ---
    private bool _justGrappled = false; // ワイヤーに掴まった直後かどうかのフラグ
    private float _grappleTimer = 0f;   // ワイヤー掴まり演出の時間管理タイマー

    private void Awake()
    {
        // Animatorコンポーネントの取得
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // ワイヤーに掴まった直後の演出用フラグを時間経過で解除
        if (_justGrappled)
        {
            // 経過時間を減らす
            _grappleTimer -= Time.deltaTime;

            // タイマーが0以下になったら演出フラグを解除
            if (_grappleTimer <= 0f)
            {
                _justGrappled = false;
                _animator.SetBool(AnimatorParams.JustGrappled, false);
            }
        }
    }

    /// <summary>
    /// 移動入力に基づき、「走り」アニメーションを制御する。
    /// 急停止などの小刻みな揺れを無視するために、停止には少し遅延を持たせて自然な挙動にする。
    /// </summary>
    /// <param name="moveInput">入力された水平移動値（-1〜1）</param>
    public void UpdateMoveAnimation(float moveInput)
    {
        if (Mathf.Abs(moveInput) > MoveThreshold)
        {
            // 入力が閾値を超えている＝移動中と判定
            _isMoving = true;

            // 停止判定用のタイマーをリセット
            _moveStopTimer = 0f;
        }
        else
        {
            // 入力が閾値以下＝停止中に近いのでタイマーを進める
            _moveStopTimer += Time.deltaTime;

            // タイマーが設定時間を超えたら停止と判定
            if (_moveStopTimer > MoveDelayTime)
            {
                _isMoving = false;
            }
        }

        // Animatorに移動状態を反映
        _animator.SetBool(AnimatorParams.IsRunning, _isMoving);

        // プレイヤースプライトの向きを入力に合わせて反転
        FlipSprite(moveInput);
    }

    /// <summary>
    /// ワイヤーに掴まった時のアニメーション制御。
    /// スイング状態への遷移とジャンプ状態への設定を行う。
    /// プレイヤーの向きも変更。
    /// </summary>
    /// <param name="swingDirection">スイング方向（X方向の速度や入力）</param>
    public void PlayGrappleSwingAnimation(float swingDirection)
    {
        // ワイヤーに引っかかった直後のフラグを立て、遷移用タイマーをセット
        _justGrappled = true;
        _grappleTimer = GrappleTransitionTime;

        // ジャンプ・スイングアニメーションに遷移
        _animator.SetBool(AnimatorParams.IsJumping, true);
        _animator.SetBool(AnimatorParams.IsSwinging, true);

        // ワイヤーに掴まった直後の演出フラグをAnimatorにセット
        _animator.SetBool(AnimatorParams.JustGrappled, true);

        // プレイヤースプライトの向きをスイング方向に合わせる
        FlipSprite(swingDirection);
    }

    /// <summary>
    /// スイングが終了したときに呼び出され、滞空・静止アニメーションへと遷移する。
    /// </summary>
    /// <param name="swingDirection">最後のスイング方向。向きの維持に使用。</param>
    public void StopSwingAnimation(float swingDirection)
    {
        // スイングアニメーションを終了
        _animator.SetBool(AnimatorParams.IsSwinging, false);

        // 静止状態に遷移
        _animator.SetBool(AnimatorParams.IsStaying, true);

        // ジャンプ状態を解除
        _animator.SetBool(AnimatorParams.IsJumping, false);

        // プレイヤースプライトの向きを最後のスイング方向に合わせる
        FlipSprite(swingDirection);
    }

    /// <summary>
    /// プレイヤーのスプライトを入力方向に合わせて左右反転する。
    /// スプライトが「X+方向で左を向く」前提で設計。
    /// </summary>
    /// <param name="moveInput">移動またはスイングのX方向入力</param>
    private void FlipSprite(float moveInput)
    {
        // 入力の絶対値が小さい場合は反転処理を行わない（不要なスケール変更を防ぐ）
        if (Mathf.Abs(moveInput) > FlipThreshold)
        {
            // 現在のスケールを取得
            Vector3 scale = transform.localScale;

            // X軸の符号を入力の符号と逆にして左右反転（スプライトの向きを合わせる）
            scale.x = -Mathf.Sign(moveInput) * Mathf.Abs(scale.x);

            // スケールを適用
            transform.localScale = scale;
        }
    }
}