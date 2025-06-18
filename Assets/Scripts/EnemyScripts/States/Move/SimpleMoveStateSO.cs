using UnityEngine;

/// <summary>
/// 敵キャラクターの単純な左右移動ステート。
/// EnemyMoveStateSO を継承し、移動ロジックを Tick で定義する。
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyMove/SimpleMoveState")]
public class SimpleMoveStateSO : EnemyMoveStateSO
{
    /// <summary>
    /// 毎フレーム呼ばれる移動処理。
    /// アニメーションイベントで移動が禁止されていなければ、
    /// 左方向に一定速度で移動する。
    /// </summary>
    /// <param name="owner">ステートを持つ敵キャラクター</param>
    /// <param name="deltaTime">経過時間</param>
    public override void Tick(EnemyController owner, float deltaTime)
    {
        // アニメーションイベント中は移動を行わない
        if (owner.IsMovementDisabledByAnimation)
        {
            return;
        }

        // 左方向へ一定速度で移動する
        owner.transform.Translate(Vector2.left * owner.MoveSpeed * deltaTime);
    }
}
