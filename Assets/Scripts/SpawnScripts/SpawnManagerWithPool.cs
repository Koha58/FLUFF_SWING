using UnityEngine;

/// <summary>
/// �v���C���[�̈��͈͓������G��R�C�����X�|�[���i�v�[������擾�j����Ǘ��N���X�̗�B
/// SpawnDataSO�����ɐ����ʒu�������A�v���C���[�Ƃ̋��������͈͓��ɂȂ�����v�[������X�|�[���B
/// </summary>
public class SpawnManagerWithPool : MonoBehaviour
{
    public SpawnDataSO spawnData;       // �X�|�[���ʒu���ނ̃��X�g
    public Transform player;             // �v���C���[��Transform
    private float spawnRange = 15f;

    // �X�|�[�������I�u�W�F�N�g�Ǘ��p�i�C���X�^���X����ɕێ��j
    private readonly System.Collections.Generic.Dictionary<int, GameObject> spawnedObjects = new();

    void Update()
    {
        if (player == null) return;

        foreach (var entry in spawnData.entries)
        {
            float distance = Vector3.Distance(player.position, entry.position);
            Debug.Log($"entry.id={entry.id}, distance={distance}");

            if (distance <= spawnRange)
            {
                if (!spawnedObjects.ContainsKey(entry.id))
                {
                    GameObject obj = SpawnFromPool(entry);
                    if (obj != null)
                    {
                        spawnedObjects.Add(entry.id, obj);
                    }
                }
            }
            else
            {
                if (spawnedObjects.TryGetValue(entry.id, out GameObject obj))
                {
                    ReturnToPool(obj, entry);
                    spawnedObjects.Remove(entry.id);
                }
            }
        }

    }

    // �X�|�[�������iEnemy��Coin�����肵�ăv�[������擾�j
    private GameObject SpawnFromPool(SpawnDataEntry entry)
    {
        string type = entry.type.ToLower();

        if (type == "enemy")
        {
            // prefabName����t�@�C�������������o���i�Ō�̃X���b�V���ȍ~�j
            string enemyName = System.IO.Path.GetFileName(entry.prefabName);

            var enemy = EnemyPool.Instance.GetFromPool(enemyName, entry.position);
            return enemy ? enemy.gameObject : null;
        }
        else if (type == "coin")
        {
            return CoinPoolManager.Instance.GetCoin(entry.position);
        }
        else
        {
            Debug.LogWarning($"Unknown spawn type: {entry.type}");
            return null;
        }
    }


    // �v�[���ɖ߂�����
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
                Destroy(obj);
            }
        }
        else if (entry.type == "coin")
        {
            CoinPoolManager.Instance.ReturnCoin(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}
