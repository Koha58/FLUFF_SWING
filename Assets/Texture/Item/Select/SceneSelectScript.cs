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

    // �������Ă���X�e�[�W�͑I��s�ɂ���
    // �N���A���ɃN���A����p�̕ϐ��ɉ��Z���Ă���
    // �����ɉ����Č��������A�I���\�ɂ���
}
