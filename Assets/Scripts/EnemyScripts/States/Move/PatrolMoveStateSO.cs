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

        // スポーンマネージャーが方向を設定していなければ初期化
        if (owner.Direction == 0) // Directionが初期値のままなら
        {
            owner.Direction = -1;
        }
    }

    /// <summary>
    /// 毎フレーム呼ばれるパトロール移動処理。
    /// 範囲を超えたら方向を反転し、スプライトも反転する。
    /// </summary>
    /// <param name="owner">ステートを持つ敵キャラクター</param>
    /// <param name="deltaTime">経過時間</param>
    public override void Tick(EnemyController owner, float deltaTime)
    {
        if (owner.IsMovementDisabledByAnimation) return;

        owner.transform.Translate(Vector2.right * owner.Direction * owner.MoveSpeed * deltaTime);

        float currentX = owner.transform.position.x;
        float startX = owner.PatrolStartX;

        // パトロール範囲を超えたら方向を反転
        if (Mathf.Abs(currentX - startX) >= patrolRange)
        {
            // 1. 【位置補正】範囲のちょうど端に戻す
            // 移動方向(Direction)が1なら右端、-1なら左端に補正
            float clampedX = startX + owner.Direction * patrolRange;
            owner.transform.position = new Vector3(clampedX, owner.transform.position.y, owner.transform.position.z);

            // 2. 【方向反転】EnemyControllerのメソッドでDirectionと見た目を反転
            owner.ReverseDirection();
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
}
