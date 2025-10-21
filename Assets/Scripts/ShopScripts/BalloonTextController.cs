using System.Collections;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// キャラクターの頭上に吹き出しテキストを表示するコントローラークラス。
/// - プレイヤーとの距離に応じて自動的に表示／非表示を切り替える
/// - 吹き出し内のテキストはタイピング風に1文字ずつ表示される
/// - 吹き出しはキャラクターの頭上に固定され、常にカメラの方を向く
/// </summary>
public class BalloonTextController : MonoBehaviour
{
    //===============================
    // 定数定義
    //===============================

    // 初期表示する吹き出しメッセージ
    private const string DEFAULT_INITIAL_MESSAGE = "ボタンをライフに交換する？";

    // 1文字を表示するのにかかる時間（秒）
    private const float DEFAULT_TYPING_SPEED = 0.05f;

    // プレイヤーがこの距離以内に近づくと吹き出しが表示される（メートル）
    private const float DEFAULT_SHOW_DISTANCE = 3.0f;

    // 吹き出しの表示位置をキャラクターの頭上にずらすオフセット
    private static readonly Vector3 DEFAULT_BALLOON_OFFSET = new Vector3(0f, 1.5f, 0f);

    // タイピング音の設定
    private static readonly string[] TYPING_SE_NAMES = { "Typing1", "Typing2", "Typing3" };
    private const float TYPING_SE_COOLDOWN = 1.0f;         // SEを鳴らす最小間隔（連打防止）
    private const float TYPING_SE_PLAY_PROBABILITY = 0.7f;  // SEが鳴る確率（1.0 = 常に鳴る）

    //===============================
    // フィールド変数
    //===============================
    private TextMeshProUGUI dialogueText;     // 吹き出し内のテキスト
    private Canvas balloonCanvas;             // 吹き出しを表示するキャンバス
    private RectTransform textRectTransform;  // テキストのRectTransform（サイズ変更用）
    private Transform player;                 // プレイヤーのTransform
    private Camera mainCamera;                // メインカメラ

    private string fullText = DEFAULT_INITIAL_MESSAGE; // 現在表示するメッセージ全文
    private Vector2 originalTextSize;                 // テキストの初期サイズを保存
    private Coroutine typingCoroutine;                // タイピングアニメーション用コルーチン参照

    private bool isDisplayed = false;        // 現在吹き出しが表示中か
    private bool isFullyDisplayed = false;   // テキストが最後まで表示されたかどうか

    private float typingSpeed = DEFAULT_TYPING_SPEED; // 現在のタイピング速度
    private float showDistance = DEFAULT_SHOW_DISTANCE; // 表示を切り替える距離
    private Vector3 offset = DEFAULT_BALLOON_OFFSET;     // 表示位置のオフセット

    private float lastTypeSETime = 0f;  // 最後にタイピングSEを鳴らした時間を記録

    //===============================
    // プロパティ・イベント
    //===============================
    public bool IsFullyDisplayed => isFullyDisplayed; // 外部から参照可能な表示完了フラグ
    public event System.Action OnFullyDisplayed;      // タイピング完了時のイベント

    //===============================
    // Unity標準コールバック
    //===============================
    private void Start()
    {
        // --- 必要なコンポーネントの取得 ---
        balloonCanvas = GetComponentInChildren<Canvas>();
        dialogueText = GetComponentInChildren<TextMeshProUGUI>();
        textRectTransform = dialogueText?.GetComponent<RectTransform>();

        // 必須要素が見つからなければ動作を停止
        if (balloonCanvas == null || dialogueText == null || textRectTransform == null)
        {
            Debug.LogError("CanvasまたはTextMeshProUGUIまたはRectTransformが見つかりません。");
            enabled = false;
            return;
        }

        // テキストの初期サイズを保持（後でリセット用）
        originalTextSize = textRectTransform.sizeDelta;

        // 初期状態は非表示
        fullText = DEFAULT_INITIAL_MESSAGE;
        balloonCanvas.enabled = false;

        // --- プレイヤーとカメラの取得 ---
        mainCamera = Camera.main;
        player = GameObject.FindWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Playerオブジェクトが見つかりません。");
            enabled = false;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, player.position);

        // 一定距離以内なら吹き出しを表示
        if (distance < showDistance && !isDisplayed)
        {
            ShowBalloon();
        }
        // 離れたら非表示に
        else if (distance >= showDistance && isDisplayed)
        {
            HideBalloon();
        }

        // 吹き出しが表示中なら、頭上に追従＆カメラに正対
        if (isDisplayed)
        {
            balloonCanvas.transform.position = transform.position + offset;
            balloonCanvas.transform.LookAt(
                balloonCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    //===============================
    // 吹き出し制御処理
    //===============================

    /// <summary>
    /// 吹き出しを表示して、テキストのタイピングアニメーションを開始する。
    /// 既に表示中であれば、コルーチンを一旦止めてリスタート。
    /// </summary>
    private void ShowBalloon()
    {
        isDisplayed = true;
        balloonCanvas.enabled = true;
        dialogueText.text = "";
        isFullyDisplayed = false;

        // 前回のタイピングが続いていた場合は停止
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // タイピング開始
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// 吹き出しを非表示にして、アニメーションを停止。
    /// </summary>
    private void HideBalloon()
    {
        isDisplayed = false;

        // タイピング中なら中断
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // テキストとCanvasをリセット
        dialogueText.text = "";
        balloonCanvas.enabled = false;
        isFullyDisplayed = false;
        textRectTransform.sizeDelta = originalTextSize;
    }

    /// <summary>
    /// 指定したメッセージを吹き出しに表示する。
    /// テキスト幅や位置オフセットもオプションで指定可能。
    /// </summary>
    public void ShowMessage(string message, float optionalWidth = -1f, Vector2? customOffset = null)
    {
        // サイズ・位置を初期化
        if (textRectTransform != null)
        {
            textRectTransform.sizeDelta = originalTextSize;
            textRectTransform.anchoredPosition = Vector2.zero;
        }

        // 表示メッセージ更新
        fullText = message;

        // 幅指定がある場合は変更
        if (optionalWidth > 0 && textRectTransform != null)
            textRectTransform.sizeDelta = new Vector2(optionalWidth, originalTextSize.y);

        // 位置オフセットが指定されていれば適用
        if (customOffset.HasValue && textRectTransform != null)
            textRectTransform.anchoredPosition = customOffset.Value;

        // 表示開始
        ShowBalloon();
    }

    /// <summary>
    /// 初期メッセージ（デフォルト文）に戻す。
    /// </summary>
    public void RestoreInitialMessage()
    {
        ShowMessage(DEFAULT_INITIAL_MESSAGE);
    }

    //===============================
    // タイピングアニメーション処理
    //===============================

    /// <summary>
    /// テキストを1文字ずつ表示するコルーチン。
    /// 文字ごとに効果音を鳴らし、全表示後にイベントを発行。
    /// </summary>
    private IEnumerator TypeText()
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            // 部分文字列を切り出して表示（例: "ボ", "ボタ", "ボタン"...）
            dialogueText.text = fullText.Substring(0, i);

            if (i < fullText.Length)
            {
                char c = fullText[i];

                // スペースではない && 一定時間経過 && 確率チェックに合格したらSE再生
                bool canPlaySE = !char.IsWhiteSpace(c)
                                 && Time.time - lastTypeSETime > TYPING_SE_COOLDOWN
                                 && UnityEngine.Random.value < TYPING_SE_PLAY_PROBABILITY;

                if (canPlaySE)
                {
                    // ランダムなタイプ音を選んで再生
                    string seName = TYPING_SE_NAMES[UnityEngine.Random.Range(0, TYPING_SE_NAMES.Length)];
                    AudioManager.Instance.PlaySE(seName);
                    lastTypeSETime = Time.time;
                }
            }

            // 次の文字までの待機（タイプ速度に応じて）
            yield return new WaitForSeconds(typingSpeed);
        }

        // 全文表示完了
        isFullyDisplayed = true;
        OnFullyDisplayed?.Invoke();
    }
}
