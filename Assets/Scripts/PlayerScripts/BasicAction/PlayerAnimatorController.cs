using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    #region Animator本体
    private Animator _animator;
    #endregion

    #region Animatorパラメータ名
    private static class AnimatorParams
    {
        public const string State = "State";
        public const string SpeedMultiplier = "speedMultiplier";
    }
    #endregion

    #region PlayerState
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
    #endregion

    #region 優先度
    private enum PlayerStatePriority { Low, Medium, High }

    private PlayerStatePriority GetStatePriority(PlayerState state)
    {
        return state switch
        {
            PlayerState.Damage or PlayerState.Goal => PlayerStatePriority.High,
            PlayerState.Wire or PlayerState.Jump or PlayerState.MeleeAttack or PlayerState.RangedAttack => PlayerStatePriority.Medium,
            _ => PlayerStatePriority.Low
        };
    }

    private bool CanTransitionTo(PlayerState newState, bool force = false)
    {
        if (force) return true;

        var currentPriority = GetStatePriority(_currentState);
        var newPriority = GetStatePriority(newState);

        if (currentPriority == PlayerStatePriority.High && newPriority < currentPriority)
            return false;

        // 🔽 攻撃中はIdleやLandingなどLow優先度のステートに戻さない
        if (_isAttacking && (newState == PlayerState.Idle || newState == PlayerState.Landing))
            return false;

        if (_pendingWireTransition && (newState == PlayerState.Landing || newState == PlayerState.Idle))
            return false;

        return true;
    }

    public bool CanAttackNow()
    {
        // 攻撃中なら攻撃禁止
        if (_isAttacking) return false;

        return true;
    }

    public bool CanAcceptAttackInput()
    {
        return !_isAttacking && !_attackInputLocked;
    }
    #endregion

    #region アニメーション速度
    private static class AnimatorSpeeds
    {
        public const float RunMin = 0.5f;
        public const float RunMax = 3.0f;
        public const float Swing = 1.5f;
        public const float Grapple = 1.5f;
        public const float Landing = 1.0f;
        public const float MeleeAttack = 1.0f;
        public const float RangedAttack = 1.0f;
        public const float Damage = 1.0f;
        public const float Goal = 1.0f;
        public const float Idle = 1.0f;
    }
    #endregion

    #region フラグ類
    private PlayerState _currentState = PlayerState.Idle;
    // 現在の状態を外部から取得可能にする
    public PlayerState CurrentState => _currentState;
    private bool _isAttacking = false;
    public bool IsAttacking => _isAttacking;

    private bool _attackInputLocked = false; // 攻撃入力をブロックするフラグ

    public bool IsDamagePlaying { get; private set; }
    private bool _pendingWireTransition = false;
    private bool _justGrappled = false;
    private float _grappleTimer = 0f;
    private float _wireDirection = 0f;
    private bool _cancelIdleTransition = false;
    private bool _isMoving = false;
    private float _moveStopTimer = 0f;
    private const float MoveThreshold = 0.05f;
    private const float MoveDelayTime = 0.1f;
    private const float GrappleTransitionTime = 0.3f;
    private const float FlipThreshold = 0.01f;
    private const float LandingToIdleDelay = 0.5f;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private AudioClip landingSE;
    [SerializeField] private AudioClip[] footstepSEs;
    private int lastFootstepIndex = -1;
    private float lastFootstepTime = 0f;
    private const float FootstepInterval = 0.25f; // 足音間隔(秒)
    #endregion

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_justGrappled)
        {
            _grappleTimer -= Time.deltaTime;
            if (_grappleTimer <= 0f)
            {
                _justGrappled = false;
                if (_pendingWireTransition)
                {
                    SetPlayerState(PlayerState.Wire, _wireDirection, 0f, true);
                    _pendingWireTransition = false;
                }
            }
        }
    }

    #region SetPlayerState
    public void SetPlayerState(PlayerState newState, float direction = 0f, float speed = 0f, bool force = false)
    {
        if (_animator == null) return;
        if (!force && _currentState == newState) return;

        // 攻撃中なら再度攻撃ステートに遷移させない
        if (_isAttacking && (newState == PlayerState.MeleeAttack || newState == PlayerState.RangedAttack))
        {
            Debug.Log("[SetPlayerState] 攻撃中の再遷移をスキップ");
            return;
        }


        if (!CanTransitionTo(newState, force)) return;

        var oldState = _currentState;
        _currentState = newState;
        Debug.Log($"[SetPlayerState] {oldState} -> {newState}");

        _isAttacking = newState == PlayerState.MeleeAttack || newState == PlayerState.RangedAttack;

        _animator.SetInteger(AnimatorParams.State, (int)newState);
        FlipSprite(direction);

        float speedMultiplier = newState switch
        {
            PlayerState.Run => Mathf.Lerp(AnimatorSpeeds.RunMin, AnimatorSpeeds.RunMax, Mathf.Clamp01(Mathf.Abs(speed))),
            PlayerState.Jump => AnimatorSpeeds.Grapple,
            PlayerState.Wire => AnimatorSpeeds.Swing,
            PlayerState.Landing => AnimatorSpeeds.Landing,
            PlayerState.MeleeAttack => AnimatorSpeeds.MeleeAttack,
            PlayerState.RangedAttack => AnimatorSpeeds.RangedAttack,
            PlayerState.Damage => AnimatorSpeeds.Damage,
            PlayerState.Goal => AnimatorSpeeds.Goal,
            _ => AnimatorSpeeds.Idle
        };
        _animator.SetFloat(AnimatorParams.SpeedMultiplier, speedMultiplier);

        // SE再生
        if (newState == PlayerState.Landing) PlayLandingSE();
        if (newState == PlayerState.Run) PlayFootstepSE();

        if (newState == PlayerState.Damage) StartCoroutine(ResetFromDamage(1f));
        if (newState == PlayerState.Landing) StartCoroutine(TransitionToIdleAfterLanding(direction));
    }
    #endregion

    #region Move / Flip
    public void UpdateMoveAnimation(float moveInput)
    {
        // Wire や Landing 中は移動アニメーション無効
        if (_pendingWireTransition || _isAttacking || IsDamagePlaying || _currentState == PlayerState.Wire || _currentState == PlayerState.Landing)
            return;

        if (Mathf.Abs(moveInput) > MoveThreshold)
        {
            _isMoving = true;
            _moveStopTimer = 0f;
        }
        else
        {
            _moveStopTimer += Time.deltaTime;
            if (_moveStopTimer > MoveDelayTime) _isMoving = false;
        }

        FlipSprite(moveInput);

        if (_isMoving)
        {
            SetPlayerState(PlayerState.Run, moveInput, Mathf.Abs(moveInput));
            PlayFootstepSE();
        }
        else if (_currentState != PlayerState.Landing && _currentState != PlayerState.Wire)
            SetPlayerState(PlayerState.Idle, moveInput, 0f);
    }

    private void FlipSprite(float moveInput)
    {
        if (Mathf.Abs(moveInput) < FlipThreshold) return;
        var scale = transform.localScale;
        scale.x = Mathf.Sign(moveInput) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
    #endregion

    #region Wire
    public void PlayGrappleSwingAnimation(float swingDirection)
    {
        _justGrappled = true;
        _grappleTimer = GrappleTransitionTime;
        StopAllCoroutines();
        SetPlayerState(PlayerState.Jump, swingDirection);
        _pendingWireTransition = true;
        _wireDirection = swingDirection;
    }

    public void StopSwingAnimation(float swingDirection)
    {
        _pendingWireTransition = false;
        _justGrappled = false;

        // Landing中なら無理に再遷移させず、コルーチンに任せる
        if (_currentState != PlayerState.Landing)
        {
            SetPlayerState(PlayerState.Landing, swingDirection, 0f, true);
        }
    }

    #endregion

    #region Attacks
    public void PlayMeleeAttackAnimation(float direction)
    {
        if (_attackInputLocked) return; // すでに攻撃入力ロック中なら無視
        _attackInputLocked = true;      // 入力ブロック
        SetPlayerState(PlayerState.MeleeAttack, direction, 0f);
    }
    public void OnMeleeAttackAnimationEnd()
    {
        _isAttacking = false;
        _attackInputLocked = false;     // アニメ終了で解除
        SetPlayerState(PlayerState.Idle, 0f, 0f, true);
    }

    public void PlayRangedAttackAnimation(float direction)
    {
        if (_attackInputLocked) return;
        _attackInputLocked = true;
        SetPlayerState(PlayerState.RangedAttack, direction, 0f);
    }

    public void OnRangedAttackAnimationEnd()
    {
        _isAttacking = false;
        _attackInputLocked = false;     // アニメ終了で解除
        SetPlayerState(PlayerState.Idle, 0f, 0f, true);
    }
    #endregion

    #region Damage / Goal
    public void PlayDamageAnimation(float direction)
    {
        IsDamagePlaying = true;
        SetPlayerState(PlayerState.Damage, direction);
    }
    public void OnDamageAnimationEnd()
    {
        IsDamagePlaying = false;
        if (_pendingWireTransition) SetPlayerState(PlayerState.Wire, _wireDirection, 0f, true);
        else SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? 1f : -1f, 0f, true);
    }

    public void PlayGoalAnimation(float direction) => SetPlayerState(PlayerState.Goal, direction);
    #endregion

    #region Coroutines
    private IEnumerator ResetFromDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsDamagePlaying = false;
        if (_currentState == PlayerState.Damage)
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? 1f : -1f, 0f, true);
    }

    public IEnumerator TransitionToIdleAfterLanding(float direction)
    {
        yield return new WaitForSeconds(LandingToIdleDelay);

        // 攻撃中ならIdleに戻さない
        if (_isAttacking)
        {
            Debug.Log("[TransitionToIdleAfterLanding] 攻撃中のためIdle遷移をスキップ");
            yield break;
        }

        if (_pendingWireTransition)
        {
            SetPlayerState(PlayerState.Wire, _wireDirection, 0f, true);
            yield break;
        }

        if (_cancelIdleTransition)
        {
            _cancelIdleTransition = false;
            yield return new WaitForSeconds(LandingToIdleDelay);
            if (_currentState == PlayerState.Landing)
                SetPlayerState(PlayerState.Idle, direction);
            yield break;
        }

        if (_currentState == PlayerState.Landing)
        {
            _pendingWireTransition = false; // スイング切断後は無視
            SetPlayerState(PlayerState.Idle, direction);
        }
    }

    #endregion

    public void OnWireCut(float swingDirection)
    {
        // すでにLanding中なら、ワイヤー切断を無視する
        if (_currentState == PlayerState.Landing)
        {
            Debug.Log("[OnWireCut] Landing中のためワイヤー切断処理を無視");
            return;
        }

        // 内部フラグをリセット（Wireからの遷移時のみ実行）
        ResetWireFlags();

        // Wire中なら Landing へ遷移
        if (_currentState == PlayerState.Wire)
        {
            Debug.Log("[OnWireCut] Wire中にワイヤー切断 → Landingへ");
            StopAllCoroutines();
            SetPlayerState(PlayerState.Landing, swingDirection, 0f, true);

            // Idleへの移行を確実に行うコルーチンを開始
            StartCoroutine(ForceTransitionToIdle(swingDirection));
        }
        else
        {
            Debug.Log($"[OnWireCut] {_currentState}中にワイヤー切断 → 無視");
        }
    }


    public void ForceLanding(float direction)
    {
        // すでにLanding中なら再実行しない（Idleに戻すための余計な再呼び出しを防ぐ）
        if (_currentState == PlayerState.Landing)
        {
            Debug.Log("[ForceLanding] すでにLanding状態なので再遷移をスキップ");
            return;
        }

        ResetWireFlags();
        _isMoving = false;
        _moveStopTimer = 0f;

        // 現在のLanding遷移コルーチンを停止
        StopAllCoroutines();

        // Landing に強制遷移
        SetPlayerState(PlayerState.Landing, direction, 0f, true);

        // Idle遷移がキャンセルされないように確実にIdleへ移行
        StartCoroutine(ForceTransitionToIdle(direction));
    }

    private IEnumerator ForceTransitionToIdle(float direction)
    {
        yield return new WaitForSeconds(LandingToIdleDelay);

        // 攻撃中ならIdle強制遷移をスキップ
        if (_isAttacking)
        {
            Debug.Log("[ForceTransitionToIdle] 攻撃中のためIdle強制遷移をスキップ");
            yield break;
        }

        if (_currentState == PlayerState.Landing || _currentState == PlayerState.Jump)
        {
            Debug.Log("[ForceTransitionToIdle] Landing → Idle 強制遷移(保険)");
            _cancelIdleTransition = false;
            _pendingWireTransition = false;
            SetPlayerState(PlayerState.Idle, direction, 0f, true);
        }
    }

    public void ForceIdle(float directionX = 0f)
    {
        StopAllCoroutines();
        _cancelIdleTransition = false;
        _pendingWireTransition = false;
        _justGrappled = false;
        SetPlayerState(PlayerState.Idle, directionX, 0f, true);
    }


    /// <summary>
    /// ワイヤーやジャンプ時にプレイヤーの移動アニメーションをリセット
    /// </summary>
    public void ResetMoveAnimation()
    {
        // 移動フラグをリセット
        _isMoving = false;
        _moveStopTimer = 0f;

        // 現在の状態がRunならIdleに切り替える
        if (_currentState == PlayerState.Run)
        {
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? 1f : -1f);
        }
    }

    // PlayerAnimatorController に追加
    public void ResetWireFlags()
    {
        _pendingWireTransition = false;
        _justGrappled = false;
        _cancelIdleTransition = false;
    }

    public bool IsPlayingLanding()
    {
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        return info.IsName("Landing") && info.normalizedTime < 1f;
    }


    /// <summary>
    /// ジャンプやワイヤー移動時に移動方向の情報を更新
    /// </summary>
    public void UpdateJumpState(float directionX)
    {
        // スプライトの反転やアニメーション状態の更新
        FlipSprite(directionX);

        // 必要であればジャンプステートに切り替え
        SetPlayerState(PlayerState.Jump, directionX);
    }

    public void CancelPendingIdleTransition() => _cancelIdleTransition = true;

    public void ThrowBombEvent()
    {
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        playerAttack.ThrowBomb(direction);
    }

    #region SE
    public void PlayFootstepSE()
    {
        if (footstepSEs == null || footstepSEs.Length == 0) return;
        if (Time.time - lastFootstepTime < FootstepInterval) return; // 間隔制限

        int index;
        do
        {
            index = Random.Range(0, footstepSEs.Length);
        } while (index == lastFootstepIndex && footstepSEs.Length > 1);

        lastFootstepIndex = index;
        lastFootstepTime = Time.time;

        AudioManager.Instance?.PlaySE(footstepSEs[index]);
    }

    public void PlayLandingSE()
    {
        AudioManager.Instance?.PlaySE(landingSE);
    }
    #endregion
}
