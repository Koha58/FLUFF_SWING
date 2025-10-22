using UnityEngine;

/// <summary>
/// SpawnDataSO �Ɋ�Â��āA�v���n�u���V�[���ɃX�|�[������}�l�[�W���[�B
/// - �X�|�[������ Resources �t�H���_����ǂݍ��ށB
/// - �v���n�u�� Resources ���烍�[�h���Ĕz�u�B
/// </summary>
public class SpawnManager : MonoBehaviour
{
    /// <summary>
    /// ���������I�u�W�F�N�g�̐e�ƂȂ� Transform�B
    /// null �̏ꍇ�̓V�[���̃��[�g�ɐ��������B
    /// </summary>
    public Transform spawnParent;

    /// <summary>
    /// �Q�[���J�n���Ɉ�x�����Ă΂�郁�\�b�h�B
    /// Resources �t�H���_���� SpawnDataSO ��ǂݍ��݁A
    /// �����ɋL�^���ꂽ�X�|�[�����Ɋ�Â��ăI�u�W�F�N�g�𐶐�����B
    /// </summary>
    void Start()
    {
        // Resources/SpawnData �t�H���_���� SpawnDataSO ��ǂݍ���
        var spawnData = Resources.Load<SpawnDataSO>("SpawnData/SpawnDataSO");

        // �f�[�^�����݂��Ȃ��ꍇ�̓G���[���o�͂��ď����𒆒f
        if (spawnData == null)
        {
            Debug.LogError("SpawnDataSO��Resources/SpawnData�ɑ��݂��܂���I");
            return;
        }

        // �X�|�[���f�[�^���̊e�G���g���ɑ΂��ď������s��
        foreach (var entry in spawnData.entries)
        {
            // �G���g���̎�ށitype�j�ɉ����ď����𕪊�
            switch (entry.type.ToLower())
            {
                case "enemy":
                    // prefabName ����t�@�C�����������o�i��F"Enemies/Goblin" �� "Goblin"�j
                    string enemyName = System.IO.Path.GetFileName(entry.prefabName);
                    // EnemyPool����Y���̓G���v�[������擾���A�w��ʒu�ɐ���
                    var enemy = EnemyPool.Instance.GetFromPool(enemyName, entry.position);
                    enemy.ResetEnemy();
                    if (enemy == null)
                    {
                        // �擾�Ɏ��s�����ꍇ�͌x�����O���o��
                        Debug.LogWarning($"Enemy�擾���s: {entry.prefabName}");
                    }
                    else if (spawnParent != null)
                    {
                        // �eTransform���ݒ肳��Ă���ꍇ�͂����Ɏq�Ƃ��Ĕz�u
                        enemy.transform.parent = spawnParent;
                    }
                    break;

                case "coin":
                    // CoinPoolManager����R�C�����v�[������擾���A�w��ʒu�ɐ���
                    var coin = CoinPoolManager.Instance.GetCoin(entry.position);
                    if (coin == null)
                    {
                        // �擾�Ɏ��s�����ꍇ�͌x�����O���o��
                        Debug.LogWarning($"Coin�擾���s: {entry.prefabName}");
                    }
                    else if (spawnParent != null)
                    {
                        // �eTransform���ݒ肳��Ă���ꍇ�͂����Ɏq�Ƃ��Ĕz�u
                        coin.transform.parent = spawnParent;
                    }
                    break;

                default:
                    // ���Ή��̃^�C�v�̏ꍇ�͌x�����O���o��
                    Debug.LogWarning($"���Ή��̃^�C�v: {entry.type}");
                    break;
            }
        }
    }
}
