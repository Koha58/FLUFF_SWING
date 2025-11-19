using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("トランジション用プレハブ (Canvasごと)")]
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

    // 外部から呼ぶ関数
    public void PlayTransitionAndLoadScene(string nextScene)
    {
        StartCoroutine(PlayTransitionSequence(nextScene));
    }

    private IEnumerator PlayTransitionSequence(string nextScene)
    {
        // --- Close演出 ---
        GameObject canvas = Instantiate(transitionCanvasPrefab);
        CloseTransition close = canvas.GetComponentInChildren<CloseTransition>();
        yield return close.Play();

        // --- シーン読み込み ---
        yield return SceneManager.LoadSceneAsync(nextScene);

        // Close演出のCanvasはここで削除
        Destroy(canvas);

        // --- Open演出 ---
        GameObject canvas2 = Instantiate(transitionCanvasPrefab);
        OpenTransition open = canvas2.GetComponentInChildren<OpenTransition>();
        yield return open.Play();

        Destroy(canvas2);
    }
}
