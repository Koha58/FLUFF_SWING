using UnityEngine;
using UnityEngine.SceneManagement;

public class ani : MonoBehaviour
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

    public void ChangeScene()
    {
        SceneManager.LoadScene("SelectScene");
        TransitionManager.Instance.PlayTransitionAndLoadScene(SelectSceneName);
        //TransitionManager.Instance.PlayTransitionAndLoadScene(SelectSceneName);


    }
}