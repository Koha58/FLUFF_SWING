using UnityEngine;

/// <summary>
/// 地上敵用の移動スクリプト。
/// 
/// ・見た目のジャンプ／バウンドは「アニメーション側」で行う
/// ・このスクリプトは「移動するかどうか」だけを制御する
/// ・ジャンプ中（空中にいる見た目のフレーム）だけ左方向に移動する
/// 
/// ※ Rigidbody / Collider は使用しない前提
/// ※ タイトル画面など、当たり判定が不要な演出用途向け
/// </summary>
public class GroundEnemyWalker : MonoBehaviour
{
    /// <summary>
    /// 空中にいる間だけ適用される移動速度（左方向）
    /// 数値を大きくすると、ジャンプ1回あたりの前進距離が伸びる
    /// </summary>
    [SerializeField] private float speed = 2f;

    /// <summary>
    /// 現在「空中状態」かどうかを表すフラグ
    /// アニメーションイベントから ON / OFF される
    /// </summary>
    private bool _inAir;

    void Update()
    {
        // 空中でなければ一切移動しない
        // → 地面にいる間に滑って進む現象を防ぐ
        if (!_inAir) return;

        // 空中にいる間だけ左方向へ移動させる
        // 見た目のジャンプ動作と同期して「跳ねながら進む」ように見える
        transform.position += Vector3.left * speed * Time.deltaTime;
    }

    /// <summary>
    /// ジャンプ開始時に呼ばれるメソッド
    /// 
    /// Animation Event として、
    /// ・離陸フレーム
    /// ・足が地面から離れた瞬間
    /// に設定する
    /// </summary>
    public void OnJumpStart()
    {
        _inAir = true;
    }

    /// <summary>
    /// 着地時に呼ばれるメソッド
    /// 
    /// Animation Event として、
    /// ・着地フレーム
    /// ・足が地面に接地した瞬間
    /// に設定する
    /// </summary>
    public void OnJumpEnd()
    {
        _inAir = false;
    }
}
