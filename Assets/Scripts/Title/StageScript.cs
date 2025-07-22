using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StageScript : MonoBehaviour
{

    //int型を変数StageTipSizeで宣言します。ここの数値は自動生成したいオブジェクトの端から端までの座標の大きさ
    const int StageTipSize = 20;
    //int型を変数currrentTipIndexで宣言します。
    int currrentTipIndex;
    //ターゲットキャラクターの指定ができるようにするよ
    public Transform character;
    //ステージチップの配列
    public GameObject[] stageTips;
    //自動生成する時に使う変数startTipIndex
    public int startTipIndex;
    //ステージ生成の先読み個数
    public int preInstantiate;
    //作ったステージチップの保持リスト
    public List<GameObject> generatedStageList = new List<GameObject>();

    void Start()
    {
        //初期化処理
        currrentTipIndex = startTipIndex - 1;
        UpdateStage(preInstantiate);
    }

    void Update()
    {
        //キャラクターの位置から現在のステージチップのインデックスを計算します。
        int charaPositionIndex = (int)(character.position.x / StageTipSize);
        //次のステージチップに入ったらステージの更新処理を行います。
        if (charaPositionIndex + preInstantiate > currrentTipIndex) 
        {
            UpdateStage(charaPositionIndex + preInstantiate);
        }
    }

    //指定のインデックスまでのステージチップを生成して、管理下に置く
    void UpdateStage(int toTipIndex)
    {
        if (toTipIndex <= currrentTipIndex) return;
        //指定のステージチップまで生成するよ
        for(int i = currrentTipIndex + 1; i <= toTipIndex; i++)
        {
            GameObject stageObject = GenerateStage(i);
            generatedStageList.Add(stageObject);
        }

        while (generatedStageList.Count > preInstantiate + 2) DestroyOldestStage();
        currrentTipIndex = toTipIndex;
    }

    //指定のインデックス位置にstageオブジェクトをランダムに生成
    GameObject GenerateStage(int tipIndex)
    {
        int nextStageTip = Random.Range(0, stageTips.Length);
        //x軸方向に無限生成するのでこの書き方をしている
        GameObject stageObject = (GameObject)Instantiate(stageTips[nextStageTip], new Vector3(tipIndex * StageTipSize, 0, 0), Quaternion.identity) as GameObject;
        return stageObject;
    }

    //一番古いステージを削除します
    void DestroyOldestStage()
    {
        GameObject OldStage = generatedStageList[0];
        generatedStageList.RemoveAt(0);
        Destroy(OldStage);
    }
}