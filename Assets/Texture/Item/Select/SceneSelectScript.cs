using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneSelectScript : MonoBehaviour
{
    public void SelectStage(String StageName)
    {
        SceneManager.LoadScene(StageName);
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }

    // 鍵がついているステージは選択不可にする
    // クリア時にクリア判定用の変数に加算していく
    // →数に応じて鍵を解除、選択可能にする
}
