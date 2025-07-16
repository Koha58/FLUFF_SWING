using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// �Q�[���̃��U���gUI�i�N���A�^�Q�[���I�[�o�[�j�𐧌䂷��N���X�B
/// </summary>
public enum GameResult { Clear, GameOver }

public class GameResultUIController : MonoBehaviour
{
    /// <summary>�V���O���g���C���X�^���X</summary>
    public static GameResultUIController Instance { get; private set; }

    [Header("����")]
    [SerializeField]
    private GraphicRaycaster raycaster; // UI�{�^���̗L��������pRaycaster

    [Header("���ʕ� UI")]
    [SerializeField]
    private GameObject clearUI; // �X�e�[�W�N���A���ɕ\������UI
    [SerializeField]
    private GameObject gameOverUI; // �Q�[���I�[�o�[���ɕ\������UI

    /// <summary>�^�C�g���V�[���̖��O�iSceneManager�Ŏg�p�j</summary>
    private const string TitleSceneName = "TitleScene";

    /// <summary>�ʏ�̃Q�[���i�s���x�iTime.timeScale = 1�j</summary>
    private readonly float normalTimeScale = 1.0f;

    /// <summary>
    /// �����������BUI��\�����ƃV���O���g���o�^���s���B
    /// </summary>
    private void Awake()
    {
        // �V���O���g���o�^
        Instance = this;

        // �Q�[���J�n���_�ł̓{�^���𖳌������AUI���\���ɂ��Ă���
        raycaster.enabled = false;
        clearUI.SetActive(false);
        gameOverUI.SetActive(false);
    }

    //========================================
    // �O�� API
    //========================================

    /// <summary>
    /// �w�肳�ꂽ���ʂɉ����ă��U���gUI��\������B
    /// </summary>
    /// <param name="result">�Q�[�����ʁiClear or GameOver�j</param>
    public void ShowResult(GameResult result)
    {
        // ���ʂɉ�����UI��\��
        clearUI.SetActive(result == GameResult.Clear);
        gameOverUI.SetActive(result == GameResult.GameOver);

        // �{�^�������L���ɂ���
        raycaster.enabled = true;

        // �f�o�b�O���O�i�J���p�j
        Debug.Log("ShowResult called. clearUI.activeSelf: " + clearUI.activeSelf);
        Debug.Log("clearUI position: " + clearUI.transform.position);
        Debug.Log("clearUI isInHierarchy: " + clearUI.activeInHierarchy);
    }

    //========================================
    // UI�{�^�� �n���h��
    //========================================

    /// <summary>
    /// ���g���C�{�^���������̏����B���݂̃V�[�����ēǂݍ��݂���B
    /// </summary>
    public void ClickRetry()
    {
        ResumeGameTime();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// �^�C�g����ʂɖ߂�{�^���������̏����B
    /// </summary>
    public void ClickQuitToTitle()
    {
        ResumeGameTime();
        SceneManager.LoadScene(TitleSceneName);
    }

    /// <summary>
    /// ���̃X�e�[�W�֐i�ރ{�^���������̏����i���݂͓���V�[�����ă��[�h�j�B
    /// </summary>
    public void ClickNext()
    {
        ResumeGameTime();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// �X�e�[�W�I����ʂɖ߂�{�^���������̏����i���݂̓^�C�g����ʁj�B
    /// </summary>
    public void ClickQuitToStageSelect()
    {
        ResumeGameTime();
        SceneManager.LoadScene(TitleSceneName);
    }

    //========================================
    // �������ʏ���
    //========================================

    /// <summary>
    /// Time.timeScale ��ʏ��1.0�ɖ߂��B
    /// </summary>
    private void ResumeGameTime()
    {
        Time.timeScale = normalTimeScale;
    }
}
