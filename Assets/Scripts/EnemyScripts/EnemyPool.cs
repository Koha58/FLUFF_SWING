using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �G�L�����N�^�[�̃I�u�W�F�N�g�v�[���Ǘ��N���X
/// �G�̎�ނ��ƂɃv�[����p�ӂ��A�g���܂킵�Ńp�t�H�[�}���X�����コ����
/// �V���O���g���Ƃ��ē��삵�A�ǂ�����ł��A�N�Z�X�\
/// </summary>
public class EnemyPool : MonoBehaviour
{
    /// <summary>
    /// �V���O���g���C���X�^���X
    /// </summary>
    public static EnemyPool Instance { get; private set; }

    /// <summary>
    /// �G�̎�ނ��Ƃ̃v���n�u�����i�[����N���X
    /// �v�[���������Ɏg�p���A�G���A�v���n�u�A�����v�[���T�C�Y��ݒ肷��
    /// </summary>
    [System.Serializable]
    public class EnemyPrefabEntry
    {
        public string enemyName;           // �G�̎�ޖ��i��F"Bird", "BlueRabbit"�j
        public EnemyController prefab;     // ���̓G�̃v���n�u
        public int initialPoolSize = 10;   // �����v�[�����i�ŏ��ɐ������đҋ@�����鐔�j
    }

    // �C���X�y�N�^�[�Őݒ肷��G�v���n�u�̃��X�g
    [SerializeField]
    private List<EnemyPrefabEntry> enemyPrefabs = new();

    // �G�̎�ޖ����L�[�ɂ����AEnemyController�̃L���[�i�v�[���j����
    private Dictionary<string, Queue<EnemyController>> poolDict = new();

    private void Awake()
    {
        // ���ł�Instance�����݂����炱�̃I�u�W�F�N�g��j�����A�B���Instance���ێ�����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // �e�G�̎�ނ��ƂɃv�[�����쐬���A���������C���X�^���X�𐶐����Ĕ�A�N�e�B�u�ɂ���
        foreach (var entry in enemyPrefabs)
        {
            Queue<EnemyController> queue = new Queue<EnemyController>();
            for (int i = 0; i < entry.initialPoolSize; i++)
            {
                var enemy = Instantiate(entry.prefab, transform);
                enemy.gameObject.SetActive(false);  // �����͔�A�N�e�B�u�őҋ@
                enemy.name = entry.enemyName;       // �G�̖��O��ݒ�i�Ǘ����₷�����邽�߁j
                queue.Enqueue(enemy);
            }
            poolDict[entry.enemyName] = queue;
        }
    }

    /// <summary>
    /// �w�肵���G�̎�ނ̃I�u�W�F�N�g���v�[������擾���A�w��ʒu�ɔz�u���ėL��������
    /// </summary>
    /// <param name="enemyName">�G�̎�ޖ�</param>
    /// <param name="position">�z�u�ʒu</param>
    /// <returns>�擾����EnemyController</returns>
    public EnemyController GetFromPool(string enemyName, Vector3 position)
    {
        // �v�[���Ɏw��̓G��ނ��Ȃ��ꍇ�̓G���[���o����null��Ԃ�
        if (!poolDict.ContainsKey(enemyName))
        {
            Debug.LogError($"EnemyPool��'{enemyName}'�̃v�[�������݂��܂���I");
            return null;
        }

        var queue = poolDict[enemyName];
        EnemyController enemy;

        if (queue.Count > 0)
        {
            // �L���[�ɋ󂫂�����΂�������擾
            enemy = queue.Dequeue();
        }
        else
        {
            // �L���[����̏ꍇ�͐V���ɐ�������
            var entry = enemyPrefabs.Find(e => e.enemyName == enemyName);
            if (entry == null)
            {
                Debug.LogError($"EnemyPrefabEntry��������܂���: {enemyName}");
                return null;
            }
            enemy = Instantiate(entry.prefab, transform);
            enemy.name = entry.enemyName;
        }

        // �ʒu��ݒ肵�L�������ĕԂ�
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    /// <summary>
    /// �G�I�u�W�F�N�g���v�[���ɖ߂��A��A�N�e�B�u�ɂ���
    /// </summary>
    /// <param name="enemy">�߂�EnemyController</param>
    public void ReturnToPool(EnemyController enemy)
    {
        // ��A�N�e�B�u�����ăv�[���ɖ߂�
        enemy.gameObject.SetActive(false);

        if (!poolDict.ContainsKey(enemy.name))
        {
            // �v�[���ɑ��݂��Ȃ���ނ̓G�Ȃ�j������i��O�Ή��j
            Debug.LogError($"EnemyPool��'{enemy.name}'�̃v�[��������܂���B");
            Destroy(enemy.gameObject);
            return;
        }

        // �L���[�ɖ߂�
        poolDict[enemy.name].Enqueue(enemy);
    }
}