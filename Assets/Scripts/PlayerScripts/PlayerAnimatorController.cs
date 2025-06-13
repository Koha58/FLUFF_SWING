using System.Collections;
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
        MeleeAttack = 5,
        RangedAttack = 6,
        Damage = 7,
        Goal = 8
    }

    /// <summary>
    /// 現在のステート（状態）
    /// </summary>
    private PlayerState _currentState = PlayerState.Idle;

    /// <summary>
    /// 指定されたプレイヤーステートに応じてAnimatorを制御する。
    /// 状態遷移は基本的に Run → Jump → Wire → Landing → Idle の流れを想定。
    /// <br/>
    /// 【force引数について】
    /// forceがtrueの場合は、現在の状態と同じステートでも強制的に状態遷移を行う。
    /// 通常は同じ状態への遷移は無視されるため、再アニメーション再生やパラメータ更新を明示的に行いたい場合に使用する。
    /// </summary>
    /// <param name="newState">遷移先の状態</param>
    /// <param name="direction">プレイヤーの向き（-1〜1）</param>
    /// <param name="speed">移動速度（0〜1の正規化値）</param>
    /// <param name="force">同じ状態でも強制的に遷移処理を行うかどうか</param>
    public void SetPlayerState(PlayerState newState, float direction = 0f, float speed = 0f, bool force = false)
    {
        // 現在の状態と同じ場合、forceがfalseなら処理を抜ける
        if (!force && _currentState == newState)
            return;

        var oldState = _currentState;
        _currentState = newState;
        Debug.Log($"[SetPlayerState] Transitioning from {oldState} to {newState}");

        // 攻撃中フラグ設定
        _isAttacking = (newState == PlayerState.MeleeAttack || newState == PlayerState.RangedAttack);

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

            case PlayerState.MeleeAttack:
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.MeleeAttack);
                break;

            case PlayerState.RangedAttack:
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.RangedAttack);
                break;

            case PlayerState.Damage:
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Damage);
                break;

            case PlayerState.Goal:
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Goal);
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
        // 走りアニメの最低速度
        public const float RunMin = 0.5f;

        // 走りアニメの最大速度
        public const float RunMax = 3.0f;

        // スイング中の再生速度
        public const float Swing = 1.5f;

        // 掴まり直後の再生速度
        public const float Grapple = 1.5f;

        // 着地時の再生速度
        public const float Landing = 1.0f;

        // 近距離攻撃の再生速度
        public const float MeleeAttack = 1.0f;

        // 遠距離攻撃の再生速度
        public const float RangedAttack = 1.0f;

        // 被弾時の再生速度
        public const float Damage = 1.0f;

        // クリアアニメ時の再生速度
        public const float Goal = 1.0f;

        // 静止時の再生速度
        public const float Idle = 1.0f;　　　　　　
    }

    #endregion


    #region Constants

    // 動作検出や演出に使う閾値やタイミング
    private const float MoveThreshold = 0.05f;        // 微小入力を無視するしきい値
    private const float MoveDelayTime = 0.1f;         // 停止とみなすまでの時間
    private const float GrappleTransitionTime = 0.3f; // 掴まり演出の維持時間
    private const float FlipThreshold = 0.01f;        // 向きを反転するための最小入力値
    private const float LandingToIdleDelay = 0.2f;    // 着地アニメーション後Idleに遷移するまでの遅延時間

    #endregion


    #region プレイヤー移動状態管理

    // プレイヤーが現在移動しているかどうかを示すフラグ（左右入力により移動している状態）
    private bool _isMoving = false;

    // 移動を停止してから経過した時間を記録するタイマー（Idle遷移判定などに使用）
    private float _moveStopTimer = 0f;

    // 着地後に自動的にIdle状態へ遷移する処理を一時的にキャンセルするためのフラグ
    // ワイヤーなどのアクション中はIdleに戻らないようにする
    private bool _cancelIdleTransition = false;

    // プレイヤーがワイヤーアクション状態へ遷移するのを保留しているかどうか
    // 攻撃中や着地中など、他の状態が完了するのを待ってから遷移させるために使用
    private bool _pendingWireTransition = false;

    // ワイヤーアクションに遷移する際の移動方向（1:右、-1:左）を保持
    // 遷移時にプレイヤーの向きや初速などに利用される
    private float _wireDirection = 0f;

    // プレイヤーが現在攻撃中かどうかを示すフラグ
    // 攻撃中は他の行動（移動やジャンプなど）を制限するために使用
    private bool _isAttacking = false;

    [SerializeField] private PlayerAttack playerAttack;

    public void ThrowBombEvent()
    {
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        playerAttack.ThrowBomb(direction);

    }

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

        if (_isAttacking) return;

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

        // Idleに遷移中だった場合も、強制的に中断してJump → Wire の流れへ
        StopAllCoroutines(); // ← Idle 遷移用コルーチンを止める

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
        Debug.Log("[StopSwingAnimation] called. CurrentState: " + _currentState);

        // Wire遷移保留をクリアする
        _pendingWireTransition = false;
        _justGrappled = false;
        // まず Landing に遷移
        SetPlayerState(PlayerState.Landing, swingDirection, 0f, true);

        // 一定時間後に Idle に遷移する処理をここに加える（Coroutineを使うのが理想）
        StartCoroutine(TransitionToIdleAfterLanding(swingDirection));
    }

    /// <summary>
    /// 着地後のIdle遷移を外部からキャンセルするためのメソッド。
    /// 例えば、ワイヤーに再接続して再ジャンプするなどの割り込み処理が発生した場合に呼び出す。
    /// このフラグが立つと、Idleへの遷移を一時的に止め、自然な状態遷移を妨げないようにする。
    /// </summary>
    public void CancelPendingIdleTransition()
    {
        _cancelIdleTransition = true;
        Debug.Log("[PlayerAnimatorController] Idle遷移をキャンセルしました");
    }

    /// <summary>
    /// ワイヤー接続からすぐに切断された際、ジャンプアニメーションが意図せずループするのを防ぐため、
    /// 手動でジャンプ状態（IsJumping）をリセットする。
    /// </summary>
    public void UpdateJumpState(float swingDirection)
    {
        Debug.Log("UpdateJumpState called");
        SetPlayerState(PlayerState.Landing, swingDirection, 0f, true);
        Debug.Log("Animator current state int: " + _animator.GetInteger(AnimatorParams.State));
    }

    /// <summary>
    /// 近距離攻撃のアニメーション制御。
    /// プレイヤーの向きも変更。
    /// </summary>
    /// <param name="direction">プレイヤーの向き（X方向）</param>
    public void PlayMeleeAttackAnimation(float direction)
    {
        if (_animator == null)
        {
            Debug.LogError("animatorController is NULL!");
        }
        Debug.Log("PlayMeleeAttackAnimation called");
        SetPlayerState(PlayerState.MeleeAttack, direction);
    }

    /// <summary>
    /// 近接攻撃アニメーションの終了時にアニメーションイベントから呼ばれるメソッド。
    /// 攻撃状態フラグを解除し、プレイヤーの状態をIdleに戻す。
    /// </summary>
    public void OnMeleeAttackAnimationEnd()
    {
        // アニメーションイベントから呼ばれたことをデバッグログに出力
        Debug.Log("[AnimationEvent] MeleeAttack animation finished.");

        // 攻撃中フラグを解除。これにより他の行動が可能になる。
        _isAttacking = false;

        // プレイヤーの状態をIdleに戻す。
        // 引数: PlayerState.Idle（状態）、0f（横速度）、0f（縦速度）、true（強制的に状態を変更する）
        SetPlayerState(PlayerState.Idle, 0f, 0f, true);
    }

    /// <summary>
    /// 遠距離攻撃のアニメーション制御。
    /// プレイヤーの向きも変更。
    /// </summary>
    /// <param name="direction">プレイヤーの向き（X方向）</param>
    public void PlayRangedAttackAnimation(float direction)
    {
        if (_animator == null)
        {
            Debug.LogError("animatorController is NULL!");
        }
        Debug.Log("PlayRangedAttackAnimation called");
        SetPlayerState(PlayerState.RangedAttack, direction);
    }

    /// <summary>
    /// 遠距離攻撃アニメーションの終了時にアニメーションイベントから呼ばれるメソッド。
    /// 攻撃状態フラグを解除し、プレイヤーの状態をIdleに戻す。
    /// </summary>
    public void OnRangedAttackAnimationEnd()
    {
        // アニメーションイベントから呼ばれたことをデバッグログに出力
        Debug.Log("[AnimationEvent] RangedAttack animation finished.");

        // 攻撃中フラグを解除。これにより他の行動が可能になる。
        _isAttacking = false;

        // プレイヤーの状態をIdleに戻す。
        // 引数: PlayerState.Idle（状態）、0f（横速度）、0f（縦速度）、true（強制的に状態を変更する）
        SetPlayerState(PlayerState.Idle, 0f, 0f, true);
    }

    /// <summary>
    /// 着地アニメーション後、一定時間待ってからIdle状態に遷移させるコルーチン。
    /// 着地演出の時間を確保しつつ、自然な状態遷移を実現する。
    /// 特殊な遷移（ワイヤー再接続など）が割り込んだ場合は、Idle遷移を一度キャンセルし、
    /// 少し待ってから再度Idle遷移を試みる。
    /// </summary>
    /// <param name="direction">プレイヤーの向き（X方向）。Idle状態でも向きを維持するために使用。</param>
    private IEnumerator TransitionToIdleAfterLanding(float direction)
    {
        // 着地演出の猶予時間。これにより地面に降りた直後にすぐIdleに入らず、自然な演出になる
        yield return new WaitForSeconds(LandingToIdleDelay);

        // もし外部からIdle遷移がキャンセルされていた場合（例：ワイヤーで再ジャンプ）
        if (_cancelIdleTransition)
        {
            Debug.Log("[TransitionToIdleAfterLanding] Idle遷移がキャンセルされました");

            _cancelIdleTransition = false; // フラグは1回の遷移キャンセルのみ有効とする

            // 一度キャンセルしたあと、さらに一定時間待ってから再度Idle遷移を試みる。
            // これにより「一瞬の割り込み（ワイヤー接続）後に再度落下してLanding中」のケースをカバー
            yield return new WaitForSeconds(LandingToIdleDelay);

            // 現在の状態がまだLandingであれば、再度Idleに遷移する
            if (_currentState == PlayerState.Landing)
            {
                SetPlayerState(PlayerState.Idle, direction);
            }

            // Idle遷移を試みたのでコルーチン終了
            yield break;
        }

        // Idle遷移前にワイヤー遷移フラグが立っている場合、
        // ワイヤー接続により他の状態へ移行予定とみなし、Idle遷移はスキップする
        if (_pendingWireTransition || _justGrappled)
        {
            Debug.Log("[TransitionToIdleAfterLanding] Skipped Idle due to pending Wire transition.");
            yield break;
        }

        // 通常通りIdleへ遷移させる（この部分がまだ未実装なので必要に応じて追加する）
        if (_currentState == PlayerState.Landing)
        {
            SetPlayerState(PlayerState.Idle, direction);
        }
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
