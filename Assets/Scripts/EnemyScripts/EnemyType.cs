/// <summary>
/// 敵キャラクターの種類を表す列挙型
/// ゲーム内での挙動やステート遷移の分岐に使用する
/// </summary>
public enum EnemyType
{
    /// <summary>巡回（パトロール）する敵</summary>
    Patrol,

    /// <summary>空を飛ぶ鳥タイプの敵</summary>
    Bird,

    /// <summary>単純な動作をする敵（例：近接攻撃など）</summary>
    Simple,

    /// <summary>移動しない敵（固定砲台など）</summary>
    NoMove,

    // 必要に応じて敵タイプを追加してください
}
