using System.Collections;
using UnityEngine;

/// <summary>
/// プレイヤーのアニメーション全体を制御するクラス。
/// Animatorパラメータ(State / SpeedMultiplier)を管理し、
/// 移動・攻撃・ワイヤー・ダメージ・着地などの状態を統合的に制御する。
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    #region === Animator本体 ===
    private Animator _animator; // プレイヤーのAnimatorコンポーネント
    #endregion

    #region === Animatorパラメータ名 ===
    /// <summary>
    /// Animatorコントローラ内で使用するパラメータ名を定数化
    /// </summary>
    private static class AnimatorParams
    {
        public const string State = "State";
        public const string SpeedMultiplier = "speedMultiplier";
    }
    #endregion

    #region === 定数定義 ===
    /// <summary>
    /// 各アニメーションの再生スピード倍率
    /// </summary>
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

    /// <summary>
    /// アニメーション遷移・入力検出・演出タイミングに関する定数
    /// </summary>
    private static class Timings
    {
        public const float MoveThreshold = 0.05f;          // 移動入力がこれ以上で「移動中」と判定
        public const float MoveDelayTime = 0.1f;           // 移動停止判定までの遅延
        public const float GrappleTransitionTime = 0.3f;   // グラップル→ワイヤー切替までの遅延
        public const float FlipThreshold = 0.01f;          // 左右反転を行う入力の閾値
        public const float LandingToIdleDelay = 0.5f;      // 着地後Idleに戻るまでの時間
        public const float FootstepInterval = 0.25f;       // 足音再生の間隔
        public const float DamageResetDelay = 1.0f;        // ダメージ終了後Idleへ戻すまでの時間
        public const float AttackTimeout = 1.2f;           // 攻撃が終わらなかった場合に強制終了する時間
    }

    /// <summary>
    /// 方向値を定義（左右・なし）
    /// </summary>
    private static class Directions
    {
        public const float None = 0f;
        public const float Left = -1f;
        public const float Right = 1f;
    }

    /// <summary>
    /// 汎用スピード定数（今後の拡張用）
    /// </summary>
    private static class Speeds
    {
        public const float None = 0f;
    }

    /// <summary>
    /// デフォルト初期値
    /// </summary>
    private static class Defaults
    {
        public const int LastFootstepIndexDefault = -1;
        public const float TimeZero = 0f;
    }
    #endregion

    #region === プレイヤーステート ===
    /// <summary>
    /// プレイヤーのアニメーション状態
    /// AnimatorのIntパラメータ「State」と同期
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
    /// ステートの優先度
    /// ダメージ・ゴールなどは他ステートを上書き
    /// </summary>
    private enum PlayerStatePriority { Low, Medium, High }
    #endregion

    #region === 変数・フラグ類 ===
    [Header("参照")]
    [SerializeField] private PlayerAttack playerAttack; // 攻撃処理スクリプト参照
    [SerializeField] private AudioClip landingSE;       // 着地音
    [SerializeField] private AudioClip[] footstepSEs;   // 足音候補

    private PlayerState _currentState = PlayerState.Idle;   // 現在の状態
    public PlayerState CurrentState => _currentState;

    private bool _isAttacking = false;               // 攻撃中フラグ
    public bool IsAttacking => _isAttacking;

    private bool _attackInputLocked = false;         // 攻撃入力を一時ロックする
    public bool IsDamagePlaying { get; private set; } // ダメージ中フラグ

    private bool _pendingWireTransition = false;     // ワイヤー遷移待ちフラグ
    private bool _justGrappled = false;              // グラップル直後フラグ
    private bool _cancelIdleTransition = false;      // Idle遷移キャンセルフラグ
    private bool _isMoving = false;                  // 移動中フラグ

    private float _grappleTimer = Defaults.TimeZero; // グラップル遷移用タイマー
    private float _wireDirection = Directions.None;  // ワイヤー方向
    private float _moveStopTimer = Defaults.TimeZero;// 停止検出タイマー
    private int lastFootstepIndex = Defaults.LastFootstepIndexDefault; // 前回の足音インデックス
    private float lastFootstepTime = Defaults.TimeZero;                // 前回足音再生時刻

    private bool _attackInput_locked; // 使用されていない旧フラグ（将来的に削除可）
    #endregion

    #region === Unityイベント ===
    private void Awake()
    {
        // Animator取得
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // グラップル直後は一定時間後にワイヤーへ自動遷移
        if (_justGrappled)
        {
            _grappleTimer -= Time.deltaTime;
            if (_grappleTimer <= Defaults.TimeZero)
            {
                _justGrappled = false;
                if (_pendingWireTransition)
                {
                    SetPlayerState(PlayerState.Wire, _wireDirection, Speeds.None, true);
                    _pendingWireTransition = false;
                }
            }
        }
    }
    #endregion

    #region === ステート管理 ===
    /// <summary>
    /// ステートごとの優先度を返す
    /// </summary>
    private PlayerStatePriority GetStatePriority(PlayerState state)
    {
        return state switch
        {
            PlayerState.Damage or PlayerState.Goal => PlayerStatePriority.High,
            PlayerState.Wire or PlayerState.Jump or PlayerState.MeleeAttack or PlayerState.RangedAttack => PlayerStatePriority.Medium,
            _ => PlayerStatePriority.Low
        };
    }

    /// <summary>
    /// 遷移可能かどうか判定（優先度や特殊状態を考慮）
    /// </summary>
    private bool CanTransitionTo(PlayerState newState, bool force = false)
    {
        if (force) return true;

        var currentPriority = GetStatePriority(_currentState);
        var newPriority = GetStatePriority(newState);

        // High優先度中はそれ以下の遷移を無効化
        if (currentPriority == PlayerStatePriority.High && newPriority < currentPriority)
            return false;

        // 攻撃中はIdle/Landingへの遷移を制限
        if (_isAttacking && (newState == PlayerState.Idle || newState == PlayerState.Landing))
            return false;

        // ワイヤー遷移中は特定ステートを禁止
        if (_pendingWireTransition && (newState == PlayerState.Landing || newState == PlayerState.Idle))
            return false;

        return true;
    }

    /// <summary>
    /// 攻撃可能かどうか
    /// </summary>
    public bool CanAttackNow() => !_isAttacking;

    /// <summary>
    /// 攻撃入力を受け付けられるか
    /// </summary>
    public bool CanAcceptAttackInput() => !_isAttacking && !_attackInputLocked;
    #endregion

    #region === ステート遷移 ===
    /// <summary>
    /// プレイヤーの状態を変更し、アニメーションや速度倍率を反映
    /// </summary>
    public void SetPlayerState(PlayerState newState, float direction = Directions.None, float speed = Speeds.None, bool force = false)
    {
        if (_animator == null) return;
        if (!force && _currentState == newState) return;
        if (_isAttacking && (newState == PlayerState.MeleeAttack || newState == PlayerState.RangedAttack)) return;
        if (!CanTransitionTo(newState, force)) return;

        var oldState = _currentState;
        _currentState = newState;
        Debug.Log($"[SetPlayerState] {oldState} -> {newState}");

        _isAttacking = newState == PlayerState.MeleeAttack || newState == PlayerState.RangedAttack;

        // Animatorへステート反映
        _animator.SetInteger(AnimatorParams.State, (int)newState);

        // スプライト反転
        FlipSprite(direction);

        // スピード倍率設定（動作ごとに個別調整）
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

        // 状態ごとの追加処理
        if (newState == PlayerState.Landing) PlayLandingSE();
        if (newState == PlayerState.Run) PlayFootstepSE();
        if (newState == PlayerState.Damage) StartCoroutine(ResetFromDamage(Timings.DamageResetDelay));
        if (newState == PlayerState.Landing) StartCoroutine(TransitionToIdleAfterLanding(direction));
    }
    #endregion

    #region === 移動アニメーション制御 ===
    /// <summary>
    /// 移動入力に応じてRun/Idleを切り替える
    /// </summary>
    public void UpdateMoveAnimation(float moveInput)
    {
        // 特定状態中は移動アニメーションを無効化
        if (_pendingWireTransition || _isAttacking || IsDamagePlaying ||
            _currentState == PlayerState.Wire || _currentState == PlayerState.Landing)
            return;

        // 移動判定
        if (Mathf.Abs(moveInput) > Timings.MoveThreshold)
        {
            _isMoving = true;
            _moveStopTimer = Defaults.TimeZero;
        }
        else
        {
            _moveStopTimer += Time.deltaTime;
            if (_moveStopTimer > Timings.MoveDelayTime) _isMoving = false;
        }

        // スプライト反転処理
        FlipSprite(moveInput);

        // ステート更新
        if (_isMoving)
        {
            SetPlayerState(PlayerState.Run, moveInput, Mathf.Abs(moveInput));
            PlayFootstepSE();
        }
        else if (_currentState != PlayerState.Landing && _currentState != PlayerState.Wire)
            SetPlayerState(PlayerState.Idle, moveInput, Speeds.None);
    }

    /// <summary>
    /// スプライトの左右反転
    /// </summary>
    private void FlipSprite(float moveInput)
    {
        if (Mathf.Abs(moveInput) < Timings.FlipThreshold) return;
        var scale = transform.localScale;
        scale.x = Mathf.Sign(moveInput) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    /// <summary>
    /// 移動停止時にIdleへ戻す
    /// </summary>
    public void ResetMoveAnimation()
    {
        _isMoving = false;
        _moveStopTimer = Defaults.TimeZero;
        if (_currentState == PlayerState.Run)
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? Directions.Right : Directions.Left);
    }
    #endregion

    #region === ワイヤー関連アニメーション ===
    /// <summary>
    /// グラップル発射後にジャンプ→ワイヤーへ移行する流れを再現
    /// </summary>
    public void PlayGrappleSwingAnimation(float swingDirection)
    {
        _justGrappled = true;
        _grappleTimer = Timings.GrappleTransitionTime;
        StopAllCoroutines();
        SetPlayerState(PlayerState.Jump, swingDirection);
        _pendingWireTransition = true;
        _wireDirection = swingDirection;
    }

    /// <summary>
    /// スイングを停止し着地アニメーションへ移行
    /// </summary>
    public void StopSwingAnimation(float swingDirection)
    {
        _pendingWireTransition = false;
        _justGrappled = false;
        if (_currentState != PlayerState.Landing)
            SetPlayerState(PlayerState.Landing, swingDirection, Speeds.None, true);
    }

    /// <summary>
    /// ワイヤー切断時の挙動
    /// </summary>
    public void OnWireCut(float swingDirection)
    {
        if (_currentState == PlayerState.Landing) return;

        ResetWireFlags();
        if (_currentState == PlayerState.Wire)
        {
            StopAllCoroutines();
            SetPlayerState(PlayerState.Landing, swingDirection, Speeds.None, true);
            StartCoroutine(ForceTransitionToIdle(swingDirection));
        }
    }

    /// <summary>
    /// ワイヤー関係の内部フラグをリセット
    /// </summary>
    public void ResetWireFlags()
    {
        _pendingWireTransition = false;
        _justGrappled = false;
        _cancelIdleTransition = false;
    }
    #endregion

    #region === 攻撃アニメーション ===
    /// <summary>
    /// 近接攻撃開始
    /// </summary>
    public void PlayMeleeAttackAnimation(float direction)
    {
        if (_attackInputLocked) return;
        _attackInputLocked = true;
        SetPlayerState(PlayerState.MeleeAttack, direction, Speeds.None);
        StartCoroutine(AttackTimeout());
    }

    /// <summary>
    /// 近接攻撃アニメーション終了時に呼ばれる
    /// </summary>
    public void OnMeleeAttackAnimationEnd()
    {
        _isAttacking = false;
        _attackInputLocked = false;
        SetPlayerState(PlayerState.Idle, Directions.None, Speeds.None, true);
    }

    /// <summary>
    /// 遠距離攻撃開始
    /// </summary>
    public void PlayRangedAttackAnimation(float direction)
    {
        if (_attackInputLocked) return;
        _attackInputLocked = true;
        SetPlayerState(PlayerState.RangedAttack, direction, Speeds.None);
    }

    /// <summary>
    /// 遠距離攻撃終了時に呼ばれる
    /// </summary>
    public void OnRangedAttackAnimationEnd()
    {
        _isAttacking = false;
        _attackInputLocked = false;
        SetPlayerState(PlayerState.Idle, Directions.None, Speeds.None, true);
        StartCoroutine(AttackTimeout());
    }

    /// <summary>
    /// 現在攻撃中かどうか
    /// </summary>
    public bool IsInAttackState() =>
        CurrentState == PlayerState.MeleeAttack || CurrentState == PlayerState.RangedAttack;
    #endregion

    #region === ダメージ・ゴール関連 ===
    /// <summary>
    /// ダメージアニメーション再生
    /// </summary>
    public void PlayDamageAnimation(float direction)
    {
        if (_isAttacking || _attackInput_locked)
        {
            _isAttacking = false;
            _attackInputLocked = false;
            StopAllCoroutines();
        }

        IsDamagePlaying = true;
        SetPlayerState(PlayerState.Damage, direction, Speeds.None);
    }

    /// <summary>
    /// ダメージアニメーション終了時に呼ばれる
    /// </summary>
    public void OnDamageAnimationEnd()
    {
        IsDamagePlaying = false;
        _isAttacking = false;
        _attackInputLocked = false;

        if (_pendingWireTransition)
            SetPlayerState(PlayerState.Wire, _wireDirection, Speeds.None, true);
        else
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? Directions.Right : Directions.Left, Speeds.None, true);
    }

    /// <summary>
    /// ゴールアニメーション再生
    /// </summary>
    public void PlayGoalAnimation(float direction) =>
        SetPlayerState(PlayerState.Goal, direction, Speeds.None);
    #endregion

    #region === コルーチン ===
    /// <summary>
    /// ダメージ終了後に自動でIdleへ戻す
    /// </summary>
    private IEnumerator ResetFromDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsDamagePlaying = false;
        if (_currentState == PlayerState.Damage)
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? Directions.Right : Directions.Left, Speeds.None, true);
    }

    /// <summary>
    /// 着地後、一定時間後にIdleへ自動遷移
    /// </summary>
    public IEnumerator TransitionToIdleAfterLanding(float direction)
    {
        yield return new WaitForSeconds(Timings.LandingToIdleDelay);

        if (_isAttacking) yield break;

        if (_pendingWireTransition)
        {
            SetPlayerState(PlayerState.Wire, _wireDirection, Speeds.None, true);
            yield break;
        }

        if (_cancelIdleTransition)
        {
            _cancelIdleTransition = false;
            yield return new WaitForSeconds(Timings.LandingToIdleDelay);
        }

        if (_currentState == PlayerState.Landing)
        {
            _pendingWireTransition = false;
            SetPlayerState(PlayerState.Idle, direction);
        }
    }

    /// <summary>
    /// 攻撃中に一定時間経過で強制終了する
    /// </summary>
    private IEnumerator AttackTimeout()
    {
        yield return new WaitForSeconds(Timings.AttackTimeout);
        if (_isAttacking)
        {
            _isAttacking = false;
            _attackInputLocked = false;
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? Directions.Right : Directions.Left, Speeds.None, true);
        }
    }

    /// <summary>
    /// 一定時間後に強制的にIdleへ戻す（着地などで使用）
    /// </summary>
    private IEnumerator ForceTransitionToIdle(float direction)
    {
        yield return new WaitForSeconds(Timings.LandingToIdleDelay);
        if (_isAttacking) yield break;
        if (_currentState == PlayerState.Landing || _currentState == PlayerState.Jump)
        {
            _cancelIdleTransition = false;
            _pendingWireTransition = false;
            SetPlayerState(PlayerState.Idle, direction, Speeds.None, true);
        }
    }
    #endregion

    #region === サウンド関連 ===
    /// <summary>
    /// 足音をランダムに再生
    /// </summary>
    public void PlayFootstepSE()
    {
        if (footstepSEs == null || footstepSEs.Length == 0) return;
        if (Time.time - lastFootstepTime < Timings.FootstepInterval) return;

        int index;
        do { index = Random.Range(0, footstepSEs.Length); }
        while (index == lastFootstepIndex && footstepSEs.Length > 1);

        lastFootstepIndex = index;
        lastFootstepTime = Time.time;
        AudioManager.Instance?.PlaySE(footstepSEs[index]);
    }

    /// <summary>
    /// 着地音を再生
    /// </summary>
    public void PlayLandingSE() =>
        AudioManager.Instance?.PlaySE(landingSE);
    #endregion

    #region === 補助関数 ===
    /// <summary>
    /// 強制的にLandingアニメーションへ切り替え
    /// </summary>
    public void ForceLanding(float direction)
    {
        if (_currentState == PlayerState.Landing) return;
        ResetWireFlags();
        _isMoving = false;
        _moveStopTimer = Defaults.TimeZero;
        StopAllCoroutines();
        SetPlayerState(PlayerState.Landing, direction, Speeds.None, true);
        StartCoroutine(ForceTransitionToIdle(direction));
    }

    /// <summary>
    /// 強制的にIdleへ戻す（コルーチン全停止）
    /// </summary>
    public void ForceIdle(float directionX = Directions.None)
    {
        StopAllCoroutines();
        _cancelIdleTransition = false;
        _pendingWireTransition = false;
        _justGrappled = false;
        SetPlayerState(PlayerState.Idle, directionX, Speeds.None, true);
    }

    /// <summary>
    /// ジャンプ中のアニメーション更新
    /// </summary>
    public void UpdateJumpState(float directionX)
    {
        FlipSprite(directionX);
        SetPlayerState(PlayerState.Jump, directionX, Speeds.None);
    }

    /// <summary>
    /// 現在Landingアニメーションが再生中か
    /// </summary>
    public bool IsPlayingLanding()
    {
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        return info.IsName("Landing") && info.normalizedTime < 1f;
    }

    /// <summary>
    /// Idle遷移のキャンセル
    /// </summary>
    public void CancelPendingIdleTransition() => _cancelIdleTransition = true;

    /// <summary>
    /// 爆弾投擲イベント（アニメーションイベントから呼ばれる）
    /// </summary>
    public void ThrowBombEvent()
    {
        float dir = transform.localScale.x > 0 ? Directions.Right : Directions.Left;
        playerAttack.ThrowBomb(dir);
    }
    #endregion
}
