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
    #region === Animator本体 ===
    private Animator _animator; // プレイヤーのAnimatorコンポーネント本体
    #endregion

    #region === Animatorパラメータ名 ===
    /// <summary>
    /// Animatorコントローラ内で使用するパラメータ名を定数化。
    /// 文字列リテラル誤りを防ぎ、コードの可読性を向上させる。
    /// </summary>
    private static class AnimatorParams
    {
        public const string State = "State"; // プレイヤーの現在の状態(Int)
        public const string SpeedMultiplier = "speedMultiplier"; // アニメーション再生速度の倍率(Float)
    }
    #endregion

    #region === 定数定義 ===
    /// <summary>
    /// 各アニメーションの再生スピード倍率を定義。
    /// アニメーションの「テンポ」を調整するために使用される。
    /// </summary>
    private static class AnimatorSpeeds
    {
        public const float RunMin = 0.5f;   // 最小移動速度でのRunアニメーション倍率
        public const float RunMax = 3.0f;   // 最大移動速度でのRunアニメーション倍率
        public const float Swing = 1.5f;    // ワイヤースイング時のアニメーション倍率
        public const float Grapple = 1.5f;  // グラップル開始時のアニメーション倍率（Jumpと併用）
        public const float Landing = 1.0f;  // 着地アニメーション倍率
        public const float MeleeAttack = 1.0f; // 近接攻撃アニメーション倍率
        public const float RangedAttack = 1.0f; // 遠距離攻撃アニメーション倍率
        public const float Damage = 1.0f;   // ダメージアニメーション倍率
        public const float Goal = 1.0f;     // ゴールアニメーション倍率
        public const float Idle = 1.0f;     // 待機アニメーション倍率
    }

    /// <summary>
    /// アニメーション遷移・入力検出・演出タイミングに関する定数。
    /// </summary>
    private static class Timings
    {
        public const float MoveThreshold = 0.05f;           // 移動入力がこれ以上（絶対値）で「移動中」と判定する閾値
        public const float MoveDelayTime = 0.1f;            // 入力が途切れてから「停止中(Idle)」と判定するまでの遅延時間
        public const float GrappleTransitionTime = 0.3f;    // グラップル（Jumpアニメ）からワイヤー（Wireアニメ）へ切り替わるまでの時間
        public const float FlipThreshold = 0.01f;           // 左右反転を行う入力の最小閾値
        public const float LandingToIdleDelay = 0.5f;       // 着地後、自動的にIdleに戻るまでの時間
        public const float FootstepInterval = 0.25f;        // 足音を再生する最小間隔
        public const float DamageResetDelay = 1.0f;         // ダメージアニメーション終了後、強制的にIdleへ戻すまでの時間
        public const float AttackTimeout = 1.2f;            // 攻撃アニメーションが正常終了しなかった場合に強制終了する時間
    }

    /// <summary>
    /// 方向値を定義（左右・なし）。スプライト反転や攻撃方向決定に使用。
    /// </summary>
    private static class Directions
    {
        public const float None = 0f;
        public const float Left = -1f;
        public const float Right = 1f;
    }

    /// <summary>
    /// 汎用スピード定数（主に引数のデフォルト値として使用）。
    /// </summary>
    private static class Speeds
    {
        public const float None = 0f;
    }

    /// <summary>
    /// デフォルト初期値。
    /// </summary>
    private static class Defaults
    {
        public const int LastFootstepIndexDefault = -1; // 足音のインデックス初期値
        public const float TimeZero = 0f; // 時間の初期値（0秒）
    }
    #endregion

    #region === プレイヤーステート ===
    /// <summary>
    /// プレイヤーのアニメーション状態。
    /// AnimatorのIntパラメータ「State」と同期し、アニメーション切り替えを行う。
    /// </summary>
    public enum PlayerState
    {
        Idle = 0, // 待機
        Run = 1, // 走行
        Jump = 2, // ジャンプ・落下・グラップル初期
        Wire = 3, // ワイヤースイング中
        Landing = 4, // 着地
        MeleeAttack = 5, // 近接攻撃
        RangedAttack = 6, // 遠距離攻撃
        Damage = 7, // ダメージ
        Goal = 8 // ゴール
    }

    /// <summary>
    /// ステートの優先度。
    /// 優先度が高いステート（例: ダメージ、ゴール）は、低いステートからの遷移を上書きできる。
    /// </summary>
    private enum PlayerStatePriority { Low, Medium, High }
    #endregion

    #region === 変数・フラグ類 ===
    [Header("参照")]
    [SerializeField] private PlayerAttack playerAttack; // 攻撃処理スクリプトへの参照（アニメーションイベント用）
    [SerializeField] private AudioClip landingSE;        // 着地音SEのクリップ
    [SerializeField] private AudioClip[] footstepSEs;    // 足音SEのクリップ候補（ランダム再生用）

    private PlayerState _currentState = PlayerState.Idle;    // 現在のアニメーション状態
    public PlayerState CurrentState => _currentState; // 外部公開用プロパティ

    private PlayerState _previousState = PlayerState.Idle; // ダメージ前のステートを保持

    private bool _isAttacking = false;              // 攻撃アニメーション再生中フラグ（攻撃判定の持続などにも使用）
    public bool IsAttacking => _isAttacking; // 外部公開用プロパティ

    private bool _attackInputLocked = false;        // 攻撃入力の一時ロックフラグ（連打防止やアニメーション終了までの待ちに使用）
    public bool IsDamagePlaying { get; private set; } // ダメージアニメーション再生中フラグ（外部から読み取り可能）

    private bool _pendingWireTransition = false;    // 攻撃などの後にワイヤー状態に遷移するのを待っているフラグ
    private bool _justGrappled = false;             // グラップル（ジャンプアニメ）開始直後フラグ（タイマーでの自動ワイヤー遷移制御用）
    private bool _isMoving = false;                 // 移動入力が継続しているかどうかのフラグ

    private float _grappleTimer = Defaults.TimeZero; // グラップル（Jump）からワイヤー（Wire）へ自動遷移するためのタイマー
    private float _wireDirection = Directions.None;  // ワイヤー方向（スプライト反転保持用）
    private float _moveStopTimer = Defaults.TimeZero;// 移動入力停止を検出するためのタイマー
    private int lastFootstepIndex = Defaults.LastFootstepIndexDefault; // 前回再生した足音SEのインデックス
    private float lastFootstepTime = Defaults.TimeZero;              // 前回足音を再生したUnity時間

    private Coroutine _attackTimeoutCoroutine; // 攻撃タイムアウトコルーチンの参照を保持
    private Coroutine _landingToIdleCoroutine; // Landing後のIdle遷移コルーチンの参照を保持
    #endregion

    #region === Unityイベント ===
    private void Awake()
    {
        // 必須コンポーネントであるAnimatorを取得
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
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
    public bool CanAcceptAttackInput() => !_isAttacking && !_attackInputLocked;
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
        if (_animator == null) return;
        // 既に同じ状態への遷移を試みた場合は、force=trueでない限り無視
        if (!force && _currentState == newState) return;
        // 攻撃中に攻撃を連続で開始しようとした場合は無視
        if (_isAttacking && (newState == PlayerState.MeleeAttack || newState == PlayerState.RangedAttack)) return;
        // 遷移可能判定（優先度など）に引っかかった場合は無視
        if (!CanTransitionTo(newState, force)) return;

        var oldState = _currentState;
        _currentState = newState; // 状態の更新
        Debug.Log($"[SetPlayerState] {oldState} -> {newState}");

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
        if (newState == PlayerState.Landing) PlayLandingSE();
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
        // 特定状態中は移動アニメーションの更新を無効化
        if (_pendingWireTransition || _isAttacking || IsDamagePlaying ||
            _currentState == PlayerState.Wire || _currentState == PlayerState.Landing || _currentState == PlayerState.Jump)
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
    public void OnWireCut(float swingDirection)
    {
        // Wire関連のフラグをリセット
        ResetWireFlags();

        // Wire状態以外、またはすでにLanding中であれば何もしない
        // ただし、Landing状態への遷移は強制で行うため、_currentState == PlayerState.Wire ではない場合でも
        // 強制的に遷移させるロジックに変更する
        if (_currentState == PlayerState.Landing) return;

        // Landing後のIdle遷移コルーチンがもし残っていれば確実に停止
        // StopCoroutine(nameof(...)) の代わりに、参照変数 (_landingToIdleCoroutine) を利用して確実に停止
        if (_landingToIdleCoroutine != null)
        {
            StopCoroutine(_landingToIdleCoroutine);
            _landingToIdleCoroutine = null;
        }

        // Landing状態へ強制遷移
        // SetPlayerState内でLandingSE再生、およびTransitionToIdleAfterLandingコルーチンが開始される
        SetPlayerState(PlayerState.Landing, swingDirection, Speeds.None, true);
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
    #endregion

    #region === コルーチン ===
    /// <summary>
    /// ダメージアニメーションが終了した後、指定時間経過で自動的にIdleへ戻す。
    /// アニメーションイベントが呼ばれなかった場合の保険も兼ねる。
    /// </summary>
    private IEnumerator ResetFromDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
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
        yield return new WaitForSeconds(Timings.LandingToIdleDelay);

        if (_isAttacking) { _landingToIdleCoroutine = null; yield break; } // 着地直後に攻撃が開始された場合は中断
                                                                           // ... (以降、ロジックは変更なし) ...

        if (_currentState == PlayerState.Landing)
        {
            // ... (遷移処理) ...
            SetPlayerState(PlayerState.Idle, direction);
        }
        _landingToIdleCoroutine = null; // 終了時に参照をクリア
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
    /// 着地音を再生。
    /// </summary>
    public void PlayLandingSE() =>
        AudioManager.Instance?.PlaySE(landingSE);
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