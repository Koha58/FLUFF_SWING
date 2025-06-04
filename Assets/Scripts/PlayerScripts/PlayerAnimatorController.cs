using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

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

    // Animatorパラメータ名の定義
    private static class AnimatorParams
    {
        public const string State = "State";
        public const string SpeedMultiplier = "speedMultiplier";
    }

    /// <summary>
    /// プレイヤーの状態を表すEnum。AnimatorのStateパラメータと連動。
    /// </summary>
    public enum PlayerState
    {
        Idle = 0,
        Run = 1,
        Jump = 2,
        Wire = 3,
        Landing = 4,
    }


    /// <summary>
    /// 現在のステート（状態）
    /// </summary>
    private PlayerState _currentState = PlayerState.Idle;

    /// <summary>
    /// 指定されたプレイヤーステートに応じてAnimatorを制御する。
    /// 状態遷移は基本的に Run → Jump → Wire → Landing → Idle の流れを想定。
    /// </summary>
    /// <param name="newState">遷移先の状態</param>
    /// <param name="direction">プレイヤーの向き（-1〜1）</param>
    /// <param name="speed">移動速度（0〜1の正規化値）</param>
    public void SetPlayerState(PlayerState newState, float direction = 0f, float speed = 0f, bool force = false)
    {
        if (!force && _currentState == newState)
            return;

        var oldState = _currentState;
        _currentState = newState;
        Debug.Log($"[SetPlayerState] Transitioning from {oldState} to {newState}");

        // Animatorにステートを整数で渡す
        _animator.SetInteger(AnimatorParams.State, (int)newState);

        // 向き反転も反映
        FlipSprite(direction);

        // 状態ごとの速度設定（必要に応じて）
        switch (newState)
        {
            case PlayerState.Run:
                // 移動速度に応じてアニメーション再生速度をリニア補間
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(speed));
                float moveSpeed = Mathf.Lerp(AnimatorSpeeds.RunMin, AnimatorSpeeds.RunMax, normalizedSpeed);
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, moveSpeed);
                break;

            case PlayerState.Jump:
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Grapple);
                break;

            case PlayerState.Wire:
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Swing);
                break;

            case PlayerState.Landing:
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Landing);
                break;

            default:
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Idle);
                break;
        }
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
        public const float Landing = 1.0f;   // 着地時の再生速度
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

    private bool _pendingWireTransition = false; // Wireに遷移待ちかどうか
    private float _wireDirection = 0f;           // Wire遷移時の向きを保持

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
                //SetPlayerState(PlayerState.Idle, 1.0f, 0f);

                if (_pendingWireTransition)
                {
                    // Wire状態に遷移
                    Debug.Log($"Animator State Set: {_currentState} ");
                    SetPlayerState(PlayerState.Wire, _wireDirection, 0f, true);
                    Debug.Log("Wire SpeedMultiplier: " + AnimatorSpeeds.Swing);
                    _pendingWireTransition = false;
                }
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
        if (_pendingWireTransition) return; // Wire遷移保留中はアニメを更新しない
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

        // プレイヤースプライトの向きを入力に合わせて反転
        FlipSprite(moveInput);

        // 実際の移動速度に応じてアニメーション再生速度を調整（スムーズに見せる）
        if (_isMoving)
        {
            SetPlayerState(PlayerState.Run, moveInput, Mathf.Abs(moveInput));
        }
        else
        {
            if (_currentState != PlayerState.Landing && _currentState != PlayerState.Wire)
            {
                SetPlayerState(PlayerState.Idle, moveInput, 0f);
            }
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

        SetPlayerState(PlayerState.Jump, swingDirection);

        // Wire への遷移を保留
        _pendingWireTransition = true;
        _wireDirection = swingDirection;
    }

    /// <summary>
    /// スイングが終了したときに呼び出され、滞空・静止アニメーションへと遷移する。
    /// </summary>
    /// <param name="swingDirection">最後のスイング方向。向きの維持に使用。</param>
    public void StopSwingAnimation(float swingDirection)
    {
        // まず Landing に遷移
        SetPlayerState(PlayerState.Landing, swingDirection, 0f, true);

        // 一定時間後に Idle に遷移する処理をここに加える（Coroutineを使うのが理想）
        StartCoroutine(TransitionToIdleAfterLanding(swingDirection));
    }

    private IEnumerator TransitionToIdleAfterLanding(float direction)
    {
        yield return new WaitForSeconds(0.2f); // 着地演出の時間を調整
        SetPlayerState(PlayerState.Idle, direction);
    }

    /// <summary>
    /// ワイヤー接続時に移動アニメーションを強制的に停止させる。
    /// </summary>
    public void ResetMoveAnimation(float swingDirection)
    {
        _moveStopTimer = 0f;

        // ワイヤー中はIdleに戻さない
        if (_currentState == PlayerState.Wire)
            return;

        SetPlayerState(PlayerState.Idle, swingDirection);
    }

    /// <summary>
    /// ワイヤー接続からすぐに切断された際、ジャンプアニメーションが意図せずループするのを防ぐため、
    /// 手動でジャンプ状態（IsJumping）をリセットする。
    /// </summary>
    public void UpdateJumpState(float swingDirection)
    {
        Debug.Log("UpdateJumpState called");
        SetPlayerState(PlayerState.Landing, 0f, 0f, true);
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
