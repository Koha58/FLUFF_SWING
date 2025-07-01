using UnityEngine;

/// <summary>
/// プレイヤーが触れたときにコインとしての処理を行うクラス。
/// コインUIを更新し、オブジェクトプールへ返却する。
/// </summary>
public class Coin : MonoBehaviour
{
    /// <summary>
    /// 他のColliderと接触した際に呼ばれる（2D用）。
    /// プレイヤーと接触した場合、コイン取得処理を実行。
    /// </summary>
    /// <param name="collision">接触したCollider情報</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 接触した相手がプレイヤーかどうか確認
        if (collision.CompareTag("Player"))
        {
            // コインUIのカウントを1つ増やす（シングルトンを使用）
            PlayerCoinUI.Instance.AddCoin(1);

            // このコインを非アクティブにしてオブジェクトプールへ返却
            CoinPoolManager.Instance.ReturnCoin(this.gameObject);
        }
    }
}
