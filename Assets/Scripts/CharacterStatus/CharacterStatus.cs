using UnityEngine;

/// <summary>
/// キャラクターごとの個別ステータス情報を保持するクラス
/// CharacterBase を継承し、追加のステータス（HPや攻撃力など）を定義する
/// ScriptableObject としてアセット化され、データ管理に使用される
/// </summary>
[CreateAssetMenu(fileName = "CharacterStatus", menuName = "Master/CharacterStatus")]
public class CharacterStatus : CharacterBase
{
    /// <summary>最大HP</summary>
    public int maxHP;

    /// <summary>攻撃力</summary>
    public int attack;

    /// <summary>近距離攻撃可能な範囲（使わない場合は0）</summary>
    public float meleeRange;

    /// <summary>遠距離攻撃可能な最大範囲（使わない場合は0）</summary>
    public float attackRadius;
}