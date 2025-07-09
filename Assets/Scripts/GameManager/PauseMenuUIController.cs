using UnityEngine;

public class PauseMenuUIController : MonoBehaviour
{
    public static PauseMenuUIController Instance { get; private set; }

    [SerializeField] private GameObject pauseUI;

    private void Awake()
    {
        Instance = this;
       // pauseUI.SetActive(false);
    }

    public void OpenPauseMenu()
    {
        //pauseUI.SetActive(true);
        // 必要に応じて選択ボタンなども設定

        Debug.Log("OpenPause");
    }

    public void ClosePauseMenu()
    {
        //pauseUI.SetActive(false);
    }

    // Resume ボタンや Quit ボタンはここで実装
}
