// �X�e�[�W�Z���N�g>SceneManager�Ŏg�p

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Security.Cryptography.X509Certificates;

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

    private void Start()
    {
        // --- �p�l���̏����ݒ� ---
        if (setPanel != null)
            setPanel.SetActive(false);

        //// --- AudioSource�ݒ� ---
        //// AudioSource�������ǉ��܂��͎擾
        //audioSource = GetComponent<AudioSource>();
        //if (audioSource == null)
        //    audioSource = gameObject.AddComponent<AudioSource>();

        //// AudioMixerGroup��ݒ�iInspector����w��j
        //if (seMixerGroup != null)
        //    audioSource.outputAudioMixerGroup = seMixerGroup;
    }

    // ======== �X�e�[�W�J�� =========
    public void SelectStage(String StageName)
    {
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

    private void LoadStageAfterSE()
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

    //// ======== ���ʉ��Đ����ʊ֐� =========
    //private void PlaySE(AudioClip clip)
    //{
    //    if (clip != null)
    //        audioSource.PlayOneShot(clip);
    //    else
    //        Debug.LogWarning("SE���ݒ肳��Ă��܂���B");
    //}
}
