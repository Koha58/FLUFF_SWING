using UnityEngine;

/// <summary>
/// プレイヤーにUI表示を制御するクラス。
/// NPCの吹き出しが全文表示され、かつ距離内にいるとき、
/// Playerの子UI（はい／いいえボタン）を表示。
/// 押すか、距離外に出るとUIは非表示に。
/// 再び距離内に入れば、再表示される。
/// </summary>
public class PlayerBalloonUIController : MonoBehaviour
{
    /// <summary>はい/いいえボタンが配置されたUIパネル</summary>
    [SerializeField]
    private GameObject uiPanel;

    /// <summary>NPCの吹き出しテキスト制御コンポーネント</summary>
    [SerializeField]
    private BalloonTextController npcBalloon;

    /// <summary>NPCのTransform（位置情報取得用）</summary>
    [SerializeField]
    private Transform npcTransform;

    /// <summary>UI表示をトリガーする距離（3メートル）</summary>
    private float triggerDistance = 3f;

    /// <summary>プレイヤーが距離内にいるかの状態管理</summary>
    private bool isInRange = false;

    void Start()
    {
        // UIは最初非表示にする
        uiPanel.SetActive(false);

        // NPCの吹き出しが全文表示されたイベントにコールバック登録
        npcBalloon.OnFullyDisplayed += OnBalloonFullyDisplayed;
    }

    void Update()
    {
        // プレイヤーとNPC間の距離を計算
        float distance = Vector3.Distance(transform.position, npcTransform.position);

        // 前フレームの距離内判定を保存
        bool wasInRange = isInRange;

        // 今フレームの距離内判定を更新
        isInRange = distance < triggerDistance;

        // 距離外から距離内に入った瞬間
        if (!wasInRange && isInRange)
        {
            TryShowUI(); // UI表示判定・表示処理
        }

        // 距離内から距離外に出た瞬間
        if (wasInRange && !isInRange)
        {
            HideUI(); // UI非表示
        }
    }

    /// <summary>
    /// 距離内に入った時に呼ばれ、
    /// 吹き出しが全文表示されていればUIを表示する。
    /// </summary>
    private void TryShowUI()
    {
        Debug.Log("TryShowUI() 呼ばれた - IsFullyDisplayed = " + npcBalloon.IsFullyDisplayed);

        if (npcBalloon.IsFullyDisplayed)
        {
            Debug.Log("UI表示！");
            uiPanel.SetActive(true); // UIを表示
        }
        else
        {
            Debug.Log("吹き出し未完了なのでUI非表示");
            // UIは非表示のまま（表示しない）
        }
    }

    /// <summary>
    /// NPCの吹き出しが全文表示された際に呼ばれるイベントハンドラ。
    /// プレイヤーが距離内にいればUIを表示する。
    /// </summary>
    private void OnBalloonFullyDisplayed()
    {
        float distance = Vector3.Distance(transform.position, npcTransform.position);

        if (distance < triggerDistance)
        {
            uiPanel.SetActive(true); // UIを表示
        }
    }

    /// <summary>
    /// プレイヤーが「はい」ボタンを押した時の処理。
    /// UIを非表示にし、必要に応じて追加処理を行う。
    /// </summary>
    public void OnClickYes()
    {
        HideUI();
        // ここに「はい」選択時の処理を追加可能（例：アイテム交換など）
    }

    /// <summary>
    /// プレイヤーが「いいえ」ボタンを押した時の処理。
    /// UIを非表示にし、必要に応じてキャンセル処理を行う。
    /// </summary>
    public void OnClickNo()
    {
        HideUI();
        // ここに「いいえ」選択時のキャンセル処理を追加可能
    }

    /// <summary>
    /// UIパネルを非表示にする共通処理。
    /// </summary>
    private void HideUI()
    {
        uiPanel.SetActive(false);
    }
}
