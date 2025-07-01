using UnityEngine;

/// <summary>
/// 1�̃X�|�[������\���N���X�i�G�E�A�C�e���Ȃǁj�B
/// ScriptableObject���Ŏg�p�����f�[�^�\���B
/// </summary>
[System.Serializable]
public class SpawnDataEntry
{
    public int id;

    /// <summary>
    /// ��ʁi��: "Enemy", "Item", "Coin" �Ȃǁj�B
    /// </summary>
    public string type;

    /// <summary>
    /// �g�p����v���n�u���iResources�t�H���_���œǂݍ��ޗp�j�B
    /// </summary>
    public string prefabName;

    /// <summary>
    /// �X�|�[���ʒu�i���[���h���W�j�B
    /// </summary>
    public Vector3 position;
}

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
