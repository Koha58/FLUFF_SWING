using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StageScript : MonoBehaviour
{

    //int�^��ϐ�StageTipSize�Ő錾���܂��B�����̐��l�͎��������������I�u�W�F�N�g�̒[����[�܂ł̍��W�̑傫��
    const int StageTipSize = 20;
    //int�^��ϐ�currrentTipIndex�Ő錾���܂��B
    int currrentTipIndex;
    //�^�[�Q�b�g�L�����N�^�[�̎w�肪�ł���悤�ɂ����
    public Transform character;
    //�X�e�[�W�`�b�v�̔z��
    public GameObject[] stageTips;
    //�����������鎞�Ɏg���ϐ�startTipIndex
    public int startTipIndex;
    //�X�e�[�W�����̐�ǂ݌�
    public int preInstantiate;
    //������X�e�[�W�`�b�v�̕ێ����X�g
    public List<GameObject> generatedStageList = new List<GameObject>();

    void Start()
    {
        //����������
        currrentTipIndex = startTipIndex - 1;
        UpdateStage(preInstantiate);
    }

    void Update()
    {
        //�L�����N�^�[�̈ʒu���猻�݂̃X�e�[�W�`�b�v�̃C���f�b�N�X���v�Z���܂��B
        int charaPositionIndex = (int)(character.position.x / StageTipSize);
        //���̃X�e�[�W�`�b�v�ɓ�������X�e�[�W�̍X�V�������s���܂��B
        if (charaPositionIndex + preInstantiate > currrentTipIndex) 
        {
            UpdateStage(charaPositionIndex + preInstantiate);
        }
    }

    //�w��̃C���f�b�N�X�܂ł̃X�e�[�W�`�b�v�𐶐����āA�Ǘ����ɒu��
    void UpdateStage(int toTipIndex)
    {
        if (toTipIndex <= currrentTipIndex) return;
        //�w��̃X�e�[�W�`�b�v�܂Ő��������
        for(int i = currrentTipIndex + 1; i <= toTipIndex; i++)
        {
            GameObject stageObject = GenerateStage(i);
            generatedStageList.Add(stageObject);
        }

        while (generatedStageList.Count > preInstantiate + 2) DestroyOldestStage();
        currrentTipIndex = toTipIndex;
    }

    //�w��̃C���f�b�N�X�ʒu��stage�I�u�W�F�N�g�������_���ɐ���
    GameObject GenerateStage(int tipIndex)
    {
        int nextStageTip = Random.Range(0, stageTips.Length);
        //x�������ɖ�����������̂ł��̏����������Ă���
        GameObject stageObject = (GameObject)Instantiate(stageTips[nextStageTip], new Vector3(tipIndex * StageTipSize, 0, 0), Quaternion.identity) as GameObject;
        return stageObject;
    }

    //��ԌÂ��X�e�[�W���폜���܂�
    void DestroyOldestStage()
    {
        GameObject OldStage = generatedStageList[0];
        generatedStageList.RemoveAt(0);
        Destroy(OldStage);
    }
}