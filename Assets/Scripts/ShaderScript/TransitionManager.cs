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

    public void PlayTransitionAndLoadScene(string nextScene)
    {
        StartCoroutine(PlayCloseAndLoad(nextScene));
    }

    IEnumerator PlayCloseAndLoad(string nextScene)
    {
        // --- CloseTransition 再生 ---
        GameObject canvas = Instantiate(transitionCanvasPrefab);
        CloseTransition close = canvas.GetComponentInChildren<CloseTransition>();
        yield return close.Play();   // アニメーション終了待ち

        // --- シーン読み込み ---
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(nextScene);

        // 閉じるCanvasは不要なので削除
        Destroy(canvas);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // --- OpenTransition 再生 ---
        GameObject canvas = Instantiate(transitionCanvasPrefab);
        OpenTransition open = canvas.GetComponentInChildren<OpenTransition>();
        StartCoroutine(DestroyAfter(open));
    }

    IEnumerator DestroyAfter(OpenTransition open)
    {
        yield return open.Play(); // アニメーション終了待ち
        Destroy(open.transform.parent.gameObject); // Canvas削除
    }
}
