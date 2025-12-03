using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移時のトランジション演出（画面を閉じる/開く）を管理するシングルトンクラス。
/// シーンをまたいでインスタンスが破棄されないように設定される。
/// </summary>
public class TransitionManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static TransitionManager Instance;

    [Header("トランジション用プレハブ (Canvasごと)")]
    [Tooltip("CloseTransitionとOpenTransitionコンポーネントを持つCanvasプレハブを指定します。")]
    public GameObject transitionCanvasPrefab;

    /// <summary>
    /// 初期化処理：シングルトンパターンの確立とDontDestroyOnLoadの設定を行う。
    /// </summary>
    private void Awake()
    {
        // 既にインスタンスが存在しない場合
        if (Instance == null)
        {
            Instance = this;
            // シーンを跨いでオブジェクトが破棄されないように設定
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 既にインスタンスが存在する場合、重複するこのオブジェクトを破棄
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// トランジション演出を再生し、指定されたシーンを非同期で読み込む。
    /// 外部からシーン遷移を開始する際にこの関数を呼び出す。
    /// </summary>
    /// <param name="nextScene">読み込む次のシーン名</param>
    public void PlayTransitionAndLoadScene(string nextScene)
    {
        // コルーチンで一連のトランジションシーケンスを開始
        StartCoroutine(PlayTransitionSequence(nextScene));
    }

    /// <summary>
    /// シーン遷移とトランジション演出の一連の流れを処理するコルーチン。
    /// </summary>
    /// <param name="nextScene">読み込む次のシーン名</param>
    private IEnumerator PlayTransitionSequence(string nextScene)
    {
        // ----------------------------------------------------------------
        // 1. --- Close演出（画面を暗くする/閉じる） ---
        // ----------------------------------------------------------------

        // 演出用のCanvasインスタンスを生成
        GameObject closeCanvasInstance = Instantiate(transitionCanvasPrefab);
        // Canvas内のCloseTransitionコンポーネントを取得
        CloseTransition close = closeCanvasInstance.GetComponentInChildren<CloseTransition>();

        if (close == null)
        {
            Debug.LogError("CloseTransitionコンポーネントが見つかりません。プレハブを確認してください。");
            yield break;
        }

        // Close演出の完了を待つ (Close.Play()はIEnumeratorを返す想定)
        yield return close.Play();


        // ----------------------------------------------------------------
        // 2. --- シーン読み込み ---
        // ----------------------------------------------------------------

        // 非同期でシーンを読み込み、完了を待つ
        Debug.Log($"Loading scene: {nextScene}");
        yield return SceneManager.LoadSceneAsync(nextScene);

        // Close演出に使ったCanvasは不要になったため削除
        Destroy(closeCanvasInstance);


        // ----------------------------------------------------------------
        // 3. --- Open演出（画面を明るくする/開く） ---
        // ----------------------------------------------------------------

        // 演出用のCanvasインスタンスを再度生成
        GameObject openCanvasInstance = Instantiate(transitionCanvasPrefab);
        // Canvas内のOpenTransitionコンポーネントを取得
        OpenTransition open = openCanvasInstance.GetComponentInChildren<OpenTransition>();

        if (open == null)
        {
            Debug.LogError("OpenTransitionコンポーネントが見つかりません。プレハブを確認してください。");
            yield break;
        }

        // Open演出の完了を待つ (Open.Play()はIEnumeratorを返す想定)
        yield return open.Play();

        // Open演出に使ったCanvasも削除し、トランジション完了
        Destroy(openCanvasInstance);
    }
}