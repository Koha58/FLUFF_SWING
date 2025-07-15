using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUIController : MonoBehaviour
{
    public static PauseMenuUIController Instance { get; private set; }

    [SerializeField] private GameObject pauseUI;

    // êe Canvas Ç…Ç†ÇÈ GraphicRaycaster ÇéwíË
    [SerializeField] private GraphicRaycaster pauseRaycaster;

    private void Awake()
    {
        Instance = this;

        pauseUI.SetActive(false);

        if (pauseRaycaster != null)
            pauseRaycaster.enabled = false;
    }

    public void OpenPauseMenu()
    {
        pauseUI.SetActive(true);

        if (pauseRaycaster != null)
            pauseRaycaster.enabled = true;

        Debug.Log("OpenPause");
    }

    public void ClosePauseMenu()
    {
        pauseUI.SetActive(false);

        if (pauseRaycaster != null)
            pauseRaycaster.enabled = false;
    }

    // === UI Buttons ===
    public void ClickResume()
    {
        GameManager.Instance.ResumeFromPauseMenu();
        ClosePauseMenu();
    }

    public void ClickQuitToTitle()
    {
        // SceneManager.LoadScene("Title");
    }
}
