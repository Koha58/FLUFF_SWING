using UnityEngine;

/// <summary>
/// �v���C���[�̈��͈͓������G��R�C�����X�|�[���i�v�[������擾�j����Ǘ��N���X�̗�B
/// SpawnDataSO�����ɐ����ʒu�������A�v���C���[�Ƃ̋��������͈͓��ɂȂ�����v�[������X�|�[���B
/// </summary>
public class SpawnManagerWithPool : MonoBehaviour
{
    public SpawnDataSO spawnData;       // �X�|�[���ʒu���ނ̃��X�g��������ScriptableObject
    public Transform player;             // �v���C���[��Transform�i�ʒu�擾�p�j
    private float spawnRange = 15f;      // �v���C���[���炱�̋����ȓ��ŃX�|�[���E�Ǘ����s��

    // �X�|�[�������I�u�W�F�N�g�Ǘ��p�iID���L�[�ɂ��ĊǗ��j
    private readonly System.Collections.Generic.Dictionary<int, GameObject> spawnedObjects = new();

    /// <summary>
    /// ���t���[���Ă΂�A�v���C���[�Ƃ̋����ɉ����ăX�|�[���E������Ǘ�����B
    /// </summary>
    void Update()
    {
        if (player == null) return; // �v���C���[���ݒ肳��Ă��Ȃ���Ώ������Ȃ�

        foreach (var entry in spawnData.entries)
        {
            // �v���C���[�ƃG���g���[�̈ʒu�̋������v�Z
            float distance = Vector3.Distance(player.position, entry.position);
            Debug.Log($"entry.id={entry.id}, distance={distance}");

            // ���͈͓��Ȃ�X�|�[���i�܂��X�|�[�����Ă��Ȃ���΁j
            if (distance <= spawnRange)
            {
                if (!spawnedObjects.ContainsKey(entry.id))
                {
                    GameObject obj = SpawnFromPool(entry); // �v�[������擾���Đ���
                    if (obj != null)
                    {
                        spawnedObjects.Add(entry.id, obj); // �Ǘ������ɓo�^
                    }
                }
            }
            else
            {
                // �͈͊O�Ȃ�X�|�[���ς݂Ȃ�v�[���ɖ߂��ĊǗ�����폜
                if (spawnedObjects.TryGetValue(entry.id, out GameObject obj))
                {
                    ReturnToPool(obj, entry);
                    spawnedObjects.Remove(entry.id);
                }
            }
        }
    }

    /// <summary>
    /// �X�|�[�������BSpawnDataEntry��type�ɂ��G���R�C�������ʂ��A
    /// �Ή�����I�u�W�F�N�g�v�[������擾���Ĕz�u����B
    /// </summary>
    /// <param name="entry">�X�|�[�����</param>
    /// <returns>�����i�擾�j�����Q�[���I�u�W�F�N�g</returns>
    private GameObject SpawnFromPool(SpawnDataEntry entry)
    {
        string type = entry.type.ToLower();

        if (type == "enemy")
        {
            // prefabName����t�@�C�������������o���i��: "Enemies/Goblin" �� "Goblin"�j
            string enemyName = System.IO.Path.GetFileName(entry.prefabName);

            // EnemyPool����擾���AEnemyController�̃Q�[���I�u�W�F�N�g��Ԃ�
            var enemy = EnemyPool.Instance.GetFromPool(enemyName, entry.position);
            return enemy ? enemy.gameObject : null;
        }
        else if (type == "coin")
        {
            // CoinPoolManager����R�C�����擾
            return CoinPoolManager.Instance.GetCoin(entry.position);
        }
        else
        {
            Debug.LogWarning($"Unknown spawn type: {entry.type}");
            return null;
        }
    }

    /// <summary>
    /// �����ς݃I�u�W�F�N�g���v�[���ɖ߂������B
    /// entry��type�ɉ����ēG���R�C�������ʂ��A�Ή������v�[����Return�������ĂԁB
    /// </summary>
    /// <param name="obj">�v�[���ɖ߂��Q�[���I�u�W�F�N�g</param>
    /// <param name="entry">�Ή�����X�|�[�����</param>
    private void ReturnToPool(GameObject obj, SpawnDataEntry entry)
    {
        if (entry.type == "enemy")
        {
            var enemyCtrl = obj.GetComponent<EnemyController>();
            if (enemyCtrl != null)
            {
                Debug.Log($"ReturnToPool�Ăяo��: id={entry.id}, enemy.name={enemyCtrl.name}");
                EnemyPool.Instance.ReturnToPool(enemyCtrl);
            }
            else
            {
                Debug.LogWarning("ReturnToPool: EnemyController��������܂���");
                Destroy(obj); // EnemyController��������Δj��
            }
        }
        else if (entry.type == "coin")
        {
            CoinPoolManager.Instance.ReturnCoin(obj);
        }
        else
        {
            // �s���ȃ^�C�v�͔j��
            Destroy(obj);
        }
    }
}
