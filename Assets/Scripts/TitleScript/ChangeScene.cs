using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    /// <summary>タイトルシーンの名前（SceneManagerで使用）</summary>
    private const string SelectSceneName = "SelectScene";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeScenes()
    {
        // TransitionManagerによるフェード付きのロードのみを実行
        if (TransitionManager.Instance != null)
        {
            // TransitionManagerにセレクトシーンへの遷移を依頼
            TransitionManager.Instance.PlayTransitionAndLoadScene(SelectSceneName);
        }
        else
        {
            // TransitionManagerが見つからない場合のフォールバック（緊急用）
            Debug.LogError("TransitionManagerが見つかりません。直接シーンロードします。");
            SceneManager.LoadScene(SelectSceneName);
        }
    }
}