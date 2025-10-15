using UnityEngine;

/// <summary>
/// プレイヤーが触れたときにコインとしての処理を行うクラス。
/// コインUIを更新し、オブジェクトプールへ返却する。
/// スイング中（ワイヤー使用中）でも確実に取得できるよう、
/// FixedUpdateで毎フレーム接触判定を行う設計。
/// </summary>
public class Coin : MonoBehaviour
{
    [Header("コイン獲得SE")]
    [SerializeField] private AudioClip coinSE; // コイン獲得時の効果音

    // FixedUpdateは物理演算の更新タイミングで呼ばれる
    // 毎フレーム、プレイヤーとの接触を確認
    private void FixedUpdate()
    {
        CheckForCoinOverlap();
    }

    /// <summary>
    /// コインの中心を中心とした一定半径内にプレイヤーが存在するかを判定する。
    /// 該当する場合、コイン獲得処理を行い、オブジェクトプールへ返却する。
    /// </summary>
    void CheckForCoinOverlap()
    {
        // 半径0.5fの円範囲内にあるすべてのCollider2Dを取得
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        foreach (var hit in hits)
        {
            // "Player"タグを持つオブジェクト（プレイヤー）との接触を検出
            if (hit.CompareTag("Player"))
            {
                // 🎵 AudioManager経由でSEを再生
                AudioManager.Instance.PlaySE(coinSE);

                // コインUIのカウントを1つ増やす（シングルトンパターンを使用）
                PlayerCoinUI.Instance.AddCoin(1);

                // このコインを非アクティブ化し、オブジェクトプールに返却
                CoinPoolManager.Instance.ReturnCoin(this.gameObject);

                // 1つのプレイヤーにのみ反応すれば十分なのでループを抜ける
                break;
            }
        }
    }
}
