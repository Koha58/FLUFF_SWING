/// <summary>
/// 状態パターン用の状態インターフェース
/// ジェネリック型Tは状態を持つオーナークラスの型
/// </summary>
/// <typeparam name="T">状態を管理するオーナークラスの型</typeparam>
public interface IState<T>
{
    /// <summary>
    /// 状態に入った時の初期化処理
    /// </summary>
    /// <param name="owner">状態のオーナーオブジェクト</param>
    void Enter(T owner);

    /// <summary>
    /// 毎フレーム呼ばれる状態の更新処理
    /// </summary>
    /// <param name="owner">状態のオーナーオブジェクト</param>
    /// <param name="deltaTime">前フレームからの経過時間（秒）</param>
    void Tick(T owner, float deltaTime);

    /// <summary>
    /// 状態から抜ける時の終了処理
    /// </summary>
    /// <param name="owner">状態のオーナーオブジェクト</param>
    void Exit(T owner);
}
