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
        // �K�v�ɉ����đI���{�^���Ȃǂ��ݒ�

        Debug.Log("OpenPause");
    }

    public void ClosePauseMenu()
    {
        //pauseUI.SetActive(false);
    }

    // Resume �{�^���� Quit �{�^���͂����Ŏ���
}
