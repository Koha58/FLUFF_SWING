using System.Collections;
using UnityEngine;

/// <summary>
/// プレイヤーのアニメーション全体を制御するクラス。
/// Animatorパラメータ(State / SpeedMultiplier)を管理し、
/// 移動・攻撃・ワイヤー・ダメージ・着地などの状態を統合的に制御する。
/// プレイヤーの**見た目と音**の制御を担当する。
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    #region === Animator 本体 ===

    /// <summary>
    /// プレイヤーの Animator コンポーネント本体
    /// </summary>
    private Animator _animator;

    #endregion


    #region === Animator パラメータ定義 ===

    /// <summary>
    /// Animator Controller 内で使用するパラメータ名。
    /// 文字列リテラルの直書きを防ぎ、安全に参照するための定数群。
    /// </summary>
    private static class AnimatorParams
    {
        public const string State = "State";                 // 現在のアニメーション状態（Int）
        public const string SpeedMultiplier = "speedMultiplier"; // 再生速度倍率（Float）
    }

    #endregion


    #region === アニメーション関連定数 ===

    /// <summary>
    /// 各ステートごとのアニメーション再生速度倍率。
    /// </summary>
    private static class AnimatorSpeeds
    {
        public const float RunMin = 0.5f;        // 最低移動時の Run 速度
        public const float RunMax = 3.0f;        // 最大移動時の Run 速度
        public const float Swing = 1.5f;         // ワイヤースイング
        public const float Grapple = 1.5f;       // グラップル開始（Jump）
        public const float Landing = 1.0f;       // 着地
        public const float MeleeAttack = 1.0f;   // 近接攻撃
        public const float RangedAttack = 1.0f;  // 遠距離攻撃
        public const float Damage = 1.0f;        // ダメージ
        public const float Goal = 1.0f;          // ゴール
        public const float Idle = 1.0f;          // 待機
    }

    /// <summary>
    /// アニメーション遷移や演出タイミングに関する定数。
    /// </summary>
    private static class Timings
    {
        public const float MoveThreshold = 0.05f;        // 移動と判定する最小入力
        public const float MoveDelayTime = 0.1f;         // 入力停止 → Idle までの猶予
        public const float GrappleTransitionTime = 0.3f; // Jump → Wire までの遅延
        public const float FlipThreshold = 0.01f;        // 反転を行う入力閾値
        public const float LandingToIdleDelay = 0.5f;    // Landing → Idle 遷移時間
        public const float FootstepInterval = 0.25f;     // 足音の最小間隔
        public const float DamageResetDelay = 1.0f;      // ダメージ復帰時間
        public const float AttackTimeout = 1.2f;         // 攻撃強制終了時間
    }

    /// <summary>
    /// 向き関連の定数。
    /// </summary>
    private static class Directions
    {
        public const float None = 0f;
        public const float Left = -1f;
        public const float Right = 1f;
    }

    /// <summary>
    /// 引数用の汎用スピード定数。
    /// </summary>
    private static class Speeds
    {
        public const float None = 0f;
    }

    /// <summary>
    /// 初期値定義。
    /// </summary>
    private static class Defaults
    {
        public const int LastFootstepIndexDefault = -1;
        public const float TimeZero = 0f;
    }

    #endregion


    #region === プレイヤーアニメーションステート ===

    /// <summary>
    /// プレイヤーのアニメーション状態。
    /// Animator の State(Int) と同期する。
    /// </summary>
    public enum PlayerState
    {
        Idle = 0,
        Run = 1,
        Jump = 2,          // ジャンプ / 落下 / グラップル初動
        Wire = 3,          // ワイヤースイング
        Landing = 4,       // 着地
        MeleeAttack = 5,   // 近接攻撃
        RangedAttack = 6,  // 遠距離攻撃
        Damage = 7,        // ダメージ
        Goal = 8           // ゴール
    }

    /// <summary>
    /// ステート遷移の優先度。
    /// 高いステートは低いステートを上書きできる。
    /// </summary>
    private enum PlayerStatePriority
    {
        Low,
        Medium,
        High
    }

    #endregion


    #region === Inspector 参照 ===

    [Header("参照")]
    [SerializeField] private PlayerAttack playerAttack; // 攻撃処理（Animation Event 用）
    [SerializeField] private AudioClip landingSE;        // 着地SE
    [SerializeField] private AudioClip[] footstepSEs;    // 足音SE候補

    #endregion


    #region === 実行時ステート管理 ===

    /// <summary>現在のアニメーション状態</summary>
    private PlayerState _currentState = PlayerState.Idle;
    public PlayerState CurrentState => _currentState;

    /// <summary>ダメージ前など、直前の状態保持用</summary>
    private PlayerState _previousState = PlayerState.Idle;

    /// <summary>攻撃アニメーション再生中か</summary>
    private bool _isAttacking = false;
    public bool IsAttacking => _isAttacking;

    /// <summary>攻撃入力ロック中か</summary>
    private bool _attackInputLocked = false;

    /// <summary>ダメージアニメーション再生中か</summary>
    public bool IsDamagePlaying { get; private set; }

    /// <summary>攻撃後に Wire へ遷移する待ち状態</summary>
    private bool _pendingWireTransition = false;

    /// <summary>グラップル直後フラグ（Jump → Wire 自動遷移用）</summary>
    private bool _justGrappled = false;

    /// <summary>移動入力が継続しているか</summary>
    private bool _isMoving = false;

    /// <summary>ゲームオーバー状態</summary>
    private bool _isGameOver = false;

    #endregion


    #region === タイマー・補助変数 ===

    private float _grappleTimer = Defaults.TimeZero;      // Jump → Wire タイマー
    private float _wireDirection = Directions.None;       // ワイヤー方向保持
    private float _moveStopTimer = Defaults.TimeZero;     // 移動停止検出用
    private int lastFootstepIndex = Defaults.LastFootstepIndexDefault;
    private float lastFootstepTime = Defaults.TimeZero;

    #endregion


    #region === SE 制御 ===

    private float _lastLandingSETime = -999f;
    private const float LandingSECooldown = 0.12f;

    /// <summary>
    /// 着地SEを次の Landing で 1 回だけ鳴らすための許可フラグ
    /// </summary>
    private bool _landingSEAllowed = false;

    #endregion


    #region === コルーチン参照 ===

    private Coroutine _attackTimeoutCoroutine;   // 攻撃タイムアウト
    private Coroutine _landingToIdleCoroutine;   // Landing → Idle 遷移

    #endregion


    #region === Unityイベント ===
    private void Awake()
    {
        // 必須コンポーネントであるAnimatorを取得
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_isGameOver) return;

        // グラップル開始直後の処理：JumpアニメーションからWireアニメーションへ自動遷移
        if (_justGrappled)
        {
            _grappleTimer -= Time.deltaTime;
            if (_grappleTimer <= Defaults.TimeZero)
            {
                // タイマー切れ
                _justGrappled = false;
                if (_pendingWireTransition)
                {
                    // 遷移待ちフラグが立っていればWireへ切り替え
                    SetPlayerState(PlayerState.Wire, _wireDirection, Speeds.None, true);
                    _pendingWireTransition = false;
                }
            }
        }
    }
    #endregion

    #region === ステート管理 ===
    /// <summary>
    /// ステートごとの優先度を返す。
    /// </summary>
    private PlayerStatePriority GetStatePriority(PlayerState state)
    {
        return state switch // C# 8.0以降のswitch式を使用し、簡潔に優先度を定義
        {
            PlayerState.Damage or PlayerState.Goal => PlayerStatePriority.High, // ダメージとゴールは最高優先度で他の状態を上書き
            PlayerState.Wire or PlayerState.Jump or PlayerState.MeleeAttack or PlayerState.RangedAttack => PlayerStatePriority.Medium, // 攻撃、ジャンプ、ワイヤーは中優先度
            _ => PlayerStatePriority.Low // その他（Idle, Run, Landing）は低優先度
        };
    }

    /// <summary>
    /// 指定された新しいステートへの遷移が可能かどうかを判定する。
    /// 優先度や現在の特殊状態（攻撃中、ワイヤー遷移待ち）を考慮する。
    /// </summary>
    private bool CanTransitionTo(PlayerState newState, bool force = false)
    {
        if (force) return true; // force=true の場合は強制的に遷移を許可

        var currentPriority = GetStatePriority(_currentState);
        var newPriority = GetStatePriority(newState);

        // 現在 Wire 状態であれば、Wire 状態以外への遷移を禁止
        if (_currentState == PlayerState.Wire && newState != PlayerState.Wire)
        {
            // ✅ 例外：高優先度（Damage/Goal）は Wire を上書きしてよい
            if (newState == PlayerState.Damage || newState == PlayerState.Goal)
            {
                return true;
            }

            return false;
        }

        // High優先度（Damage/Goal）中に、それ以下の優先度の遷移を無効化
        if (currentPriority == PlayerStatePriority.High && newPriority < currentPriority)
            return false;

        // 攻撃中は、IdleやLandingへの遷移を制限（攻撃アニメーション終了まで待つ）
        if (_isAttacking && (newState == PlayerState.Idle || newState == PlayerState.Landing))
            return false;

        // ワイヤー遷移待ち（_pendingWireTransition）中は、Landing/Idleへの遷移を禁止（ワイヤーへの遷移を優先）
        if (_pendingWireTransition && (newState == PlayerState.Landing || newState == PlayerState.Idle))
            return false;

        return true;
    }

    /// <summary>
    /// 攻撃アニメーションを開始できる状態か（現在攻撃中でないか）
    /// </summary>
    public bool CanAttackNow() => !_isAttacking;

    /// <summary>
    /// 攻撃入力を受け付けられる状態か（攻撃中でなく、入力ロックもかかっていないか）
    /// </summary>
    // CanAcceptAttackInput のデバッグ強化
    public bool CanAcceptAttackInput()
    {
        bool canAttack = !_isAttacking && !_attackInputLocked;

        // 攻撃がブロックされた時のみログ出力
        if (!canAttack)
        {
            Debug.LogWarning($"[Attack Blocked] IsAttacking: {_isAttacking}, InputLocked: {_attackInputLocked}, CurrentState: {_currentState}");
        }
        return canAttack;
    }
    #endregion

    #region === ステート遷移 ===
    /// <summary>
    /// プレイヤーの状態を変更し、Animatorのパラメータ（StateとSpeedMultiplier）を更新する。
    /// </summary>
    /// <param name="newState">新しいアニメーション状態</param>
    /// <param name="direction">スプライト反転に使用する方向（通常は移動入力）</param>
    /// <param name="speed">Runアニメーションの速度倍率計算に使用する入力速度</param>
    /// <param name="force">遷移可能判定を無視して強制的に遷移するか</param>
    public void SetPlayerState(PlayerState newState, float direction = Directions.None, float speed = Speeds.None, bool force = false)
    {
        if (_isGameOver) return;

        if (_animator == null) return;
        // 既に同じ状態への遷移を試みた場合は、force=trueでない限り無視
        if (!force && _currentState == newState) return;
        // 攻撃中に攻撃を連続で開始しようとした場合は無視
        if (_isAttacking && (newState == PlayerState.MeleeAttack || newState == PlayerState.RangedAttack)) return;
        // 遷移可能判定（優先度など）に引っかかった場合は無視
        if (!CanTransitionTo(newState, force)) return;

        var oldState = _currentState;

        // 攻撃終了後のステートへ移行する場合、入力ロックを解除する
        // 攻撃ステート (MeleeAttack, RangedAttack) 以外へ遷移する時
        if (oldState == PlayerState.MeleeAttack || oldState == PlayerState.RangedAttack)
        {
            if (newState != PlayerState.MeleeAttack && newState != PlayerState.RangedAttack)
            {
                // 攻撃ステートから離脱する際には、フラグを強制リセット
                if (_attackTimeoutCoroutine != null)
                {
                    StopCoroutine(_attackTimeoutCoroutine);
                    _attackTimeoutCoroutine = null;
                }
                _isAttacking = false;
                _attackInputLocked = false;
                Debug.Log("[SetPlayerState] Attack -> Non-Attack: Input Lock Forced Reset.");
            }
        }

        _currentState = newState; // 状態の更新

        // 攻撃中フラグの更新
        _isAttacking = newState == PlayerState.MeleeAttack || newState == PlayerState.RangedAttack;

        // Animatorへステート（Int）を反映
        _animator.SetInteger(AnimatorParams.State, (int)newState);

        // スプライト反転
        FlipSprite(direction);

        // スピード倍率設定（Stateごとの固定値、またはRunのように変数）
        float speedMultiplier = newState switch
        {
            // Run状態は入力速度に応じてMin～Max間で線形補間
            PlayerState.Run => Mathf.Lerp(AnimatorSpeeds.RunMin, AnimatorSpeeds.RunMax, Mathf.Clamp01(Mathf.Abs(speed))),
            PlayerState.Jump => AnimatorSpeeds.Grapple,
            PlayerState.Wire => AnimatorSpeeds.Swing,
            PlayerState.Landing => AnimatorSpeeds.Landing,
            PlayerState.MeleeAttack => AnimatorSpeeds.MeleeAttack,
            PlayerState.RangedAttack => AnimatorSpeeds.RangedAttack,
            PlayerState.Damage => AnimatorSpeeds.Damage,
            PlayerState.Goal => AnimatorSpeeds.Goal,
            _ => AnimatorSpeeds.Idle // その他の状態（Idleなど）
        };
        _animator.SetFloat(AnimatorParams.SpeedMultiplier, speedMultiplier);

        // 状態ごとの追加処理（コルーチン開始、SE再生など）
        if (newState == PlayerState.Run) PlayFootstepSE(); // 初回Runアニメーション開始時の足音（連続再生はUpdateMoveAnimationで制御）
        if (newState == PlayerState.Damage) StartCoroutine(ResetFromDamage(Timings.DamageResetDelay)); // ダメージ後の自動復帰
        if (newState == PlayerState.Landing) StartCoroutine(TransitionToIdleAfterLanding(direction)); // 着地後の自動Idle遷移
    }
    #endregion

    #region === 移動アニメーション制御 ===
    /// <summary>
    /// 移動入力に応じてRun/Idleを切り替え、スプライトの左右反転を行う。
    /// プレイヤーの移動処理から毎フレーム呼ばれることを想定。
    /// </summary>
    /// <param name="moveInput">移動入力値（-1.0～1.0）</param>
    public void UpdateMoveAnimation(float moveInput)
    {
        // ワイヤー、攻撃、ダメージ、着地中は常にリターン。Jump中は移動入力によるRun/Idleへの遷移はしない。
        if (_pendingWireTransition || _isAttacking || IsDamagePlaying ||
            _currentState == PlayerState.Wire || _currentState == PlayerState.Landing)
            return;

        // 移動判定ロジック
        if (Mathf.Abs(moveInput) > Timings.MoveThreshold)
        {
            // 移動入力あり
            _isMoving = true;
            _moveStopTimer = Defaults.TimeZero; // 停止タイマーをリセット
        }
        else
        {
            // 移動入力なし
            _moveStopTimer += Time.deltaTime;
            if (_moveStopTimer > Timings.MoveDelayTime) _isMoving = false; // 一定時間入力がなければ「停止中」と判定
        }

        // スプライト反転処理
        FlipSprite(moveInput);

        // ステート更新
        if (_isMoving)
        {
            // 移動中
            SetPlayerState(PlayerState.Run, moveInput, Mathf.Abs(moveInput));
            PlayFootstepSE(); // 足音再生を試みる（Timings.FootstepIntervalで制御される）
        }
        // 停止中かつ現在状態がLanding/Wire以外であればIdleへ（Jumpは別途処理が必要な場合があるため除外）
        else if (_currentState != PlayerState.Landing && _currentState != PlayerState.Wire && _currentState != PlayerState.Jump)
            SetPlayerState(PlayerState.Idle, moveInput, Speeds.None);
    }

    /// <summary>
    /// スプライト（TransformのlocalScale.x）の左右反転を行う。
    /// </summary>
    /// <param name="moveInput">反転方向決定に使用する入力値（正:右, 負:左）</param>
    private void FlipSprite(float moveInput)
    {
        if (Mathf.Abs(moveInput) < Timings.FlipThreshold) return; // 閾値以下の入力は無視

        var scale = transform.localScale;
        // 入力方向に応じてlocalScale.xの符号を決定（絶対値は維持）
        scale.x = Mathf.Sign(moveInput) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    /// <summary>
    /// 外部から移動停止を強制し、Run状態であればIdleへ戻す。
    /// </summary>
    public void ResetMoveAnimation()
    {
        _isMoving = false;
        _moveStopTimer = Defaults.TimeZero;
        if (_currentState == PlayerState.Run)
            // 現在の向きを維持してIdleへ遷移
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? Directions.Right : Directions.Left);
    }
    #endregion

    #region === ワイヤー関連アニメーション ===
    /// <summary>
    /// グラップル発射後にジャンプ（Jumpアニメ）→ワイヤー（Wireアニメ）へ移行する流れを開始する。
    /// </summary>
    /// <param name="swingDirection">スイング方向</param>
    public void PlayGrappleSwingAnimation(float swingDirection)
    {
        if (_isGameOver) return;

        _wireDirection = swingDirection;

        // すでにワイヤー中なら無視
        if (_currentState == PlayerState.Wire) return;

        // 攻撃中なら、攻撃アニメーション終了後にワイヤーへ移行するようフラグをセットし、現在の攻撃アニメーションの終了を待つ。
        if (_isAttacking)
        {
            Debug.Log("[PlayGrappleSwingAnimation] Attack in progress, deferring wire transition.");
            _pendingWireTransition = true;
            _justGrappled = false; // タイマーでの強制移行は行わない
            return;
        }

        // 攻撃中でなければ通常の Jump → Wire 遷移処理
        Debug.Log("[PlayGrappleSwingAnimation] Normal wire transition started.");
        _justGrappled = true; // タイマーでの自動遷移を有効化
        _pendingWireTransition = true;
        _grappleTimer = Timings.GrappleTransitionTime; // 自動遷移タイマーを設定

        // まずJump状態へ遷移（グラップル発射アニメーションを兼ねる）
        SetPlayerState(PlayerState.Jump, swingDirection, Speeds.None, true);
    }

    /// <summary>
    /// スイングを停止し着地アニメーションへ移行。
    /// </summary>
    /// <param name="swingDirection">最後の移動方向</param>
    public void StopSwingAnimation(float swingDirection)
    {
        _pendingWireTransition = false;
        _justGrappled = false;
        if (_currentState != PlayerState.Landing)
            // 強制的にLanding状態へ遷移
            SetPlayerState(PlayerState.Landing, swingDirection, Speeds.None, true);
    }

    /// <summary>
    /// ワイヤー切断時（プレイヤーがワイヤーを離した/ワイヤーが切れた）の挙動。
    /// </summary>
    /// <param name="swingDirection">切断時の移動方向</param>
    /// <param name="isPlayerGrounded">切断時にプレイヤーが地面に接触していたか</param>
    public void OnWireCut(float swingDirection, bool isPlayerGrounded)
    {
        if (_isGameOver) return;

        // Wire関連のフラグをリセット
        ResetWireFlags();

        // ワイヤー切断時は、攻撃中の状態を強制的に解除し、ロックを外す
        if (_isAttacking || _attackInputLocked)
        {
            Debug.Log("[OnWireCut] Forcing attack flags reset due to wire cut.");
            _isAttacking = false;
            _attackInputLocked = false;
            if (_attackTimeoutCoroutine != null)
            {
                StopCoroutine(_attackTimeoutCoroutine);
                _attackTimeoutCoroutine = null;
            }
        }

        // Landing後のIdle遷移コルーチンがもし残っていれば確実に停止
        if (_landingToIdleCoroutine != null)
        {
            StopCoroutine(_landingToIdleCoroutine);
            _landingToIdleCoroutine = null;
        }

        // 既にDamageステートなどの最高優先度ステートであれば何もしない
        if (GetStatePriority(_currentState) == PlayerStatePriority.High) return;

        // --- 遷移処理の開始 ---

        // isPlayerGrounded にかかわらず、まずは必ず Landing へ遷移させる
        PlayerState targetState = PlayerState.Landing;
        float speedMultiplier = AnimatorSpeeds.Landing;

        // 現在の状態がターゲットと異なる、または Wire / Jump 状態からの離脱であれば強制実行
        // Jump(グラップル予備動作)状態でカットされた場合も、遷移設定を無視して即Landingへ移すために条件に含める
        if (_currentState != targetState || _currentState == PlayerState.Wire || _currentState == PlayerState.Jump)
        {
            Debug.Log($"[OnWireCut] Forcing transition: {_currentState} -> {targetState} (Forced Play)");

            _previousState = _currentState; // 履歴を更新
            _currentState = targetState;

            // 1. パラメータ更新
            _animator.SetInteger(AnimatorParams.State, (int)targetState);
            _animator.SetFloat(AnimatorParams.SpeedMultiplier, speedMultiplier);

            // 2.【重要】Play()を使って強制的にステートを再生する
            // これにより、Jump -> Landing への矢印(Transition)がなくても、
            // またJumpアニメーションの途中であっても即座に切り替わる。
            // ※Animatorウィンドウ内のステート名が "Landing" である必要がある。
            _animator.Play("Landing", 0, 0f);

            // 3. アニメーターを即座に評価
            _animator.Update(0f);

            // 4. スプライト反転
            FlipSprite(swingDirection);

            // 5. Landing後の処理（Idleへの自動遷移コルーチン開始）
            _landingToIdleCoroutine = StartCoroutine(TransitionToIdleAfterLanding(swingDirection));
        }
        // 念のためのフェイルセーフ（Play()を使えば基本的には不要だが、念の為残す）
        else if (_currentState == PlayerState.Wire)
        {
            Debug.LogWarning("[OnWireCut] State transition logic fallback triggered. Forcing Landing.");
            _animator.SetInteger(AnimatorParams.State, (int)PlayerState.Landing);
            _currentState = PlayerState.Landing;

            _animator.Play("Landing", 0, 0f); // ここでも強制再生
            _animator.Update(0f);

            if (_landingToIdleCoroutine != null) StopCoroutine(_landingToIdleCoroutine);
            _landingToIdleCoroutine = StartCoroutine(TransitionToIdleAfterLanding(swingDirection));
            ResetWireFlags();
        }

        // 攻撃フラグの状態チェック
        Debug.Log($"[OnWireCut End] Attack Flags: IsAttacking={_isAttacking}, InputLocked={_attackInputLocked}");
    }

    /// <summary>
    /// ワイヤー関係の内部フラグをリセット。
    /// </summary>
    public void ResetWireFlags()
    {
        _pendingWireTransition = false;
        _justGrappled = false;
    }
    #endregion

    #region === 攻撃アニメーション ===
    /// <summary>
    /// 近接攻撃アニメーションを開始。
    /// </summary>
    /// <param name="direction">攻撃方向（スプライト反転用）</param>
    public void PlayMeleeAttackAnimation(float direction)
    {
        if (_attackInputLocked) return; // 入力ロック中は受け付けない
        _attackInputLocked = true; // 入力ロック開始
        SetPlayerState(PlayerState.MeleeAttack, direction, Speeds.None);
        // タイムアウトコルーチンの開始と参照の保持 ▼ ---
        if (_attackTimeoutCoroutine != null) StopCoroutine(_attackTimeoutCoroutine);
        _attackTimeoutCoroutine = StartCoroutine(AttackTimeout());
    }

    /// <summary>
    /// 近接攻撃アニメーション終了時にアニメーターイベントから呼ばれる。
    /// </summary>
    public void OnMeleeAttackAnimationEnd()
    {
        // タイムアウトコルーチンの確実な停止
        if (_attackTimeoutCoroutine != null)
        {
            StopCoroutine(_attackTimeoutCoroutine);
            _attackTimeoutCoroutine = null;
        }

        // AttackTimeoutコルーチンをここで停止させたい場合は、AttackTimeoutコルーチンの参照を保持するか、StopCoroutine(nameof(AttackTimeout))を行う必要があります。
        // 今回はAttackTimeoutコルーチンが自己終了する仕様なので、フラグのみリセット。
        _isAttacking = false;
        _attackInputLocked = false;

        // ワイヤー遷移待ちがあれば、DelayedWireTransitionで処理
        if (_pendingWireTransition)
        {
            Debug.Log("[OnMeleeAttackAnimationEnd] Wire transition deferred: Attack -> Idle -> Jump -> Wire");
            // DelayedWireTransitionコルーチンを開始し、後続の遷移を処理
            StartCoroutine(DelayedWireTransition(_wireDirection));
        }
        else
        {
            // 待機状態へ戻す (強制遷移でRunから上書き可能にする)
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? Directions.Right : Directions.Left, Speeds.None, true);
        }
    }


    /// <summary>
    /// 遠距離攻撃アニメーションを開始。
    /// </summary>
    /// <param name="direction">攻撃方向（スプライト反転用）</param>
    public void PlayRangedAttackAnimation(float direction)
    {
        if (_attackInputLocked) return; // 入力ロック中は受け付けない
        _attackInputLocked = true; // 入力ロック開始
        SetPlayerState(PlayerState.RangedAttack, direction, Speeds.None);
        // タイムアウトコルーチンの開始と参照の保持
        if (_attackTimeoutCoroutine != null) StopCoroutine(_attackTimeoutCoroutine);
        _attackTimeoutCoroutine = StartCoroutine(AttackTimeout());
    }

    /// <summary>
    /// 遠距離攻撃アニメーション終了時にアニメーターイベントから呼ばれる。
    /// </summary>
    public void OnRangedAttackAnimationEnd()
    {
        // タイムアウトコルーチンの確実な停止
        if (_attackTimeoutCoroutine != null)
        {
            StopCoroutine(_attackTimeoutCoroutine);
            _attackTimeoutCoroutine = null;

        }
        _isAttacking = false;
        _attackInputLocked = false;

        if (_pendingWireTransition)
        {
            Debug.Log("[OnRangedAttackAnimationEnd] Wire transition executed after ranged attack.");
            _pendingWireTransition = false;
            // 遠距離攻撃は硬直が短いため、Idleを経由せずにJumpへ直接遷移しても問題ないと判断される場合がある。
            SetPlayerState(PlayerState.Jump, _wireDirection, Speeds.None, true);
            StartCoroutine(TransitionToWireAfterJump()); // JumpからWireへの自動遷移コルーチン開始
        }
        else
        {
            // 待機状態へ戻す
            SetPlayerState(PlayerState.Idle, Directions.None, Speeds.None, true);
        }
    }

    /// <summary>
    /// 近接攻撃終了後、Idleを経由してJump→Wireへ遷移するための遅延コルーチン。
    /// </summary>
    private IEnumerator DelayedWireTransition(float swingDirection)
    {
        // 攻撃終了処理（OnMeleeAttackAnimationEnd）で_pendingWireTransitionがfalseになっていないことを保証
        _pendingWireTransition = true;

        // Attack → Idle にまず戻る（自然な遷移のため）
        SetPlayerState(PlayerState.Idle, Directions.None, Speeds.None, true);

        // 1フレーム待ってから Jump → Wire に移行
        yield return null;

        // Jump状態へ遷移（グラップル開始アニメ）
        SetPlayerState(PlayerState.Jump, swingDirection, Speeds.None, true);
        // JumpからWireへの自動遷移コルーチンを開始
        StartCoroutine(TransitionToWireAfterJump());
    }


    /// <summary>
    /// Jump状態から一定時間後にWire状態へ強制遷移させるコルーチン。
    /// </summary>
    private IEnumerator TransitionToWireAfterJump()
    {
        yield return new WaitForSeconds(Timings.GrappleTransitionTime);

        // タイマー経過時点でまだJump状態であれば、Wireへ遷移
        if (_currentState == PlayerState.Jump)
        {
            Debug.Log("[TransitionToWireAfterJump] Forcing Wire transition after Jump.");
            SetPlayerState(PlayerState.Wire, _wireDirection, Speeds.None, true);
        }

        // 遷移処理完了（または不要）
        _pendingWireTransition = false;
        _justGrappled = false;
    }


    /// <summary>
    /// 現在、近接/遠距離攻撃アニメーションが再生中かどうか。
    /// </summary>
    public bool IsInAttackState() =>
        CurrentState == PlayerState.MeleeAttack || CurrentState == PlayerState.RangedAttack;
    #endregion

    #region === ダメージ・ゴール関連 ===

    /// <summary>
    /// ダメージアニメーション再生を開始する。
    /// </summary>
    /// <param name="direction">被ダメージ時のノックバック方向（スプライト反転用）</param>
    public void PlayDamageAnimation(float direction)
    {
        _previousState = _currentState; // ←元のステートを保存

        // 攻撃中にダメージを受けた場合、攻撃を強制終了
        if (_isAttacking)
        {
            _isAttacking = false;
            _attackInputLocked = false;
            // 攻撃タイムアウトコルーチンを確実に停止
            if (_attackTimeoutCoroutine != null)
            {
                StopCoroutine(_attackTimeoutCoroutine);
                _attackTimeoutCoroutine = null;
            }
            StopAllCoroutines(); // 攻撃タイムアウトやIdle遷移コルーチンなどを全て停止
        }

        IsDamagePlaying = true; // ダメージ中フラグON
        // ダメージ状態へ遷移（SetPlayerState内でResetFromDamageコルーチンが開始される）
        SetPlayerState(PlayerState.Damage, direction, Speeds.None);
    }

    /// <summary>
    /// ダメージアニメーション終了時にアニメーターイベントから呼ばれる。
    /// </summary>
    public void OnDamageAnimationEnd()
    {
        if (_isGameOver) return;

        IsDamagePlaying = false;
        _isAttacking = false;
        _attackInputLocked = false;

        // まず、ワイヤー遷移待ちがある場合はWireへ
        if (_pendingWireTransition)
        {
            SetPlayerState(PlayerState.Wire, _wireDirection, Speeds.None, true);
            _pendingWireTransition = false;
            _justGrappled = false;
            return;
        }

        // ワイヤー切断やLanding済みの場合はIdleに戻す
        if (_currentState != PlayerState.Wire && !IsPlayingLanding())
        {
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? Directions.Right : Directions.Left, Speeds.None, true);
        }
        // Wire中かつまだワイヤーが接続されていればWireに復帰
        else if (_currentState == PlayerState.Wire)
        {
            SetPlayerState(PlayerState.Wire, _wireDirection, Speeds.None, true);
        }
        else if (IsPlayingLanding())
        {
            // Landing中ならLandingステートを維持したままIdle遷移コルーチンに任せる
            StartCoroutine(TransitionToIdleAfterLanding(transform.localScale.x > 0 ? Directions.Right : Directions.Left));
        }
    }


    /// <summary>
    /// ゴールアニメーション再生を開始する。
    /// </summary>
    /// <param name="direction">ゴール時の向き</param>
    public void PlayGoalAnimation(float direction) =>
        SetPlayerState(PlayerState.Goal, direction, Speeds.None);



    /// <summary>
    /// ゲームオーバー時の見た目・アニメーション状態を強制的にリセットする。
    /// GameManager からのみ呼ばれることを想定。
    /// </summary>
    public void OnGameOverVisual(float directionX, bool forceIdle = true)
    {
        _isGameOver = true;

        StopAllCoroutines();

        ResetWireFlags();
        IsDamagePlaying = false;
        _isAttacking = false;
        _attackInputLocked = false;

        // 見た目をLandingに固定（SetPlayerStateは使わない）
        _currentState = PlayerState.Landing;
        _animator.SetInteger("State", (int)PlayerState.Landing);
        _animator.SetFloat("speedMultiplier", 1.0f);
        _animator.Update(0f);

        FlipSprite(directionX);

        Debug.Log($"[GameOverVisual] state={_currentState} pending={_pendingWireTransition} just={_justGrappled}");

    }

    #endregion


    #region === コルーチン ===
    /// <summary>
    /// ダメージアニメーションが終了した後、指定時間経過で自動的にIdleへ戻す。
    /// アニメーションイベントが呼ばれなかった場合の保険も兼ねる。
    /// </summary>
    private IEnumerator ResetFromDamage(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_isGameOver) yield break;

        IsDamagePlaying = false;
        // 現在のステートに応じて遷移先を決定
        if (_currentState == PlayerState.Damage)
        {
            if (_previousState == PlayerState.Wire || _pendingWireTransition)
            {
                // ワイヤー中ならWireに戻す
                SetPlayerState(PlayerState.Wire, _wireDirection, Speeds.None, true);
            }
            else
            {
                // それ以外はIdleへ
                SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? Directions.Right : Directions.Left, Speeds.None, true);
            }
        }
    }

    /// <summary>
    /// 着地後、一定時間後にIdleへ自動遷移するコルーチン。
    /// </summary>
    public IEnumerator TransitionToIdleAfterLanding(float direction)
    {
        // コルーチンの参照を保持
        _landingToIdleCoroutine = StartCoroutine(TransitionToIdleAfterLandingInternal(direction));
        yield return _landingToIdleCoroutine;
    }

    private IEnumerator TransitionToIdleAfterLandingInternal(float direction)
    {
       // yield return new WaitForSeconds(Timings.LandingToIdleDelay); // 0.5秒待機

        if (_isAttacking) { _landingToIdleCoroutine = null; yield break; }

        if (_currentState == PlayerState.Landing)
        {
            // Landing アニメーション終了後、ここで Idle に遷移する
            SetPlayerState(PlayerState.Idle, direction);
        }
        _landingToIdleCoroutine = null;
    }

    /// <summary>
    /// 攻撃アニメーション中に一定時間経過した場合、強制的に攻撃を終了しIdleへ戻すコルーチン。
    /// アニメーションイベントが呼ばれなかった場合のエラー回避。
    /// </summary>
    private IEnumerator AttackTimeout()
    {
        yield return new WaitForSeconds(Timings.AttackTimeout);
        if (_isAttacking)
        {
            // タイムアウト発生
            _isAttacking = false;
            _attackInputLocked = false;
            Debug.LogWarning("Attack animation timed out. Forcing Idle transition.");
            // 攻撃中にコルーチンが停止しなかった場合、Idleへ強制遷移
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? Directions.Right : Directions.Left, Speeds.None, true);
        }

        // 終了時に参照をクリア
        _attackTimeoutCoroutine = null;
    }

    #endregion

    #region === サウンド関連 ===
    /// <summary>
    /// 足音をランダムに再生。足音の間隔（Timings.FootstepInterval）で制御される。
    /// </summary>
    public void PlayFootstepSE()
    {
        if (footstepSEs == null || footstepSEs.Length == 0) return;
        // 前回再生から一定時間が経過していなければ再生しない
        if (Time.time - lastFootstepTime < Timings.FootstepInterval) return;

        int index;
        // 前回と同じSEが連続で再生されないようにランダムにインデックスを選択（SEが2種類以上の場合）
        do { index = Random.Range(0, footstepSEs.Length); }
        while (index == lastFootstepIndex && footstepSEs.Length > 1);

        lastFootstepIndex = index;
        lastFootstepTime = Time.time;
        // AudioManagerクラスのインスタンスを通じてSEを再生
        AudioManager.Instance?.PlaySE(footstepSEs[index]);
    }

    /// <summary>
    /// 次回の Landing アニメーション中に、
    /// 着地SEを「1回だけ」再生することを許可する。
    ///
    /// PlayerMove 側で「空中 → 接地の瞬間」を検知したときに呼ばれる。
    /// 実際の SE 再生は Animation Event（PlayLandingSE）に委ねる。
    /// </summary>
    public void AllowLandingSEOnce()
    {
        // 着地SEの再生許可フラグを立てる
        _landingSEAllowed = true;
    }

    /// <summary>
    /// 着地アニメーション内の Animation Event から呼ばれる。
    /// 許可されている場合のみ、着地SEを再生する。
    ///
    /// ・空中で Landing に入った場合
    /// ・Landing が多重に再生された場合
    /// ・ワイヤー切断などで強制遷移した場合
    /// でも SE が鳴らないようにするための安全装置。
    /// </summary>
    public void PlayLandingSE()
    {
        // 許可フラグが立っていなければ再生しない
        // （＝ 正しい「着地瞬間」ではない）
        if (!_landingSEAllowed) return;

        // 1回再生したら即消費（多重再生防止）
        _landingSEAllowed = false;

        // クールダウン時間内であれば再生しない
        // （床の微振動・段差・MovingFloor対策）
        if (Time.time - _lastLandingSETime < LandingSECooldown) return;

        // 最終再生時刻を更新
        _lastLandingSETime = Time.time;

        // 着地SEを再生
        AudioManager.Instance?.PlaySE(landingSE);
    }

    #endregion

    #region === 補助関数 ===
    /// <summary>
    /// 強制的にLandingアニメーションへ切り替え、その後Idleへ自動遷移させる。
    /// </summary>
    /// <param name="direction">着地時の向き</param>
    public void ForceLanding(float direction)
    {
        if (_currentState == PlayerState.Landing) return;

        // ワイヤー関連のフラグと移動フラグをリセット
        ResetWireFlags();
        _isMoving = false;
        _moveStopTimer = Defaults.TimeZero;

        // コルーチン参照による停止
        if (_landingToIdleCoroutine != null)
        {
            StopCoroutine(_landingToIdleCoroutine);
            _landingToIdleCoroutine = null;
        }

        // Landingへ強制遷移
        SetPlayerState(PlayerState.Landing, direction, Speeds.None, true);
    }

    /// <summary>
    /// 強制的にIdleへ戻し、関連するコルーチンとフラグをリセットする。
    /// </summary>
    /// <param name="directionX">Idle時の向き（任意）</param>
    public void ForceIdle(float directionX = Directions.None)
    {
        StopAllCoroutines(); // 全コルーチン停止

        // 関連フラグをリセット
        _pendingWireTransition = false;
        _justGrappled = false;

        // Idleへ強制遷移
        SetPlayerState(PlayerState.Idle, directionX, Speeds.None, true);
    }

    /// <summary>
    /// Jump中のアニメーション更新（主にスプライト反転を更新）。
    /// </summary>
    /// <param name="directionX">移動入力方向</param>
    public void UpdateJumpState(float directionX)
    {
        FlipSprite(directionX);
        // 既にJump状態ならSetPlayerState内のチェックで無視されるが、スプライト反転は行われる。
        SetPlayerState(PlayerState.Jump, directionX, Speeds.None);
    }

    /// <summary>
    /// 現在Landingアニメーションが再生中かどうかをAnimatorStateInfoから判定。
    /// </summary>
    /// <returns>Landingアニメーション再生中（ただし終了前）であればtrue</returns>
    public bool IsPlayingLanding()
    {
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        // "Landing"という名前のアニメーションが再生中で、かつ再生時間（normalizedTime）が1.0未満（終了前）
        return info.IsName("Landing") && info.normalizedTime < 1f;
    }

    /// <summary>
    /// 遠距離攻撃アニメーションの特定のフレームでAnimatorイベントから呼ばれ、爆弾を投擲する。
    /// </summary>
    public void ThrowBombEvent()
    {
        // 現在のスプライトの向きを取得
        float dir = transform.localScale.x > 0 ? Directions.Right : Directions.Left;
        // PlayerAttackスクリプトの爆弾投擲メソッドを呼び出す
        playerAttack.ThrowBomb(dir);
    }
    #endregion
}