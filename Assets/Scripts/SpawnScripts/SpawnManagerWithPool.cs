using UnityEngine;

/// <summary>
/// �v���C���[�̈��͈͓������G��R�C�����X�|�[���i�v�[������擾�j����Ǘ��N���X�B
/// - Resources ���� SpawnDataSO ��ǂݍ���Ŏg�p�B
/// - �v���C���[�̈ʒu�ɉ����āA�͈͓��̂��̂����X�|�[�����A
///   �͈͊O�ɂȂ�����v�[���ɖ߂��i�������͔j���j�B
/// </summary>
public class SpawnManagerWithPool : MonoBehaviour
{
    /// <summary>
    /// Resources����SpawnDataSO�̃p�X�i�g���q�Ȃ��j�B
    /// </summary>
    public string spawnDataResourcePath = "SpawnData/SpawnDataSO";

    /// <summary>
    /// �v���C���[��Transform�B�ʒu�擾�Ɏg�p�B
    /// </summary>
    public Transform player;

    /// <summary>
    /// �v���C���[����̋����B���͈͓̔��̃X�|�[�����̂݊Ǘ��E�����Ώۂɂ���B
    /// </summary>
    private float spawnRange = 15f;

    /// <summary>
    /// ���s����Resources����ǂݍ��񂾃X�|�[���f�[�^�B
    /// </summary>
    private SpawnDataSO spawnData;

    /// <summary>
    /// �X�|�[���ς݂̃I�u�W�F�N�g�Ǘ��p�B
    /// ID���L�[�ɂ��A���݃X�|�[������GameObject��ێ�����B
    /// </summary>
    private readonly System.Collections.Generic.Dictionary<int, GameObject> spawnedObjects = new();

    /// <summary>
    /// �N�����Ɉ�x�Ă΂�ASpawnDataSO��Resources����ǂݍ��ށB
    /// </summary>
    void Start()
    {
        spawnData = Resources.Load<SpawnDataSO>(spawnDataResourcePath);

        if (spawnData == null)
        {
            Debug.LogError($"SpawnDataSO��Resources����ǂݍ��߂܂���: {spawnDataResourcePath}");
        }
    }

    /// <summary>
    /// ���t���[���Ă΂�A�v���C���[�Ƃ̋����ɉ����ăX�|�[���E������Ǘ�����B
    /// �͈͓��Ȃ�X�|�[�����A�͈͊O�Ȃ�v�[���ɕԋp�܂��͔j���B
    /// </summary>
    void Update()
    {
        // �v���C���[�E�f�[�^�������Ă��Ȃ���Ή������Ȃ�
        if (player == null || spawnData == null) return;

        foreach (var entry in spawnData.entries)
        {
            // �v���C���[����X�|�[���ʒu�܂ł̋������v�Z
            float distance = Vector3.Distance(player.position, entry.position);

            // �������͈͓��Ȃ�X�|�[���Ǘ�
            if (distance <= spawnRange)
            {
                // �܂��X�|�[�����Ă��Ȃ���΃X�|�[������
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
                // �͈͊O�Ȃ�X�|�[���ς݂Ȃ�������
                if (spawnedObjects.TryGetValue(entry.id, out GameObject obj))
                {
                    ReturnToPool(obj, entry);
                    spawnedObjects.Remove(entry.id);
                }
            }
        }
    }

    /// <summary>
    /// SpawnDataEntry�̓��e�Ɋ�Â��āA�v�[������I�u�W�F�N�g���擾���ăX�|�[������B
    /// </summary>
    /// <param name="entry">�X�|�[���f�[�^�̃G���g��</param>
    /// <returns>�X�|�[�����ꂽGameObject�i���s����null�j</returns>
    private GameObject SpawnFromPool(SpawnDataEntry entry)
    {
        string type = entry.type.ToLower();

        if (type == "enemy")
        {
            // prefabName����t�@�C�����������o
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

    /// <summary>
    /// �X�|�[���ς݃I�u�W�F�N�g���v�[���ɖ߂��i�������͔j���j���鏈���B
    /// </summary>
    /// <param name="obj">�Ώۂ�GameObject</param>
    /// <param name="entry">�Ή�����SpawnDataEntry</param>
    private void ReturnToPool(GameObject obj, SpawnDataEntry entry)
    {
        if (entry.type == "enemy")
        {
            var enemyCtrl = obj.GetComponent<EnemyController>();
            if (enemyCtrl != null)
            {
                // Enemy�̏ꍇ��EnemyPool�ɕԋp
                EnemyPool.Instance.ReturnToPool(enemyCtrl);
            }
            else
            {
                // �R���|�[�l���g���Ȃ���Δj��
                Destroy(obj);
            }
        }
        else if (entry.type == "coin")
        {
            // Coin�̏ꍇ��CoinPoolManager�ɕԋp
            CoinPoolManager.Instance.ReturnCoin(obj);
        }
        else
        {
            // ���Ή��^�C�v�͔j��
            Destroy(obj);
        }
    }
}
