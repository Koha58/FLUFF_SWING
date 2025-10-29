using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// �v���C���[�̎��͂� SpawnDataSO �̃f�[�^�Ɋ�Â���
/// �I�u�W�F�N�g�iEnemy / Coin�j���X�|�[�����A�v���C���[���痣�ꂽ��v�[���ɕԋp����Ǘ��N���X�B
/// �I�u�W�F�N�g�v�[���𗘗p���Č����I�ɃX�|�[���Ǘ����s���B
/// </summary>
public class SpawnManagerWithPool : MonoBehaviour
{
    /// <summary>�v���C���[�� Transform�B��������Ɏg�p</summary>
    public Transform player;

    /// <summary>�v���C���[����̃X�|�[���L���͈�</summary>
    private float spawnRange = 15f;

    /// <summary>���݂̃V�[���p�� SpawnDataSO</summary>
    private SpawnDataSO spawnData;

    /// <summary>�����ς݃I�u�W�F�N�g�� ID �ŊǗ�</summary>
    private readonly Dictionary<int, GameObject> spawnedObjects = new();

    /// <summary>
    /// Start: �V�[�����ɉ����� SpawnDataSO �����[�h
    /// </summary>
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Resources/SpawnData �t�H���_����V�[������ SpawnDataSO �����[�h
        spawnData = Resources.Load<SpawnDataSO>($"SpawnData/{sceneName}");

        if (spawnData == null)
        {
            Debug.LogError($"SpawnDataSO ��������܂���: Resources/SpawnData/{sceneName}");
        }
    }

    /// <summary>
    /// Update: �v���C���[�̈ʒu�ɉ����ăI�u�W�F�N�g�̃X�|�[���E������Ǘ�
    /// </summary>
    void Update()
    {
        // �v���C���[��f�[�^��������Ώ������Ȃ�
        if (player == null || spawnData == null) return;

        foreach (var entry in spawnData.entries)
        {
            // �v���C���[�Ƃ̋������v�Z
            float distance = Vector3.Distance(player.position, entry.position);

            if (distance <= spawnRange)
            {
                // �͈͓��ł܂���������Ă��Ȃ���΃X�|�[��
                if (!spawnedObjects.ContainsKey(entry.id))
                {
                    var obj = SpawnFromPool(entry);
                    if (obj != null) spawnedObjects.Add(entry.id, obj);
                }
            }
            else
            {
                // �͈͊O�̏ꍇ�A�����ς݂Ȃ�v�[���ɕԋp���ĊǗ�����폜
                if (spawnedObjects.TryGetValue(entry.id, out GameObject obj))
                {
                    ReturnToPool(obj, entry);
                    spawnedObjects.Remove(entry.id);
                }
            }
        }
    }

    /// <summary>
    /// SpawnDataEntry �ɉ����ăI�u�W�F�N�g���v�[������擾���ăX�|�[��
    /// </summary>
    /// <param name="entry">SpawnDataEntry</param>
    /// <returns>�������ꂽ GameObject�A�擾���s�Ȃ� null</returns>
    private GameObject SpawnFromPool(SpawnDataEntry entry)
    {
        string type = entry.type.ToLower();

        if (type == "enemy")
        {
            // Enemy �� EnemyPool ����擾
            string enemyName = System.IO.Path.GetFileName(entry.prefabName);
            var enemy = EnemyPool.Instance.GetFromPool(enemyName, entry.position);
            return enemy ? enemy.gameObject : null;
        }
        else if (type == "coin")
        {
            // Coin �� CoinPoolManager ����擾
            return CoinPoolManager.Instance.GetCoin(entry.position);
        }
        else
        {
            Debug.LogWarning($"Unknown spawn type: {entry.type}");
            return null;
        }
    }

    /// <summary>
    /// �I�u�W�F�N�g���v�[���ɕԋp
    /// </summary>
    /// <param name="obj">�ԋp���� GameObject</param>
    /// <param name="entry">�Ή����� SpawnDataEntry</param>
    private void ReturnToPool(GameObject obj, SpawnDataEntry entry)
    {
        if (entry.type == "enemy")
        {
            // Enemy �� EnemyController ���o�R���ăv�[���ɕԋp
            var enemyCtrl = obj.GetComponent<EnemyController>();
            if (enemyCtrl != null)
                EnemyPool.Instance.ReturnToPool(enemyCtrl);
            else
                Destroy(obj); // �擾�ł��Ȃ���Δj��
        }
        else if (entry.type == "coin")
        {
            // Coin �� CoinPoolManager �ɕԋp
            CoinPoolManager.Instance.ReturnCoin(obj);
        }
        else
        {
            // ���m�̃^�C�v�͔j��
            Destroy(obj);
        }
    }
}
