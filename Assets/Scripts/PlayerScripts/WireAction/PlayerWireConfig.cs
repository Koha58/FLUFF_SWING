using UnityEngine;

/// <summary>
/// プレイヤーのワイヤー関連の設定データを保持する ScriptableObject
/// インスペクター上で各種パラメータを調整可能
/// </summary>
[CreateAssetMenu(fileName = "PlayerWireConfig", menuName = "Config/PlayerWireConfig")]
public class PlayerWireConfig : ScriptableObject
{
    /// <summary>ワイヤーの固定長さ（単位: ユニット）</summary>
    public float fixedWireLength = 3.5f;

    /// <summary>針の飛ぶ速度（単位: ユニット/フレームなど）</summary>
    public float needleSpeed = 0.3f;

    /// <summary>スイング開始時の初速（単位: ユニット/秒）</summary>
    public float swingInitialSpeed = 10f;

    /// <summary>ワイヤー接続中のプレイヤーの重力スケール</summary>
    public float playerGravityScale = 3f;

    /// <summary>プレイヤーの物理挙動に影響を与える空気抵抗（線形減衰）</summary>
    public float linearDamping = 0f;

    /// <summary>プレイヤーの物理挙動に影響を与える回転減衰</summary>
    public float angularDamping = 0f;
}
