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

    // --------------------------------------------------------------
    // 【Animator本体（Animatorコンポーネントへの参照）】
    // プレイヤーの状態遷移を制御する中心コンポーネント
    // --------------------------------------------------------------
    private Animator _animator;

    #endregion


    #region アニメーターパラメータ名

    // --------------------------------------------------------------
    // 【Animatorパラメータ名の定数定義】
    // パラメータ名のtypo防止と一元管理
    // --------------------------------------------------------------
    private static class AnimatorParams
    {
        // プレイヤー状態の整数値
        public const string State = "State";

        // アニメーション再生速度
        public const string SpeedMultiplier = "speedMultiplier";
    }

    #endregion


    #region プレイヤーステート（Enum）

    // --------------------------------------------------------------
    // 【プレイヤーの状態を表すEnum】
    // AnimatorのStateパラメータと連動する値
    // --------------------------------------------------------------
    public enum PlayerState
    {
        Idle = 0,         // 待機
        Run = 1,          // 走り
        Jump = 2,         // ジャンプ
        Wire = 3,         // ワイヤー掴まり
        Landing = 4,      // 着地
        MeleeAttack = 5,  // 近距離攻撃
        RangedAttack = 6, // 遠距離攻撃
        Damage = 7,       // ダメージ
        Goal = 8          // クリア
    }

    #endregion


    #region プレイヤー状態制御

    // --------------------------------------------------------------
    // 【現在のプレイヤーステート】
    // Animatorに連動させて管理
    // --------------------------------------------------------------
    private PlayerState _currentState = PlayerState.Idle;

    // --------------------------------------------------------------
    // 【Animatorの状態遷移メソッド】
    // 状態に応じてパラメータや向き、速度を適用
    // --------------------------------------------------------------
    public void SetPlayerState(PlayerState newState, float direction = 0f, float speed = 0f, bool force = false)
    {
        // Awake()より先に呼ばれた場合に備え、Animatorの初期化を確認
        if (_animator == null)
        {
            Debug.LogWarning("Animator is not yet initialized. Skipping state transition.");
            return;
        }

        // 同じ状態でforce=falseなら何もしない
        if (!force && _currentState == newState) return;

        if (_currentState == PlayerState.Goal && newState != PlayerState.Goal)
        {
            // Goalアニメーションが終わったらIdleに遷移
            StartCoroutine(WaitForGoalAnimationToEnd());
            return;
        }

        // 被弾中は他状態を許さない
        if (!force && _currentState == PlayerState.Damage || !force && _currentState == PlayerState.Goal) return;

        var oldState = _currentState;
        _currentState = newState;
        Debug.Log($"[SetPlayerState] Transitioning from {oldState} to {newState}");

        // 攻撃フラグ更新
        _isAttacking = (newState == PlayerState.MeleeAttack || newState == PlayerState.RangedAttack);

        // Animatorパラメータ更新
        _animator.SetInteger(AnimatorParams.State, (int)newState);

        // 向き反映
        FlipSprite(direction);

        // --------------------------------------------------------------
        // 【新しいプレイヤーステートに応じて再生速度を設定】
        // 状態に合わせて SpeedMultiplier を調整する
        // --------------------------------------------------------------
        switch (newState)
        {
            case PlayerState.Run:
                // 【走り】入力速度に応じて最小〜最大速度を補間
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(speed));
                float moveSpeed = Mathf.Lerp(AnimatorSpeeds.RunMin, AnimatorSpeeds.RunMax, normalizedSpeed);
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, moveSpeed);
                break;

            case PlayerState.Jump:
                // 【ジャンプ】掴まり動作と共通の基準速度
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Grapple);
                break;

            case PlayerState.Wire:
                // 【ワイヤー】スイング中の基準速度
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Swing);
                break;

            case PlayerState.Landing:
                // 【着地】着地アニメーションの速度
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Landing);
                break;

            case PlayerState.MeleeAttack:
                // 【近距離攻撃】近接攻撃アニメーション速度
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.MeleeAttack);
                break;

            case PlayerState.RangedAttack:
                // 【遠距離攻撃】遠距離攻撃アニメーション速度
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.RangedAttack);
                break;

            case PlayerState.Damage:
                // 【被弾】ダメージリアクションの速度
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Damage);
                StartCoroutine(ResetFromDamage(1.0f)); // 例：1秒後に解除
                break;

            case PlayerState.Goal:
                // 【ゴール】クリア演出の速度
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Goal);
                break;

            default:
                // 【待機などその他】Idleの基準速度
                _animator.SetFloat(AnimatorParams.SpeedMultiplier, AnimatorSpeeds.Idle);
                break;
        }
    }

    #endregion


    #region アニメーション速度定義

    // --------------------------------------------------------------
    // 【Animator速度設定】
    // 各ステートごとのアニメーション再生速度の基準値
    // --------------------------------------------------------------
    private static class AnimatorSpeeds
    {
        public const float RunMin = 0.5f;    // 走り最小速度
        public const float RunMax = 3.0f;    // 走り最大速度
        public const float Swing = 1.5f;     // スイング中
        public const float Grapple = 1.5f;   // 掴まり
        public const float Landing = 1.0f;   // 着地
        public const float MeleeAttack = 1.0f; // 近距離攻撃
        public const float RangedAttack = 1.0f; // 遠距離攻撃
        public const float Damage = 1.0f;    // ダメージ
        public const float Goal = 1.0f;      // クリア
        public const float Idle = 1.0f;      // 待機
    }

    #endregion


    #region 各種定数

    // --------------------------------------------------------------
    // 【各種閾値・遅延時間】
    // プレイヤー挙動や演出に使用
    // --------------------------------------------------------------
    private const float MoveThreshold = 0.05f;        // 微小入力無視
    private const float MoveDelayTime = 0.1f;         // 移動停止判定
    private const float GrappleTransitionTime = 0.3f; // 掴まり演出時間
    private const float FlipThreshold = 0.01f;        // 向き反転閾値
    private const float LandingToIdleDelay = 0.2f;    // 着地→Idle遷移遅延

    #endregion


    #region プレイヤー挙動フラグ

    // --------------------------------------------------------------
    // 【移動状態管理】
    // 入力による移動/停止を判断
    // --------------------------------------------------------------
    private bool _isMoving = false;
    private float _moveStopTimer = 0f;

    // --------------------------------------------------------------
    // 【状態遷移制御フラグ】
    // 特定の状況での遷移抑制や保留
    // --------------------------------------------------------------
    private bool _cancelIdleTransition = false;
    private bool _pendingWireTransition = false;
    private float _wireDirection = 0f;

    // --------------------------------------------------------------
    // 【攻撃状態フラグ】
    // 攻撃中は他動作を制御
    // --------------------------------------------------------------
    private bool _isAttacking = false;

    // ダメージアニメ再生中かどうか
    public bool IsDamagePlaying { get; private set; }

    // --------------------------------------------------------------
    // 【攻撃コンポーネント参照】
    // 爆弾投げなど攻撃用
    // --------------------------------------------------------------
    [SerializeField] private PlayerAttack playerAttack;

    // --------------------------------------------------------------
    // 【投擲イベント】
    // 現在向きに応じて爆弾投げ
    // --------------------------------------------------------------
    public void ThrowBombEvent()
    {
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        playerAttack.ThrowBomb(direction);
    }

    #endregion


    #region ワイヤー状態管理

    // --------------------------------------------------------------
    // 【ワイヤー掴まり状態管理】
    // 掴まり演出の一時状態
    // --------------------------------------------------------------
    private bool _justGrappled = false;
    private float _grappleTimer = 0f;

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
        // アニメーターが初期化されていない場合は処理を中断
        if (_animator == null)
        {
            Debug.LogWarning("Animator is not yet initialized. Skipping UpdateJumpState call.");
            return;
        }

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
    /// ダメージのアニメーションを再生する。
    /// プレイヤーの向きを指定方向に合わせる。
    /// </summary>
    /// <param name="direction">プレイヤーの向き（X方向：-1または1）</param>
    public void PlayDamageAnimation(float direction)
    {
        // Animator が設定されていない場合はエラーを出す
        if (_animator == null)
        {
            Debug.LogError("animatorController is NULL!");
        }

        // ダメージ状態をON
        IsDamagePlaying = true;

        // 状態を Damage に遷移させる（向きも反映）
        SetPlayerState(PlayerState.Damage, direction);
    }

    /// <summary>
    /// Damage アニメーションが終了した際に
    /// Animation Event から呼ばれるコールバック。
    /// 強制的に Idle に戻す。
    /// </summary>
    public void OnDamageAnimationEnd()
    {
        Debug.Log("[PlayerAnimatorController] Damage animation ended by Animation Event.");

        // ダメージ状態をOFF
        IsDamagePlaying = false;

        if(_pendingWireTransition == true)
        {
            // Damage 終了後、Wire中であれば強制的に Wire に遷移させる
            SetPlayerState(PlayerState.Wire, force: true);
        }
        else
        {
            // Damage 終了後、強制的に Idle に遷移させる
            SetPlayerState(PlayerState.Idle, force: true);
        }
    }

    /// <summary>
    /// ダメージのアニメーションを再生する。
    /// プレイヤーの向きを指定方向に合わせる。
    /// </summary>
    /// <param name="direction">プレイヤーの向き（X方向：-1または1）</param>
    public void PlayGoalAnimation(float direction)
    {
        // Animator が設定されていない場合はエラーを出す
        if (_animator == null)
        {
            Debug.LogError("animatorController is NULL!");
        }

        // 状態を Goal に遷移させる（向きも反映）
        SetPlayerState(PlayerState.Goal, direction);
    }

    /// <summary>
    /// ゴールアニメーションの再生が終了するまで待機し、
    /// アニメーション終了後にプレイヤーの状態をIdleに遷移させる。
    /// </summary>
    /// <returns>アニメーションの再生を待機するためのコルーチン</returns>
    private IEnumerator WaitForGoalAnimationToEnd()
    {
        // Animatorの現在のステート（レイヤー0）のアニメーションの長さを取得
        float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;

        // アニメーションの再生が完了するまで待機
        yield return new WaitForSeconds(animationLength);

        // アニメーションが終了したら、プレイヤーの状態をIdleに遷移させる
        SetPlayerState(PlayerState.Idle, 0f, 0f, true);
    }

    /// <summary>
    /// ダメージアニメーションの再生後、指定された遅延時間を待ってから
    /// プレイヤーの状態をIdleにリセットする。
    /// </summary>
    /// <param name="delay">待機する秒数（通常はダメージアニメーションの長さ）</param>
    /// <returns>状態リセット処理を行うコルーチン</returns>
    private IEnumerator ResetFromDamage(float delay)
    {
        // 指定された遅延時間だけ待機（ダメージアニメーションの再生時間など）
        yield return new WaitForSeconds(delay);

        // ダメージ再生中フラグを解除
        IsDamagePlaying = false;

        // 現在の状態がDamageであれば、Idle状態に戻す
        // 向きはlocalScale.xによって判断（右向きなら1、左向きなら-1）
        if (_currentState == PlayerState.Damage)
        {
            SetPlayerState(PlayerState.Idle, transform.localScale.x > 0 ? 1f : -1f, 0f, true);
        }
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
            scale.x = Mathf.Sign(moveInput) * Mathf.Abs(scale.x);

            // スケールを適用
            transform.localScale = scale;
        }
    }

}
