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
    // 吹き出し内のテキストコンポーネント
    private TextMeshProUGUI dialogueText;
    // 表示したい全文テキスト
    private string fullText = "ボタンをライフに交換する？";
    // タイピング速度（1文字あたりの待ち時間）
    private float typingSpeed = 0.05f;

    // プレイヤーのTransform（距離計算用）
    private Transform player;
    // 吹き出しを表示する最大距離
    private float showDistance = 3.0f;

    // キャラクター頭上へのオフセット位置
    private Vector3 offset = new Vector3(0, 1.5f, 0);
    // メインカメラ（吹き出しをカメラ方向に向けるため）
    private Camera mainCamera;

    // 吹き出し用Canvasの参照
    private Canvas balloonCanvas;

    // タイピングコルーチンの参照
    private Coroutine typingCoroutine;

    // 吹き出し表示中かどうかのフラグ
    private bool isDisplayed = false; 

    /// <summary>
    /// 初期化処理。
    /// 子オブジェクトからCanvasとTextMeshProUGUIを取得し、
    /// プレイヤーやカメラの参照も設定します。
    /// 吹き出しは初期非表示にします。
    /// </summary>
    void Start()
    {
        // 子オブジェクトからCanvasを取得
        balloonCanvas = GetComponentInChildren<Canvas>();
        // 子オブジェクトからTextMeshProUGUIを取得
        dialogueText = GetComponentInChildren<TextMeshProUGUI>();

        // Canvasが存在しない場合はエラー表示して処理を停止
        if (balloonCanvas == null)
        {
            Debug.LogError("吹き出しCanvasが見つかりません。子オブジェクトにCanvasを設置してください。");
            enabled = false;
            return;
        }
        // TextMeshProUGUIが存在しない場合はエラー表示して処理を停止
        if (dialogueText == null)
        {
            Debug.LogError("TextMeshProUGUIが見つかりません。子オブジェクトにTextMeshProUGUIを設置してください。");
            enabled = false;
            return;
        }

        // 初期状態は吹き出しを非表示にする
        balloonCanvas.enabled = false;

        // メインカメラが未設定ならシーン内のメインカメラを取得
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // プレイヤーオブジェクトをタグ「Player」から探してセット
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Playerオブジェクトが見つかりません。タグ「Player」を設定してください。");
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// 毎フレーム呼び出される更新処理。
    /// プレイヤーとの距離判定で吹き出しの表示/非表示を切り替え、
    /// 表示中は吹き出しの位置をキャラクター頭上に固定し、
    /// カメラ方向を向かせるBillboard処理を行います。
    /// </summary>
    void Update()
    {
        // プレイヤーとキャラクターの距離を計算
        float distance = Vector3.Distance(transform.position, player.position);

        // プレイヤーが近づいたら吹き出しを表示
        if (distance < showDistance && !isDisplayed)
        {
            ShowBalloon();
        }
        // プレイヤーが遠ざかったら吹き出しを非表示
        else if (distance >= showDistance && isDisplayed)
        {
            HideBalloon();
        }

        // 吹き出し表示中は毎フレーム吹き出しの位置と回転を調整
        if (isDisplayed)
        {
            // キャラクターの頭上（offset分上）に吹き出しCanvasを移動
            balloonCanvas.transform.position = transform.position + offset;

            // カメラに常に正面を向くように回転（Billboard効果）
            balloonCanvas.transform.LookAt(
                balloonCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// 吹き出しを表示し、テキストのタイピング表示を開始します。
    /// 既にタイピング中なら停止してから再スタートします。
    /// </summary>
    private void ShowBalloon()
    {
        isDisplayed = true;            // 表示中フラグを立てる
        balloonCanvas.enabled = true;  // Canvasを表示状態にする
        dialogueText.text = "";        // テキストを空に初期化

        // 既にタイピング中なら停止してから新たに開始
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // タイピングを開始
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// 吹き出しを非表示にし、タイピング表示を停止します。
    /// </summary>
    private void HideBalloon()
    {
        isDisplayed = false;          // 表示中フラグを倒す

        // タイピング中のコルーチンがあれば停止
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = "";        // テキストをクリア
        balloonCanvas.enabled = false; // Canvasを非表示にする
    }

    /// <summary>
    /// fullTextを1文字ずつタイピング風に表示するコルーチン。
    /// </summary>
    private IEnumerator TypeText()
    {
        // 0文字から全文字まで順に表示
        for (int i = 0; i <= fullText.Length; i++)
        {
            // 部分文字列を切り出してテキストにセット
            dialogueText.text = fullText.Substring(0, i);

            // typingSpeed秒待つ（1文字表示する時間）
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
