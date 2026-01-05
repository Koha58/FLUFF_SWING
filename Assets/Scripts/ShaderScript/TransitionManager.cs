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

    /// <summary>
    /// 遷移中フラグ（多重遷移防止）
    /// </summary>
    public static bool isTransitioning = false;

    [Header("トランジション用プレハブ (Canvasごと)")]
    [Tooltip("CloseTransitionとOpenTransitionコンポーネントを持つCanvasプレハブを指定します。")]
    public GameObject transitionCanvasPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// トランジション演出を再生し、指定されたシーンを非同期で読み込む。
    /// 
    /// 戻り値：
    /// true  = 遷移開始できた（SEを鳴らしてOK）
    /// false = 既に遷移中でブロックされた（SEを鳴らさない）
    /// </summary>
    public bool TryPlayTransitionAndLoadScene(string nextScene)
    {
        // 既に遷移中の場合は処理を中断
        if (isTransitioning)
        {
            Debug.LogWarning("既にシーン遷移中です。多重実行をブロックしました。");
            return false;
        }

        StartCoroutine(PlayTransitionSequence(nextScene));
        return true;
    }

    /// <summary>
    /// 旧API互換（他所で使っている場合のため残す）
    /// </summary>
    public void PlayTransitionAndLoadScene(string nextScene)
    {
        TryPlayTransitionAndLoadScene(nextScene);
    }

    private IEnumerator PlayTransitionSequence(string nextScene)
    {
        isTransitioning = true;

        // 1) Close
        GameObject closeCanvasInstance = Instantiate(transitionCanvasPrefab);
        CloseTransition close = closeCanvasInstance.GetComponentInChildren<CloseTransition>();

        if (close == null)
        {
            Debug.LogError("CloseTransitionコンポーネントが見つかりません。プレハブを確認してください。");
            Destroy(closeCanvasInstance);
            isTransitioning = false; // ★異常終了でもフラグ解除
            yield break;
        }

        yield return close.Play();

        // 2) Load
        Debug.Log($"Loading scene: {nextScene}");
        yield return SceneManager.LoadSceneAsync(nextScene);

        Destroy(closeCanvasInstance);

        // 3) Open
        GameObject openCanvasInstance = Instantiate(transitionCanvasPrefab);
        OpenTransition open = openCanvasInstance.GetComponentInChildren<OpenTransition>();

        if (open == null)
        {
            Debug.LogError("OpenTransitionコンポーネントが見つかりません。プレハブを確認してください。");
            Destroy(openCanvasInstance);
            isTransitioning = false; // ★異常終了でもフラグ解除
            yield break;
        }

        yield return open.Play();

        Destroy(openCanvasInstance);

        // 完了
        isTransitioning = false;
    }
}
