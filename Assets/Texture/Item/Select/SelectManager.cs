// �X�e�[�W�Z���N�g>SceneManager�Ŏg�p

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Security.Cryptography.X509Certificates;
// 10/23�ǉ���������
using System.Text.RegularExpressions;
// 10/23�ǉ������܂�

public class SelectManager : MonoBehaviour
{
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

    // 10/23�ǉ���������
    [Header("�e�X�e�[�W�̃��b�N")]
    [SerializeField] private GameObject[] stageLocks;
    // 10/23�ǉ������܂�

    private void Start()
    {
        // --- �p�l���̏����ݒ� ---
        if (setPanel != null)
            setPanel.SetActive(false);

        // 10/23�ǉ���������
        // --- �X�e�[�W���b�N�̏�Ԃ��X�V ---
        UpdateStageLocks();
        // 10/23�ǉ������܂�
    }

    // 10/23�ǉ���������
    // ======== �e�X�e�[�W�̃��b�N��Ԃ��X�V =========
    private void UpdateStageLocks()
    {
        int clearedStage = PlayerPrefs.GetInt("ClearedStage", 0);

        for (int i = 0; i < stageLocks.Length; i++)
        {
            bool unlocked = i <= clearedStage; // �N���A�ς� + 1 �X�e�[�W�܂ŉ��
            if (stageLocks[i] != null)
                stageLocks[i].SetActive(!unlocked); // ��\�� = ����ς�
        }
    }
    // 10/23�ǉ������܂�

    // ======== �X�e�[�W�J�� =========
    public void SelectStage(String StageName)
    {
        // 10/23�ǉ���������
        // --- ���b�N�`�F�b�N ---
        int stageIndex = GetStageIndex(StageName);
        if (stageIndex >= 0 && stageLocks[stageIndex] != null && stageLocks[stageIndex].activeSelf)
        {
            Debug.Log(StageName + " �̓��b�N���ł��B");
            return;
        }
        // 10/23�ǉ������܂�

        nextStageName = StageName;

        // AudioManager�o�R��SE���Đ��i���ꉹ�ʁj
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        // ���ʉ��̒����������҂��Ă���V�[���ړ�
        float delay = onClickSE != null ? onClickSE.length : 0.2f;
        Invoke(nameof(LoadNextScene), delay);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextStageName);
    }

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

    // ========�^�C�g���ɖ߂�========
    public void TitleBack(String StageName)
    {
        nextStageName = StageName;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        float delay = offClickSE != null ? offClickSE.length : 0.2f;
        Invoke(nameof(LoadNextScene), delay);
    }

    // ======== �A�v���I�� =========
    public void OnApplicationQuit()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        Application.Quit();
    }

    // 10/23�ǉ���������
    // ======== Stage�� �� Index�ϊ� ========
    private int GetStageIndex(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName)) return -1;

        // �啶���������������āuStage�v����n�܂邩�`�F�b�N
        if (!stageName.StartsWith("Stage", StringComparison.OrdinalIgnoreCase))
            return -1;

        // �uStage�v�ȍ~�ɂ���A�����������𒊏o
        Match m = Regex.Match(stageName, @"Stage(\d+)");    // �v�C��
        if (m.Success)
        {
            int num = int.Parse(m.Groups[1].Value);
            return num - 1; // �z���0�n�܂�
        }

        return -1;
    }
    // 10/23�ǉ������܂�
}


//// �X�e�[�W�Z���N�g>SceneManager�Ŏg�p

//using System;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.Audio;
//using System.Security.Cryptography.X509Certificates;

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
