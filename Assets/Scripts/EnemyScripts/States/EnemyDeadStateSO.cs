using UnityEngine;

/// <summary>
/// 敵の死亡状態を定義する ScriptableObject。
/// 死亡アニメーションの再生と、終了後のプール返却を管理する。
/// 死亡アニメーションがない場合は「マリオ風に反転して落下」演出を行う。
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyDeadState")]
public class EnemyDeadStateSO : EnemyStateSO
{
    #region === 設定フィールド ===

    [Header("死亡処理設定")]
    [Tooltip("死亡アニメーション再生後に待機する秒数（アニメーションがある場合）")]
    [SerializeField] private float waitAfterDeathAnimation = 1.5f; // 死亡アニメ再生後の待機時間

    [Header("マリオ風死亡演出設定")]
    [Tooltip("死亡時に上方向へ吹き飛ばす力（重力が有効な場合）")]
    [SerializeField] private float deathJumpForce = 6f; // 上方向への吹き飛ばし力

    [Tooltip("死亡後に落下してプールへ返却するまでの時間")]
    [SerializeField] private float deathFallDuration = 2.5f; // 落下演出時間

    [Tooltip("死亡演出中に手前に表示するSortingOrder")]
    [SerializeField] private int deathSortingOrder = 9; // 死亡演出用の表示優先度

    [Header("物理設定")]
    [Tooltip("Dynamic時の重力スケール")]
    [SerializeField] private float dynamicGravityScale = 1f; // Dynamic Rigidbody 用重力

    [Tooltip("Kinematic時の重力スケール")]
    [SerializeField] private float kinematicGravityScale = 0f; // Kinematic Rigidbody 用重力

    [Tooltip("初期速度（Rigidbodyリセット時に使用）")]
    [SerializeField] private Vector2 initialVelocity = Vector2.zero; // Rigidbody初期化用

    [Tooltip("上方向を表すベクトル（死亡時の吹き飛ばし方向）")]
    [SerializeField] private Vector2 upDirection = Vector2.up; // 上方向

    #endregion

    #region === 状態遷移 ===

    /// <summary>
    /// 死亡状態に入ったときの処理
    /// ・アニメあり → アニメ再生後に返却  
    /// ・アニメなし → 逆さまにして落下（マリオ風）
    /// </summary>
    public override void Enter(EnemyController owner)
    {
        base.Enter(owner);

        var animatorCtrl = owner.GetAnimationController();

        if (animatorCtrl && owner.StateMachineSO.usesDead)
        {
            // 死亡アニメ再生
            animatorCtrl.PlayDeadAnimation();

            // アニメ終了後に返却
            owner.StartCoroutine(WaitAndHandleDead(owner));
        }
        else
        {
            // アニメなし → マリオ風死亡演出
            owner.StartCoroutine(PerformMarioStyleDeath(owner));
        }
    }

    /// <summary>
    /// 毎フレーム更新（死亡中は特になし）
    /// </summary>
    public override void Tick(EnemyController owner, float deltaTime) { }

    /// <summary>
    /// 死亡状態から抜けるときの処理（再出現時）
    /// </summary>
    public override void Exit(EnemyController owner)
    {
        // SpriteRenderer を元に戻す
        var sr = owner.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipY = false; // 上下反転を解除
            sr.sortingOrder = owner.OriginalSortingOrder; // 元の表示順に戻す
        }

        // Rigidbody の状態を復元
        var rb = owner.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = initialVelocity; // 速度リセット

            // Dynamicを維持する敵かどうかで分岐
            if (owner.keepDynamicBody)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = dynamicGravityScale;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = kinematicGravityScale;
            }
        }

        // 全コライダーを再有効化
        Collider2D[] colliders = owner.GetComponents<Collider2D>();
        foreach (var c in colliders)
            c.enabled = true;
    }

    #endregion

    #region === コルーチン ===

    /// <summary>
    /// 死亡アニメ終了後にプールへ返却
    /// </summary>
    private System.Collections.IEnumerator WaitAndHandleDead(EnemyController owner)
    {
        Animator animator = owner.GetAnimationController()?.GetComponent<Animator>();

        // アニメーション再生後に待機
        if (animator != null)
            yield return new WaitForSeconds(waitAfterDeathAnimation);
        else
            yield return null;

        // プールへ返却
        owner.HandleDead();
    }

    /// <summary>
    /// 死亡アニメなしの敵を「マリオ風」に反転・落下させる
    /// </summary>
    private System.Collections.IEnumerator PerformMarioStyleDeath(EnemyController owner)
    {
        Rigidbody2D rb = owner.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = owner.GetComponent<SpriteRenderer>();

        // すべてのコライダーを無効化して衝突を防ぐ
        Collider2D[] colliders = owner.GetComponents<Collider2D>();
        foreach (var c in colliders)
            c.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = initialVelocity; // 速度リセット
            rb.bodyType = RigidbodyType2D.Dynamic; // 動的にする
            rb.gravityScale = dynamicGravityScale; // 重力有効
            rb.AddForce(upDirection * deathJumpForce, ForceMode2D.Impulse); // 上方向に吹き飛ばす
        }

        if (sr != null)
        {
            sr.flipY = true; // 上下反転
            sr.sortingOrder = deathSortingOrder; // 前面に表示
        }

        // 落下演出時間待機
        yield return new WaitForSeconds(deathFallDuration);

        // プールへ返却
        owner.HandleDead();
    }

    #endregion
}
