using UnityEngine;

/// <summary>
/// キャラクターの共通基本情報を管理する抽象クラス
/// ScriptableObjectとしてデータを保持し、
/// プレイヤーや敵など様々なキャラクターが継承して使用することを想定している
/// </summary>
public abstract class CharacterBase : ScriptableObject
{
    /// <summary>キャラクターの一意識別ID</summary>
    public int id;

    /// <summary>キャラクター名</summary>
    public string characterName;

    /// <summary>キャラクターの移動速度</summary>
    public float moveSpeed;
}