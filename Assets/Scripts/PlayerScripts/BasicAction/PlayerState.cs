/// <summary>
/// プレイヤーの状態を表す列挙型。
/// ゲーム内でのプレイヤーの行動やアニメーション制御に利用される。
/// </summary>
public enum PlayerState
{
    /// <summary>待機状態（何もしていない、停止中）</summary>
    Idle,

    /// <summary>走っている状態</summary>
    Run,

    /// <summary>ジャンプ中の状態</summary>
    Jump,

    /// <summary>ワイヤーアクション中（ワイヤー接続、スイングなど）</summary>
    Wire,

    /// <summary>着地した直後の状態（着地モーションなど）</summary>
    Landing,

    /// <summary>近接攻撃を行っている状態</summary>
    MeleeAttack,

    /// <summary>遠距離攻撃を行っている状態</summary>
    RangedAttack,

    /// <summary>ダメージを受けている状態</summary>
    Damage,

    /// <summary>ゴール（ステージクリア）した状態</summary>
    Goal,
}
