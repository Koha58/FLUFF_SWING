// �X�e�[�W�Z���N�g>SceneManager�Ŏg�p

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SceneSelectScript : MonoBehaviour
{
    [Header("�N���b�N��")]
    [Tooltip("�X�e�[�W�I���E�ݒ�{�^�����������Ƃ��ɖ炷���ʉ�")]
    public AudioClip onClickSE;
    [Tooltip("�z�[���E����{�^�����������Ƃ��ɖ炷���ʉ�")]
    public AudioClip offClickSE;

    [Tooltip("�ڑ�����AudioMixer��SE�O���[�v�i�C�Ӂj")]
    public AudioMixerGroup seMixerGroup;

    private AudioSource audioSource;

    void Start()
    {
        // AudioSource�������ǉ��܂��͎擾
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // AudioMixerGroup��ݒ�iInspector����w��j
        if (seMixerGroup != null)
            audioSource.outputAudioMixerGroup = seMixerGroup;
    }

    public void SelectStage(String StageName)
    {
        OnClickSE();
        SceneManager.LoadScene(StageName);
    }

    private void OnClickSE()
    {
        if (onClickSE != null)
            audioSource.PlayOneShot(onClickSE);
        else
            Debug.LogWarning("�N���b�N�����ݒ肳��Ă��܂���B");
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }

    // �������Ă���X�e�[�W�͑I��s�ɂ���
    // �N���A���ɃN���A����p�̕ϐ��ɉ��Z���Ă���
    // �����ɉ����Č��������A�I���\�ɂ���

    // ���ʐݒ肪�ł���悤�ɂ���
    // �{�^����������SE�ǉ�
}
