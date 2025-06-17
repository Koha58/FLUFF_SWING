using UnityEngine;

/// <summary>
/// ScriptableObjectで作る状態の基底クラス（ジェネリック）
/// IState<T>インターフェースを実装し、状態として使うための抽象クラス
/// </summary>
/// <typeparam name="T">状態のオーナークラスの型</typeparam>
public abstract class StateSO<T> : ScriptableObject, IState<T>
{
    /// <summary>
    /// 状態に入った時の処理（抽象メソッド）
    /// </summary>
    /// <param name="owner">状態のオーナークラス</param>
    public abstract void Enter(T owner);

    /// <summary>
    /// 毎フレーム呼ばれる状態の更新処理（抽象メソッド）
    /// </summary>
    /// <param name="owner">状態のオーナークラス</param>
    /// <param name="deltaTime">前フレームからの経過時間（秒）</param>
    public abstract void Tick(T owner, float deltaTime);

    /// <summary>
    /// 状態を抜ける時の処理（抽象メソッド）
    /// </summary>
    /// <param name="owner">状態のオーナークラス</param>
    public abstract void Exit(T owner);
}
