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
            switch (entry.type.ToLower())
            {
                case "enemy":
                    string enemyName = System.IO.Path.GetFileName(entry.prefabName);
                    var enemy = EnemyPool.Instance.GetFromPool(enemyName, entry.position);
                    if (enemy == null)
                    {
                        Debug.LogWarning($"Enemy�擾���s: {entry.prefabName}");
                    }
                    break;

                case "coin":
                    var coin = CoinPoolManager.Instance.GetCoin(entry.position);
                    if (coin == null)
                    {
                        Debug.LogWarning($"Coin�擾���s: {entry.prefabName}");
                    }
                    break;

                default:
                    Debug.LogWarning($"���Ή��̃^�C�v: {entry.type}");
                    break;
            }
        }
    }

}
