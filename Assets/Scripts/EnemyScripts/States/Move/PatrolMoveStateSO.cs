using UnityEngine;

/// <summary>
/// 敵キャラクターのパトロール移動ステート。
/// 一定範囲内を左右に往復移動するロジックを持つ。
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyMove/PatrolMoveState")]
public class PatrolMoveStateSO : EnemyMoveStateSO
{
    /// <summary>
    /// パトロールする範囲の幅（左右の移動距離）
    /// </summary>
    public float patrolRange = 3f;

    /// <summary>
    /// ステート開始時に初期位置と移動方向を設定する。
    /// </summary>
    /// <param name="owner">ステートを持つ敵キャラクター</param>
    public override void Enter(EnemyController owner)
    {
        base.Enter(owner);

        // 現在位置をパトロール開始位置として記録
        owner.PatrolStartX = owner.transform.position.x;

        // 初期の移動方向を左に設定
        owner.PatrolDirection = -1;
    }

    /// <summary>
    /// 毎フレーム呼ばれるパトロール移動処理。
    /// 範囲を超えたら方向を反転し、スプライトも反転する。
    /// </summary>
    /// <param name="owner">ステートを持つ敵キャラクター</param>
    /// <param name="deltaTime">経過時間</param>
    public override void Tick(EnemyController owner, float deltaTime)
    {
        // アニメーションイベントで移動が禁止されている場合は移動しない
        if (owner.IsMovementDisabledByAnimation) return;

        // 現在の方向に向かって移動
        owner.transform.Translate(Vector2.right * owner.PatrolDirection * owner.MoveSpeed * deltaTime);

        // パトロール範囲を超えたら方向を反転
        if (Mathf.Abs(owner.transform.position.x - owner.PatrolStartX) >= patrolRange)
        {
            owner.PatrolDirection *= -1;

            // スプライトを左右反転
            Flip(owner);
        }
    }

    /// <summary>
    /// ステート終了時の処理。
    /// 基底クラスの Exit を呼び出す。
    /// </summary>
    /// <param name="owner">ステートを持つ敵キャラクター</param>
    public override void Exit(EnemyController owner)
    {
        base.Exit(owner);
    }

    /// <summary>
    /// 敵キャラクターのスプライトを左右反転する。
    /// </summary>
    /// <param name="owner">ステートを持つ敵キャラクター</param>
    private void Flip(EnemyController owner)
    {
        Vector3 scale = owner.transform.localScale;
        scale.x *= -1;
        owner.transform.localScale = scale;
    }
}
