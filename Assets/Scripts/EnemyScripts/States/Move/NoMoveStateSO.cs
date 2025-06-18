using UnityEngine;

/// <summary>
/// 敵キャラクターの「移動しないステート」。
/// EnemyMoveStateSO を継承するが、 Tick 内で移動処理を行わない。
/// ボスや固定砲台などの「その場で攻撃のみを行う敵」に使用する。
/// </summary>
[CreateAssetMenu(menuName = "State/EnemyMove/NoMoveState")]
public class NoMoveStateSO : EnemyMoveStateSO
{
    /// <summary>
    /// 毎フレーム呼ばれるが、移動処理は行わない。
    /// </summary>
    /// <param name="owner">ステートを持つ敵キャラクター</param>
    /// <param name="deltaTime">経過時間</param>
    public override void Tick(EnemyController owner, float deltaTime)
    {
        // 移動処理なし（静止状態）
    }
}
