using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// キャラクターの頭上に吹き出しテキストを表示するコントローラークラス。
/// ・プレイヤーとの距離に応じて吹き出しの表示・非表示を自動切替
/// ・テキストはタイピング風に1文字ずつ表示
/// ・表示位置はキャラクターの頭上に固定し、常にカメラを向く
/// </summary>
public class BalloonTextController : MonoBehaviour
{
    // --- 定数定義 ---
    // 初期表示する吹き出しのメッセージ
    private const string DefaultInitialMessage = "ボタンをライフに交換する？";

    // タイピングアニメーションの1文字表示時間（秒）
    private const float DefaultTypingSpeed = 0.05f;

    // プレイヤーから吹き出しを表示する距離の閾値（メートル）
    private const float DefaultShowDistance = 3.0f;

    // 吹き出しを表示する際のキャラクターの頭上へのオフセット位置
    private static readonly Vector3 DefaultOffset = new Vector3(0, 1.5f, 0);

    // 吹き出しのテキストコンポーネント
    private TextMeshProUGUI dialogueText;

    // 吹き出しのキャンバスコンポーネント
    private Canvas balloonCanvas;

    // テキストのRectTransform（幅や位置調整用）
    private RectTransform textRectTransform;

    // プレイヤーのTransform参照
    private Transform player;

    // メインカメラ参照
    private Camera mainCamera;

    // 表示したいメッセージ全文
    private string fullText = DefaultInitialMessage;

    // テキスト初期サイズ保存（幅や位置調整のリセット用）
    private Vector2 originalTextSize;

    // タイピングアニメーションのコルーチン参照
    private Coroutine typingCoroutine;

    // 吹き出しが現在表示中かどうかの状態管理
    private bool isDisplayed = false;

    // タイピングアニメーションが完了したかのフラグ
    private bool isFullyDisplayed = false;

    // タイピング表示が完了したか取得用プロパティ
    public bool IsFullyDisplayed => isFullyDisplayed;

    // タイピング完了時に呼ばれるイベント
    public event System.Action OnFullyDisplayed;

    // 1文字あたりの表示時間（秒）
    private float typingSpeed = DefaultTypingSpeed;

    // プレイヤーからの表示距離閾値
    private float showDistance = DefaultShowDistance;

    // 吹き出しの表示位置オフセット（キャラ頭上）
    private Vector3 offset = DefaultOffset;

    /// <summary>
    /// 初期化処理
    /// ・コンポーネント取得と存在チェック
    /// ・プレイヤーとカメラの参照取得
    /// ・吹き出しの初期非表示設定
    /// </summary>
    void Start()
    {
        // 吹き出し用Canvasとテキスト、RectTransformを取得
        balloonCanvas = GetComponentInChildren<Canvas>();
        dialogueText = GetComponentInChildren<TextMeshProUGUI>();
        textRectTransform = dialogueText?.GetComponent<RectTransform>();

        // 必須コンポーネントが揃っていなければエラー出力し、処理停止
        if (balloonCanvas == null || dialogueText == null || textRectTransform == null)
        {
            Debug.LogError("CanvasまたはTextMeshProUGUIまたはRectTransformが見つかりません。");
            enabled = false;
            return;
        }

        // テキストの元サイズを保存（幅や位置調整のリセット用）
        originalTextSize = textRectTransform.sizeDelta;

        // 初期表示メッセージをセット
        fullText = DefaultInitialMessage;

        // 吹き出しを最初は非表示に設定
        balloonCanvas.enabled = false;

        // メインカメラの参照を取得
        mainCamera = Camera.main;

        // Playerオブジェクトをタグで探しTransformを取得
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Playerオブジェクトが見つかりません。");
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// 毎フレーム更新処理
    /// ・プレイヤーとの距離を測り吹き出し表示のON/OFF制御
    /// ・吹き出しの位置をキャラクター頭上に固定しカメラ方向を向く処理
    /// </summary>
    void Update()
    {
        // プレイヤーが存在しなければ処理しない
        if (player == null) return;

        // キャラとプレイヤー間の距離を取得
        float distance = Vector3.Distance(transform.position, player.position);

        // プレイヤーが近ければ吹き出し表示開始、離れれば非表示
        if (distance < showDistance && !isDisplayed)
        {
            ShowBalloon();
        }
        else if (distance >= showDistance && isDisplayed)
        {
            HideBalloon();
        }

        // 吹き出しが表示中なら位置更新してカメラを向く
        if (isDisplayed)
        {
            // キャラクターの頭上に吹き出しのCanvasを配置
            balloonCanvas.transform.position = transform.position + offset;

            // 吹き出しを常にカメラの方向に向ける（正面から見えるように回転）
            balloonCanvas.transform.LookAt(
                balloonCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// 吹き出しを表示し、タイピングアニメーションを開始する。
    /// 既にアニメーションが走っていたら停止してから再開する。
    /// </summary>
    private void ShowBalloon()
    {
        // 吹き出し表示状態をONに設定
        isDisplayed = true;

        // Canvasを表示
        balloonCanvas.enabled = true;

        // テキストを空にしてタイピング開始準備
        dialogueText.text = "";

        // タイピング完了フラグをリセット
        isFullyDisplayed = false;

        // 既にタイピングコルーチンが動いていたら停止
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // タイピングコルーチンを開始
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// 吹き出しに指定したメッセージを表示する。
    /// テキスト幅の調整やテキスト位置の微調整オフセットも可能。
    /// </summary>
    /// <param name="message">表示したいメッセージ文字列</param>
    /// <param name="optionalWidth">テキスト表示幅（-1で変更なし）</param>
    /// <param name="customOffset">テキストの表示位置オフセット（任意）</param>
    public void ShowMessage(string message, float optionalWidth = -1f, Vector2? customOffset = null)
    {
        // まずテキストのサイズと位置を初期値に戻す
        if (textRectTransform != null)
        {
            textRectTransform.sizeDelta = originalTextSize;
            textRectTransform.anchoredPosition = Vector2.zero;
        }

        // メッセージをセット
        fullText = message;

        // 幅が指定されていればテキスト幅を変更
        if (optionalWidth > 0 && textRectTransform != null)
        {
            textRectTransform.sizeDelta = new Vector2(optionalWidth, originalTextSize.y);
        }

        // 位置のオフセットが指定されていれば適用
        if (customOffset.HasValue && textRectTransform != null)
        {
            textRectTransform.anchoredPosition = customOffset.Value;
        }

        // 吹き出しを表示（タイピング開始）
        ShowBalloon();
    }

    /// <summary>
    /// 吹き出しを非表示にしてタイピングアニメーションを停止する。
    /// テキストサイズを元に戻す処理も行う。
    /// </summary>
    private void HideBalloon()
    {
        // 吹き出し表示状態をOFFに設定
        isDisplayed = false;

        // タイピングコルーチンが動いていれば停止
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // テキストを空にする
        dialogueText.text = "";

        // Canvasを非表示にする
        balloonCanvas.enabled = false;

        // タイピング完了フラグをリセット
        isFullyDisplayed = false;

        // テキストのサイズを元のサイズに戻す
        if (textRectTransform != null)
        {
            textRectTransform.sizeDelta = originalTextSize;
        }
    }

    /// <summary>
    /// 初期メッセージに戻して表示し直す。
    /// </summary>
    public void RestoreInitialMessage()
    {
        ShowMessage(DefaultInitialMessage);
    }

    /// <summary>
    /// タイピングアニメーションのコルーチン。
    /// メッセージを1文字ずつ順番に表示し、最後に完了フラグを立ててイベントを発行。
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator TypeText()
    {
        // 0文字目から全文字数まで1文字ずつ表示
        for (int i = 0; i <= fullText.Length; i++)
        {
            // i文字目まで切り出してテキスト表示
            dialogueText.text = fullText.Substring(0, i);

            // 次の文字まで少し待機
            yield return new WaitForSeconds(typingSpeed);
        }

        // 全文字表示完了
        isFullyDisplayed = true;

        // 完了イベントを発火（登録があれば呼ばれる）
        OnFullyDisplayed?.Invoke();
    }
}
