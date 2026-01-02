using UnityEngine;

/// <summary>
/// プレイヤーの後方に回り込んだオブジェクトを自動的に削除するコンポーネント。
/// 
/// ・無限生成（敵 / 演出オブジェクト）で必須
/// ・当たり判定や物理は一切使わない
/// ・Player は Tag から自動取得するため、Inspector設定不要
/// 
/// 主な用途：
/// ・タイトル画面の敵演出
/// ・背景用オブジェクト
/// ・奥に流れていく装飾要素
/// </summary>
public class DestroyBehind : MonoBehaviour
{
    // =========================================================
    // Player Auto Find
    // =========================================================

    [Header("Player Auto Find")]
    [Tooltip("Player を検索するためのタグ名")]
    [SerializeField] private string playerTag = "Player";

    // =========================================================
    // Destroy Settings
    // =========================================================

    [Header("Destroy Condition")]
    [Tooltip("プレイヤーからこの距離だけ後ろに行ったら削除される（ワールド距離）")]
    [SerializeField] private float destroyBehindDistance = 40f;

    /// <summary>
    /// 自動取得した Player の Transform
    /// 外部から触る必要がないため private
    /// </summary>
    private Transform player;

    // =========================================================
    // Unity Lifecycle
    // =========================================================

    void Awake()
    {
        // 起動時に一度だけ Player を探す
        // FindWithTag はやや重いので、毎フレームは行わない
        TryFindPlayer();
    }

    void Update()
    {
        // Player がまだ見つかっていなければ何もしない
        // （シーン構成ミスや Player が後から生成されるケース対策）
        if (!player) return;

        // 自分が Player の後方一定距離よりも左に来たら削除
        // ※ 右方向に進むゲーム前提
        if (transform.position.x < player.position.x - destroyBehindDistance)
        {
            Destroy(gameObject);
        }
    }

    // =========================================================
    // Utility
    // =========================================================

    /// <summary>
    /// Player タグを持つオブジェクトを検索して Transform を保持する
    /// </summary>
    private void TryFindPlayer()
    {
        var go = GameObject.FindWithTag(playerTag);

        if (go != null)
        {
            player = go.transform;
        }
        else
        {
            // 見つからなくても致命的ではないので Warning に留める
            Debug.LogWarning(
                $"DestroyBehind: Tag '{playerTag}' のオブジェクトが見つかりません",
                this
            );
        }
    }
}
