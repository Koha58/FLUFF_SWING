using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// キャラクターの頭上に吹き出しテキストを表示するコントローラークラス
/// プレイヤーとの距離に応じて吹き出しの表示・非表示を切り替え、
/// テキストはタイピング風に1文字ずつ表示します。
/// </summary>
public class BalloonTextController : MonoBehaviour
{
    /// <summary>吹き出し内のテキスト表示コンポーネント</summary>
    private TextMeshProUGUI dialogueText;

    /// <summary>表示する全文テキスト</summary>
    private string fullText = "ボタンをライフに交換する？";

    /// <summary>1文字表示の間隔（秒）</summary>
    private float typingSpeed = 0.05f;

    /// <summary>プレイヤーのTransform（位置取得用）</summary>
    private Transform player;

    /// <summary>吹き出し表示の最大距離（3メートル）</summary>
    private float showDistance = 3.0f;

    /// <summary>吹き出しの表示位置オフセット（頭上あたり）</summary>
    private Vector3 offset = new Vector3(0, 1.5f, 0);

    /// <summary>メインカメラ参照（吹き出しの回転制御用）</summary>
    private Camera mainCamera;

    /// <summary>吹き出しCanvasコンポーネント</summary>
    private Canvas balloonCanvas;

    /// <summary>テキストタイピング処理のCoroutine管理</summary>
    private Coroutine typingCoroutine;

    /// <summary>吹き出しが現在表示中かどうかのフラグ</summary>
    private bool isDisplayed = false;

    /// <summary>テキストが全文表示されたかどうかのフラグ</summary>
    private bool isFullyDisplayed = false;

    /// <summary>全文表示されたかどうかの公開プロパティ</summary>
    public bool IsFullyDisplayed => isFullyDisplayed;

    /// <summary>テキスト全文表示完了時に呼ばれるイベント</summary>
    public event System.Action OnFullyDisplayed;


    void Start()
    {
        // Canvasコンポーネントを取得（子オブジェクトから）
        balloonCanvas = GetComponentInChildren<Canvas>();

        // TextMeshProUGUIコンポーネントを取得（子オブジェクトから）
        dialogueText = GetComponentInChildren<TextMeshProUGUI>();

        // Canvasが存在しない場合はエラーを出して処理停止
        if (balloonCanvas == null)
        {
            Debug.LogError("吹き出しCanvasが見つかりません。");
            enabled = false;
            return;
        }

        // TextMeshProUGUIが存在しない場合はエラーを出して処理停止
        if (dialogueText == null)
        {
            Debug.LogError("TextMeshProUGUIが見つかりません。");
            enabled = false;
            return;
        }

        // 最初は吹き出しを非表示にする
        balloonCanvas.enabled = false;

        // メインカメラの参照を取得
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // タグ「Player」が付いたオブジェクトのTransformを取得
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Playerオブジェクトが見つかりません。タグ「Player」を設定してください。");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        // プレイヤーとこのオブジェクトの距離を計算
        float distance = Vector3.Distance(transform.position, player.position);

        // 距離内でまだ表示していなければ表示開始
        if (distance < showDistance && !isDisplayed)
        {
            ShowBalloon();
        }
        // 距離外になり表示中なら非表示にする
        else if (distance >= showDistance && isDisplayed)
        {
            HideBalloon();
        }

        // 表示中は吹き出しの位置を頭上に設定し、
        // カメラの方向を向くように回転を調整する
        if (isDisplayed)
        {
            balloonCanvas.transform.position = transform.position + offset;

            // カメラ方向に常に正面を向くように回転
            balloonCanvas.transform.LookAt(
                balloonCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// 吹き出しを表示し、テキストのタイピングを開始する
    /// </summary>
    private void ShowBalloon()
    {
        isDisplayed = true;
        balloonCanvas.enabled = true;
        dialogueText.text = "";
        isFullyDisplayed = false;

        // もし前回のタイピング処理があれば停止する
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 新しくタイピング処理を開始
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// 吹き出しを非表示にし、タイピング処理を停止する
    /// </summary>
    private void HideBalloon()
    {
        isDisplayed = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = "";
        balloonCanvas.enabled = false;
        isFullyDisplayed = false;
    }

    /// <summary>
    /// テキストを1文字ずつ表示していくコルーチン
    /// 表示完了後、OnFullyDisplayedイベントを呼び出す
    /// </summary>
    private IEnumerator TypeText()
    {
        // 0文字目から全文字分ループ
        for (int i = 0; i <= fullText.Length; i++)
        {
            // 0文字目からi文字目までを切り出して表示
            dialogueText.text = fullText.Substring(0, i);

            // 指定秒数待機
            yield return new WaitForSeconds(typingSpeed);
        }

        // 全文表示完了フラグを立てる
        isFullyDisplayed = true;

        // 表示完了の通知イベントを呼び出す（nullチェック付き）
        OnFullyDisplayed?.Invoke();
    }

}
