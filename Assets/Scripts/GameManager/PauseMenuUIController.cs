using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// �|�[�Y���j���[UI�𐧌䂷��N���X�B
/// �|�[�Y����UI�\���E��\����{�^������𐧌䂷��B
/// </summary>
public class PauseMenuUIController : MonoBehaviour
{
    /// <summary>�V���O���g���C���X�^���X</summary>
    public static PauseMenuUIController Instance { get; private set; }

    [Header("UI�{��")]
    [SerializeField]
    private GameObject pauseUI; // �|�[�Y���j���[��UI�I�u�W�F�N�g

    [Header("���͐���")]
    [SerializeField]
    private GraphicRaycaster pauseRaycaster; // �|�[�Y����UI����L�����pRaycaster

    /// <summary>�Z���N�g�V�[���̖��O�iSceneManager�Ŏg�p�j</summary>
    private const string SelectSceneName = "SelectScene";

    /// <summary>�^�C�g���V�[���̖��O�iSceneManager�Ŏg�p�j</summary>
    private const string TitleSceneName = "SubTitleScene";

    /// <summary>
    /// �����������BUI���\���ɂ��A�{�^�����������������B
    /// </summary>
    private void Awake()
    {
        // �V���O���g���C���X�^���X��o�^
        Instance = this;

        // ������Ԃł̓|�[�YUI���\���ɂ��A������s�ɂ���
        pauseUI.SetActive(false);
        if (pauseRaycaster != null)
            pauseRaycaster.enabled = false;
    }

    //========================================
    // �O������Ăяo�� UI �J�� API
    //========================================

    /// <summary>
    /// �|�[�Y���j���[���J���BUI�\���{���͗L�����B
    /// </summary>
    public void OpenPauseMenu()
    {
        pauseUI.SetActive(true);

        if (pauseRaycaster != null)
            pauseRaycaster.enabled = true;

        Debug.Log("OpenPause");
    }

    /// <summary>
    /// �|�[�Y���j���[�����BUI��\���{���͖������B
    /// </summary>
    public void ClosePauseMenu()
    {
        pauseUI.SetActive(false);

        if (pauseRaycaster != null)
            pauseRaycaster.enabled = false;
    }

    //========================================
    // === UI �{�^���n���h�� ===
    //========================================

    /// <summary>
    /// Resume�{�^���F�Q�[�����ĊJ���ă|�[�Y���j���[�����B
    /// </summary>
    public void ClickResume()
    {
        GameManager.Instance.ResumeFromPauseMenu();
        ClosePauseMenu();
    }

    /// <summary>
    /// �X�e�[�W�I����ʂɖ߂�{�^���i���݂̓^�C�g����ʂƓ���j�B
    /// </summary>
    public void ClickQuitToStageSelect()
    {
        ResumeAndLoadScene(SelectSceneName);
    }

    /// <summary>
    /// �^�C�g����ʂɖ߂�{�^���B
    /// </summary>
    public void ClickQuitToTitle()
    {
        ResumeAndLoadScene(TitleSceneName);
    }

    //========================================
    // �������ʏ���
    //========================================

    /// <summary>
    /// �Q�[�����ĊJ���Ďw��V�[����ǂݍ��ށB
    /// </summary>
    /// <param name="sceneName">�J�ڐ�̃V�[����</param>
    private void ResumeAndLoadScene(string sceneName)
    {
        GameManager.Instance.ResumeFromPauseMenu();
        SceneManager.LoadScene(sceneName);
    }
}
