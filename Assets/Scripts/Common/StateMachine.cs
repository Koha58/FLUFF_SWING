/// <summary>
/// 汎用的なステートマシンのクラス
/// ジェネリック型Tは状態を管理するオーナークラスの型
/// </summary>
/// <typeparam name="T">状態を管理するオーナークラスの型</typeparam>
public class StateMachine<T>
{
    private T owner;                 // このステートマシンが管理するオーナークラスのインスタンス
    private IState<T> currentState;  // 現在の状態（State）

    /// <summary>
    /// コンストラクタ。オーナークラスのインスタンスを受け取る
    /// </summary>
    /// <param name="owner">状態を管理するオーナークラスのインスタンス</param>
    public StateMachine(T owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// 状態を切り替える
    /// </summary>
    /// <param name="newState">新しく遷移する状態</param>
    public void ChangeState(IState<T> newState)
    {
        // 現在の状態から抜ける処理を呼ぶ
        currentState?.Exit(owner);

        // 新しい状態に切り替え
        currentState = newState;

        // 新しい状態の初期化処理を呼ぶ
        currentState?.Enter(owner);
    }

    /// <summary>
    /// 毎フレーム呼ばれる更新処理
    /// 現在の状態のTickを呼ぶ
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間（秒）</param>
    public void Update(float deltaTime)
    {
        currentState?.Tick(owner, deltaTime);
    }
}
