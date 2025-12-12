using UnityEngine;

// Unityメニューからアセットを作成できるようにする
[CreateAssetMenu(fileName = "CreditData_", menuName = "Game Data/Credit Data SO", order = 100)]
public class CreditDataSO : ScriptableObject
{
    // クレジットの全行を格納する配列
    public CreditEntry[] entries;
}
