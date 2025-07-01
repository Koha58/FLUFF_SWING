using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �R�C���̃I�u�W�F�N�g�v�[�����Ǘ�����V���O���g���N���X�B
/// �K�v�ȂƂ��ɃR�C�����擾���A�s�v�ɂȂ�����ė��p�\�ȏ�ԂŃv�[���ɖ߂��B
/// </summary>
public class CoinPoolManager : MonoBehaviour
{
    /// <summary>
    /// �V���O���g���C���X�^���X�ւ̃A�N�Z�X�p�B
    /// </summary>
    public static CoinPoolManager Instance { get; private set; }

    /// <summary>
    /// �v�[������R�C���̃v���n�u�B
    /// </summary>
    [SerializeField] private GameObject coinPrefab;

    /// <summary>
    /// �N�����ɐ�������R�C���̏������B
    /// </summary>
    [SerializeField] private int initialPoolSize = 20;

    /// <summary>
    /// ��A�N�e�B�u�ȃR�C����ێ�����L���[�B
    /// </summary>
    private Queue<GameObject> pool = new Queue<GameObject>();

    /// <summary>
    /// �V���O���g���̏������Ə����v�[�������B
    /// </summary>
    private void Awake()
    {
        // �V���O���g���̏d���h�~
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // �����v�[���̐���
        for (int i = 0; i < initialPoolSize; i++)
        {
            var coin = Instantiate(coinPrefab);
            coin.SetActive(false);   // �ŏ��͔�A�N�e�B�u
            pool.Enqueue(coin);      // �v�[���ɒǉ�
        }
    }

    /// <summary>
    /// �v�[������R�C�����擾���āA�w��ʒu�ɔz�u�E�A�N�e�B�u������B
    /// </summary>
    /// <param name="position">�\�����������[���h���W</param>
    /// <returns>�g�p�\�ȃR�C����GameObject</returns>
    public GameObject GetCoin(Vector3 position)
    {
        // �v�[���Ɏc�肪����΍ė��p�A�Ȃ���ΐV�K����
        GameObject coin = pool.Count > 0 ? pool.Dequeue() : Instantiate(coinPrefab);
        coin.transform.position = position;
        coin.SetActive(true);
        return coin;
    }

    /// <summary>
    /// �g�p�ς݃R�C�����A�N�e�B�u�ɂ��A�v�[���ɖ߂��B
    /// </summary>
    /// <param name="coin">�ė��p�Ώۂ̃R�C��</param>
    public void ReturnCoin(GameObject coin)
    {
        coin.SetActive(false);
        pool.Enqueue(coin);
    }
}
