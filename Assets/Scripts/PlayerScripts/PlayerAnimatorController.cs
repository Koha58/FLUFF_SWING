using UnityEngine;

/// <summary>
/// プレイヤーのアニメーションを制御するクラス。
/// プレイヤーの状態（移動、ジャンプ、スイングなど）に応じてAnimatorのパラメータを更新する。
/// スプライトの向き変更（左右反転）もここで行う。
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    #region アニメーター本体

    // アニメーター本体（Animatorコンポーネント）
    private Animator _animator;

    #endregion


    #region アニメーターパラメータ名

    // Animator パラメータ名の定義クラス
    private static class AnimatorParams
    {
        public const string IsRunning = "isRunning";             // 走っている状態
        public const string IsJumping = "isJumping";             // ジャンプ中状態
        public const string IsSwinging = "isSwinging";           // スイング中状態（ワイヤー）
        public const string IsStaying = "isStaying";             // 静止状態
        public const string SpeedMultiplier = "speedMultiplier"; // アニメーション速度調整
    }

    #endregion


    #region アニメーション再生速度定数

    // 各状態に対応するアニメーション速度設定
    private static class AnimatorSpeeds
    {
        public const float RunMin = 0.5f;    // 走りアニメの最低速度
        public const float RunMax = 3.0f;    // 走りアニメの最大速度
        public const float Swing = 1.5f;     // スイング中の再生速度
        public const float Grapple = 1.5f;   // 掴まり直後の再生速度
        public const float Idle = 1.0f;      // 静止時の再生速度
    }

    #endregion


    #region Constants

    // 動作検出や演出に使う閾値やタイミング
    private const float MoveThreshold = 0.05f;        // 微小入力を無視するしきい値
    private const float MoveDelayTime = 0.1f;         // 停止とみなすまでの時間
    private const float GrappleTransitionTime = 0.3f; // 掴まり演出の維持時間
    private const float FlipThreshold = 0.01f;        // 向きを反転するための最小入力値

    #endregion


    #region プレイヤー移動状態管理

    // プレイヤーの移動状態管理
    private bool _isMoving = false;     // プレイヤーが移動中かどうか
    private float _moveStopTimer = 0f;  // 停止判定用タイマー

    #endregion


    #region ワイヤー掴まり状態管理

    // ワイヤー掴まり直後の状態管理
    private bool _justGrappled = false; // 掴まった直後かどうか
    private float _grappleTimer = 0f;   // 掴まり状態の残り演出時間

    #endregion


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
                _animator.SetBool(AnimatorParams.IsJumping, false);
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

        // 実際の移動速度に応じてアニメーション再生速度を調整（スムーズに見せる）
        if (_isMoving)
        {
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(moveInput)); // -1〜1 → 0〜1
            float moveSpeed = Mathf.Lerp(AnimatorSpeeds.RunMin, AnimatorSpeeds.RunMax, normalizedSpeed);
            _animator.SetFloat(AnimatorParams.SpeedMultiplier, moveSpeed);
        }
        else
        {
            _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Idle);
        }
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

        // 掴まり直後の演出速度を設定
        _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Grapple);

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

        // スイング終了後は通常速度に戻す
        _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Idle);

        // プレイヤースプライトの向きを最後のスイング方向に合わせる
        FlipSprite(swingDirection);
    }

    /// <summary>
    /// ワイヤー接続時に移動アニメーションを強制的に停止させる。
    /// </summary>
    public void ResetMoveAnimation()
    {
        _isMoving = false;
        _moveStopTimer = 0f;
        _animator.SetBool(AnimatorParams.IsRunning, false);
        _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Idle);
    }

    /// <summary>
    /// ワイヤー接続からすぐに切断された際、ジャンプアニメーションが意図せずループするのを防ぐため、
    /// 手動でジャンプ状態（IsJumping）をリセットする。
    /// </summary>
    public void UpdateJumpState()
    {
        _animator.SetBool(AnimatorParams.IsJumping, false);
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
