using UnityEngine;

/// <summary>
/// �����̃X�|�[�����iSpawnDataEntry�j��ێ�����ScriptableObject�B
/// �G�f�B�^��CSV�C���|�[�g�ȂǂŃf�[�^�Ǘ���e�Ղɂ���B
/// </summary>
[CreateAssetMenu(fileName = "SpawnDataSO", menuName = "Data/SpawnData")]
public class SpawnDataSO : ScriptableObject
{
    /// <summary>
    /// �X�|�[���f�[�^�̈ꗗ�i�V�[����ł̔z�u�ɗ��p�j�B
    /// </summary>
    public SpawnDataEntry[] entries;
}
