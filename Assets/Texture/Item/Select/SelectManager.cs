// �X�e�[�W�Z���N�g>SceneManager�Ŏg�p

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SelectManager : MonoBehaviour
{
    #region �C���X�y�N�^�[

    [Header("���ʐݒ�p�l��")]
    [SerializeField] private GameObject setPanel;

    [Header("�N���b�N���ݒ�")]
    [Tooltip("�X�e�[�W�I���E�ݒ�{�^�����������Ƃ��ɖ炷���ʉ�")]
    public AudioClip onClickSE;
    [Tooltip("�z�[���E����{�^�����������Ƃ��ɖ炷���ʉ�")]
    public AudioClip offClickSE;

    [Tooltip("�ڑ�����AudioMixer��SE�O���[�v")]
    public AudioMixerGroup seMixerGroup;

    private AudioSource audioSource;

    private string nextStageName;

    [Header("�e�X�e�[�W�̃��b�N")]   // �e�{�^����Lock�i�q�I�u�W�F�N�g�j�����X�g�ɐݒ�
    [SerializeField] private GameObject[] stageLocks;

    #endregion

    #region �p�l���̏����ݒ�E���b�N�̏�ԍX�V

    private void Start()
    {
        // --- �p�l���̏����ݒ� ---
        if (setPanel != null)
            setPanel.SetActive(false);

        // --- �X�e�[�W���b�N�̏�Ԃ��X�V ---
        UpdateStageLocks();
    }

    // ======== �e�X�e�[�W�̃��b�N��Ԃ��X�V =========
    private void UpdateStageLocks()
    {
        int clearedStage = PlayerPrefs.GetInt("ClearedStage", 0);

        for (int i = 0; i < stageLocks.Length; i++)
        {
            bool unlocked = i <= clearedStage; // �N���A�ς� + 1 �X�e�[�W�܂ŉ��
                                               // ����N�����X�e�[�W�P���
            if (stageLocks[i] != null)
                stageLocks[i].SetActive(!unlocked); // ��\�� = ����ς�
        }
    }

    #endregion

    #region �X�e�[�W�J��

    // ======== �X�e�[�W�J�� =========
    public void SelectStage(String StageName)
    {
        // --- ���b�N�`�F�b�N ---
        int stageIndex = GetStageIndex(StageName);
        if (stageIndex >= 0 && stageLocks[stageIndex] != null && stageLocks[stageIndex].activeSelf)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE(offClickSE);
            Debug.Log(StageName + " �̓��b�N���ł��B");
            return;
        }

        nextStageName = StageName;

        // AudioManager�o�R��SE���Đ��i���ꉹ�ʁj
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        // ���ʉ��̒����������҂��Ă���V�[���ړ�
        float delay = onClickSE != null ? onClickSE.length : 0.1f;
        Invoke(nameof(LoadNextScene), delay);
    }

    // �V�[���ړ��p
    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextStageName);
    }

    // ======== Stage�� �� Index�ϊ� ========
    private int GetStageIndex(string stageName)
    {
        // ��FStage1, Stage2,... ��O��
        if (stageName.StartsWith("Stage"))
        {
            if (int.TryParse(stageName.Replace("Stage", ""), out int num))
                return num - 1; // �z���0�n�܂�Ȃ̂Œ���
        }
        return -1;
    }

    // ========�^�C�g���ɖ߂�========
    public void TitleBack(String StageName)
    {
        nextStageName = StageName;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        float delay = offClickSE != null ? offClickSE.length : 0.1f;
        Invoke(nameof(LoadNextScene), delay);
    }

    // ======== �A�v���I�� =========
    public void OnApplicationQuit()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Application.Quit();
    }

    #endregion

    #region �p�l���̕\���E��\���؂�ւ�

    // ======== �p�l���\�� =========
    public void OnPanel()
    {
        if (setPanel == null) return;

        setPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        Debug.Log("�ݒ�p�l���\��");
    }

    // ======== �p�l����\�� =========
    public void OffPanel()
    {
        if (setPanel == null) return;

        setPanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Debug.Log("�ݒ�p�l����\��");
    }

    #endregion
}

//// �X�e�[�W�Z���N�g>SceneManager�Ŏg�p

//using System;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.Audio;

//public class SelectManager : MonoBehaviour
//{
//    [Header("���ʐݒ�p�l��")]
//    [SerializeField] private GameObject setPanel;

//    [Header("�N���b�N���ݒ�")]
//    [Tooltip("�X�e�[�W�I���E�ݒ�{�^�����������Ƃ��ɖ炷���ʉ�")]
//    public AudioClip onClickSE;
//    [Tooltip("�z�[���E����{�^�����������Ƃ��ɖ炷���ʉ�")]
//    public AudioClip offClickSE;

//    [Tooltip("�ڑ�����AudioMixer��SE�O���[�v")]
//    public AudioMixerGroup seMixerGroup;

//    private AudioSource audioSource;

//    private string nextStageName;

//    private void Start()
//    {
//        // --- �p�l���̏����ݒ� ---
//        if (setPanel != null)
//            setPanel.SetActive(false);
//    }

//    // ======== �X�e�[�W�J�� =========
//    public void SelectStage(String StageName)
//    {
//        nextStageName = StageName;

//        // AudioManager�o�R��SE���Đ��i���ꉹ�ʁj
//        if (AudioManager.Instance != null)
//            AudioManager.Instance.PlaySE(onClickSE);

//        // ���ʉ��̒����������҂��Ă���V�[���ړ�
//        float delay = onClickSE != null ? onClickSE.length : 0.2f;
//        Invoke(nameof(LoadNextScene), delay);
//    }

//    private void LoadNextScene()
//    {
//        SceneManager.LoadScene(nextStageName);
//    }

//    // ======== �p�l���\�� =========
//    public void OnPanel()
//    {
//        if (setPanel == null) return;

//        setPanel.SetActive(true);

//        if (AudioManager.Instance != null)
//            AudioManager.Instance.PlaySE(onClickSE);

//        Debug.Log("�ݒ�p�l���\��");
//    }

//    // ======== �p�l����\�� =========
//    public void OffPanel()
//    {
//        if (setPanel == null) return;

//        setPanel.SetActive(false);

//        if (AudioManager.Instance != null)
//            AudioManager.Instance.PlaySE(offClickSE);

//        Debug.Log("�ݒ�p�l����\��");
//    }

//    // ========�^�C�g���ɖ߂�========
//    public void TitleBack(String StageName)
//    {
//        nextStageName = StageName;

//        if (AudioManager.Instance != null)
//            AudioManager.Instance.PlaySE(offClickSE);

//        float delay = offClickSE != null ? offClickSE.length : 0.2f;
//        Invoke(nameof(LoadNextScene), delay);
//    }

//    // ======== �A�v���I�� =========
//    public void OnApplicationQuit()
//    {
//        if (AudioManager.Instance != null)
//            AudioManager.Instance.PlaySE(offClickSE);

//        Application.Quit();
//    }
//}
