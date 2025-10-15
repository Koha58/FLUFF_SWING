using System.Collections;
using UnityEngine;

/// <summary>
/// プレイヤーがNPCに近づいた際に吹き出しUIを表示・制御するクラス。
/// プレイヤーが一定距離内にいるかを判定し、NPCの吹き出しメッセージの表示や
/// UIのON/OFFを管理する。また、プレイヤーの「はい」「いいえ」ボタン操作を受けて
/// 回復やメッセージ切り替えを行う。
/// </summary>
public class PlayerBalloonUIController : MonoBehaviour
{
    /// <summary>NPCとの会話UIパネル本体</summary>
    [SerializeField] private GameObject uiPanel;

    /// <summary>NPCの吹き出しテキスト制御クラス</summary>
    [SerializeField] private BalloonTextController npcBalloon;

    /// <summary>NPCのTransform（位置取得用）</summary>
    [SerializeField] private Transform npcTransform;

    /// <summary>吹き出しテキストのRectTransform（位置調整用）</summary>
    [SerializeField] private RectTransform balloonTextRectTransform;

    /// <summary>プレイヤーの体力UI制御クラス</summary>
    [SerializeField] private PlayerHealthUI playerHealthUI;

    /// <summary>UI表示を開始する距離の閾値</summary>
    [SerializeField] private float triggerDistance = 3f;

    /// <summary>回復に必要なコイン枚数</summary>
    [SerializeField] private int requiredCoinAmount = 5;

    /// <summary>メッセージを元に戻すまでの遅延秒数</summary>
    [SerializeField] private float restoreDelay = 3f;

    /// <summary>「まいどあり」時に再生するSE</summary>
    [SerializeField] private AudioClip thankYouSE;

    /// <summary>「まいどあり」時に再生するSE</summary>
    [SerializeField] private AudioClip GetLifeSE;

    /// <summary>プレイヤーの攻撃制御クラス（HP回復の際にコイン所持数を参照）</summary>
    private PlayerAttack playerAttack;

    /// <summary>感謝メッセージ時にテキストを下にずらす量</summary>
    private const float balloonShiftY = -25f;

    /// <summary>コイン不足時の吹き出し幅指定</summary>
    private const float insufficientMessageWidth = 220f;

    /// <summary>コイン不足メッセージのテキスト位置オフセット</summary>
    private static readonly Vector2 insufficientMessageOffset = new Vector2(-10f, 0f);

    /// <summary>プレイヤーがトリガー距離内にいるかどうかのフラグ</summary>
    private bool isInRange = false;

    /// <summary>メッセージ復帰用コルーチンの管理用</summary>
    private Coroutine restoreMessageCoroutine;

    /// <summary>メッセージ表示状態を管理する内部列挙型</summary>
    private enum MessageState
    {
        None,               // 通常状態
        ShowingThankYou,    // 感謝メッセージ表示中
        ShowingOriginal     // 元のメッセージ表示中
    }

    /// <summary>現在のメッセージ表示状態</summary>
    private MessageState currentMessageState = MessageState.None;

    /// <summary>吹き出しテキストの通常位置</summary>
    private Vector3 originalTextPosition;

    /// <summary>感謝メッセージ時にずらす位置</summary>
    private Vector3 shiftedTextPosition;

    /// <summary>
    /// 初期化処理。UIを非表示にし、吹き出しのイベント登録とテキスト位置の初期値を取得する。
    /// </summary>
    void Start()
    {
        // PlayerAttack をシーン内から検索して取得
        playerAttack = FindFirstObjectByType<PlayerAttack>();

        // UIパネルを最初は非表示に設定
        uiPanel.SetActive(false);

        // npcBalloonが設定されていれば、吹き出しが完全表示されたイベントにハンドラを登録
        if (npcBalloon != null)
        {
            npcBalloon.OnFullyDisplayed += OnBalloonFullyDisplayed;
        }

        // 吹き出しテキストのRectTransformがあれば初期位置とずらした位置を計算して保存
        if (balloonTextRectTransform != null)
        {
            originalTextPosition = balloonTextRectTransform.localPosition;
            shiftedTextPosition = originalTextPosition + new Vector3(0, balloonShiftY, 0);
        }
    }

    /// <summary>
    /// 毎フレーム呼ばれ、プレイヤーとNPCの距離を測定して
    /// UIの表示状態を切り替える。
    /// </summary>
    void Update()
    {
        // プレイヤー（このオブジェクト）とNPCの距離を計算
        float distance = Vector3.Distance(transform.position, npcTransform.position);

        // 直前の状態を保存
        bool wasInRange = isInRange;

        // トリガー距離内かどうか判定
        isInRange = distance < triggerDistance;

        // トリガー内に入った瞬間ならUI表示を試みる
        if (!wasInRange && isInRange)
        {
            TryShowUI();
        }
        // トリガー外に出た瞬間ならUIを非表示にする
        else if (wasInRange && !isInRange)
        {
            HideUI();
        }
    }

    /// <summary>
    /// NPCの吹き出しメッセージが完全表示されていて
    /// 感謝メッセージ表示中でなければUIパネルを表示する。
    /// </summary>
    private void TryShowUI()
    {
        // 吹き出しが完全表示されているかつ感謝メッセージ表示中でなければ表示
        if (npcBalloon.IsFullyDisplayed && currentMessageState != MessageState.ShowingThankYou)
        {
            uiPanel.SetActive(true);
        }
        else
        {
            // それ以外は非表示
            uiPanel.SetActive(false);
        }
    }

    /// <summary>
    /// NPCの吹き出しが完全表示された時に呼ばれるイベントハンドラ。
    /// UI表示条件を満たしていればUIを表示する。
    /// </summary>
    private void OnBalloonFullyDisplayed()
    {
        // 感謝メッセージ以外の状態で、プレイヤーが範囲内ならUI表示
        if (currentMessageState != MessageState.ShowingThankYou && isInRange)
        {
            uiPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 「はい」ボタンが押された際に呼ばれる。
    /// プレイヤーのコイン消費を試み、成功すれば回復し感謝メッセージを表示。
    /// コインが足りない場合は警告メッセージを表示する。
    /// </summary>
    public void OnClickYes()
    {
        // コインを消費できるかチェック
        if (PlayerCoinUI.Instance.TryUseCoins(requiredCoinAmount))
        {
            // コイン消費成功時は回復し感謝メッセージを表示
            playerAttack.Heal(1);

            // SEが設定されていれば再生
            if (thankYouSE != null)
            {
                AudioManager.Instance.PlaySE(thankYouSE);
                AudioManager.Instance.PlaySE(GetLifeSE);
            }

            HandleResponse("まいどあり！");
        }
        else
        {
            // コイン不足時は警告メッセージを表示
            HandleResponse("ボタンが足らないよ！", insufficientMessageWidth, insufficientMessageOffset);
        }
    }

    /// <summary>
    /// 指定メッセージをNPCの吹き出しに表示し、UIは非表示に切り替える。
    /// メッセージ表示後、一定時間経過で元のメッセージに戻す処理を開始する。
    /// </summary>
    /// <param name="message">表示する文字列</param>
    /// <param name="optionalWidth">吹き出しの幅（-1で自動調整）</param>
    /// <param name="offset">テキストの位置オフセット</param>
    private void HandleResponse(string message, float optionalWidth = -1f, Vector2? offset = null)
    {
        // UIパネルを非表示に切り替え
        HideUI();

        // NPCの吹き出しに新しいメッセージを表示（幅やオフセット指定可）
        npcBalloon.ShowMessage(message, optionalWidth, offset);

        // 既にメッセージ復帰のコルーチンが動いていれば停止する
        if (restoreMessageCoroutine != null)
        {
            StopCoroutine(restoreMessageCoroutine);
        }

        // 現在のメッセージ状態を感謝メッセージ表示中に変更
        currentMessageState = MessageState.ShowingThankYou;

        // 吹き出しテキストの位置をずらす（感謝メッセージ用）
        if (balloonTextRectTransform != null)
        {
            balloonTextRectTransform.localPosition = shiftedTextPosition;
        }

        // 一定時間後に元のメッセージに戻すコルーチンを開始
        restoreMessageCoroutine = StartCoroutine(ShowOriginalMessageWithUIDelay());
    }

    /// <summary>
    /// 「いいえ」ボタンが押された時に呼ばれる。
    /// UIを非表示に切り替えるだけの処理。
    /// </summary>
    public void OnClickNo()
    {
        // UIパネルを非表示に切り替え
        HideUI();
    }

    /// <summary>
    /// 一定時間待機した後にNPCの吹き出しメッセージを元のメッセージに戻すコルーチン。
    /// 元に戻った時のイベント登録もここで行う。
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator ShowOriginalMessageWithUIDelay()
    {
        // restoreDelay秒だけ待機
        yield return new WaitForSeconds(restoreDelay);

        // 古いイベントハンドラを解除
        npcBalloon.OnFullyDisplayed -= OnBalloonFullyDisplayed;

        // メッセージ状態を元のメッセージ表示中に変更
        currentMessageState = MessageState.ShowingOriginal;

        // 元に戻ったメッセージが完全表示されたとき用のイベントハンドラを登録
        npcBalloon.OnFullyDisplayed += OnBalloonFullyDisplayedAfterRestore;

        // NPC吹き出しメッセージを元に戻す
        npcBalloon.RestoreInitialMessage();

        // 吹き出しテキストの位置も元に戻す
        if (balloonTextRectTransform != null)
        {
            balloonTextRectTransform.localPosition = originalTextPosition;
        }

        // コルーチン管理用変数をリセット
        restoreMessageCoroutine = null;
    }

    /// <summary>
    /// 元のメッセージに戻った後、吹き出しが完全表示された際に呼ばれるイベントハンドラ。
    /// UIを再表示し、状態とイベント登録を元に戻す。
    /// </summary>
    private void OnBalloonFullyDisplayedAfterRestore()
    {
        // プレイヤーが範囲内ならUIを再表示
        if (isInRange)
        {
            uiPanel.SetActive(true);
        }

        // このイベントハンドラを解除
        npcBalloon.OnFullyDisplayed -= OnBalloonFullyDisplayedAfterRestore;

        // メッセージ状態を通常に戻す
        currentMessageState = MessageState.None;

        // 元のイベントハンドラを再登録
        npcBalloon.OnFullyDisplayed += OnBalloonFullyDisplayed;
    }

    /// <summary>
    /// UIパネルを非表示に切り替える。
    /// </summary>
    private void HideUI()
    {
        uiPanel.SetActive(false);
    }
}
