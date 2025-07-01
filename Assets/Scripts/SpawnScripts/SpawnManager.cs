using UnityEngine;

/// <summary>
/// SpawnDataSO �Ɋ�Â��āA�v���n�u���V�[���ɃX�|�[������}�l�[�W���[�B
/// - �X�|�[������ ScriptableObject�ispawnData�j����ǂݍ��ށB
/// - Resources �t�H���_����v���n�u�����[�h���Ĕz�u�B
/// - �I�v�V������ spawnParent �Ɏq�Ƃ��Ĕz�u�\�B
/// </summary>
public class SpawnManager : MonoBehaviour
{
    /// <summary>
    /// �X�|�[���f�[�^�iScriptableObject�j�B
    /// CSV�Ȃǂ��玖�O�ɐ������ꂽ�f�[�^��ǂݍ��ށB
    /// </summary>
    public SpawnDataSO spawnData;

    /// <summary>
    /// ���������I�u�W�F�N�g�̐e�ƂȂ� Transform�B
    /// null �̏ꍇ�̓��[�g�ɐ��������B
    /// </summary>
    public Transform spawnParent;

    /// <summary>
    /// �Q�[���J�n���ɃX�|�[�����������s�B
    /// </summary>
    void Start()
    {
        foreach (var entry in spawnData.entries)
        {
            // Resources �t�H���_����v���n�u�����[�h
            GameObject prefab = Resources.Load<GameObject>(entry.prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab��������܂���: {entry.prefabName}");
                continue;
            }

            // �v���n�u���w�肳�ꂽ�ʒu�ɐ���
            GameObject obj = Instantiate(prefab, entry.position, Quaternion.identity, spawnParent);

            // TODO: �K�v�ɉ����� type �ɉ�����������������ǉ��\
            // ��: if (entry.type == "Enemy") { ... }
        }
    }
}
