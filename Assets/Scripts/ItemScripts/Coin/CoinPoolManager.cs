using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �R�C���̃I�u�W�F�N�g�v�[�����Ǘ�����V���O���g���N���X�B
/// �K�v�ȂƂ��ɃR�C�����擾���A�s�v�ɂȂ�����ė��p�\�ȏ�ԂŃv�[���ɖ߂��B
/// </summary>
public class CoinPoolManager : MonoBehaviour
{
    /// <summary>
    /// �V���O���g���C���X�^���X�ւ̃A�N�Z�X�p�v���p�e�B�B
    /// ����ɂ��ǂ�����ł�CoinPoolManager�̗B��̃C���X�^���X���擾�\�B
    /// </summary>
    public static CoinPoolManager Instance { get; private set; }

    /// <summary>
    /// �v�[������R�C���̃v���n�u���C���X�y�N�^�[�Őݒ肷��B
    /// </summary>
    [SerializeField] private GameObject coinPrefab;

    /// <summary>
    /// �Q�[���J�n���ɐ������Ă����R�C���̏������B
    /// </summary>
    [SerializeField] private int initialPoolSize = 20;

    /// <summary>
    /// �g�p���Ă��Ȃ��i��A�N�e�B�u�ȁj�R�C����ۊǂ���L���[�B
    /// �V���ɃR�C�����K�v�ȂƂ��͂���������o���A�s�v�ɂȂ�����߂��B
    /// </summary>
    private Queue<GameObject> pool = new Queue<GameObject>();

    /// <summary>
    /// Awake�̓I�u�W�F�N�g�������ɌĂ΂�鏉�������\�b�h�B
    /// �V���O���g���̃Z�b�g�A�b�v�Ə����v�[���������s���B
    /// </summary>
    private void Awake()
    {
        // �V���O���g���̏d���h�~����
        if (Instance == null)
            Instance = this; // �ŏ��̃C���X�^���X�Ƃ��ēo�^
        else
        {
            Destroy(gameObject); // ���łɑ��݂���Ȃ玩����j��
            return;
        }

        // �����v�[���̐���
        for (int i = 0; i < initialPoolSize; i++)
        {
            // �R�C���𐶐����ACoinPoolManager�̎q�ɐݒ肷�邱�Ƃ�
            // Hierarchy�����������i���₷���Ȃ�j
            var coin = Instantiate(coinPrefab, transform);

            coin.SetActive(false); // ��������͔�A�N�e�B�u�ɂ���
            pool.Enqueue(coin);    // �v�[���ɒǉ�
        }
    }

    /// <summary>
    /// �v�[������R�C�����擾���A�w�肵���ʒu�ɔz�u���ăA�N�e�B�u������B
    /// �v�[���ɋ󂫂��Ȃ���ΐV�K�������e��ݒ肷��B
    /// </summary>
    /// <param name="position">�\�����������[���h���W</param>
    /// <returns>�g�p�\�ȃR�C����GameObject</returns>
    public GameObject GetCoin(Vector3 position)
    {
        // �v�[��������o�����A�V�K�ɐ�������
        GameObject coin = pool.Count > 0 ? pool.Dequeue() : Instantiate(coinPrefab, transform);

        coin.transform.position = position; // �ʒu�ݒ�
        coin.SetActive(true);                // �\����Ԃɐ؂�ւ�

        return coin; // �g�p�\�ȃR�C����Ԃ�
    }

    /// <summary>
    /// �g�p�ς݂̃R�C�����A�N�e�B�u�ɂ��ăv�[���ɖ߂��B
    /// </summary>
    /// <param name="coin">�ė��p�Ώۂ̃R�C��GameObject</param>
    public void ReturnCoin(GameObject coin)
    {
        coin.SetActive(false); // �\��������
        pool.Enqueue(coin);    // �v�[���ɖ߂��i�L���[�ɒǉ��j
    }
}
