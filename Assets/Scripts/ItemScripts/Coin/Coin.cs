using UnityEngine;

/// <summary>
/// �v���C���[���G�ꂽ�Ƃ��ɃR�C���Ƃ��Ă̏������s���N���X�B
/// �R�C��UI���X�V���A�I�u�W�F�N�g�v�[���֕ԋp����B
/// </summary>
public class Coin : MonoBehaviour
{
    /// <summary>
    /// ����Collider�ƐڐG�����ۂɌĂ΂��i2D�p�j�B
    /// �v���C���[�ƐڐG�����ꍇ�A�R�C���擾���������s�B
    /// </summary>
    /// <param name="collision">�ڐG����Collider���</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �ڐG�������肪�v���C���[���ǂ����m�F
        if (collision.CompareTag("Player"))
        {
            // �R�C��UI�̃J�E���g��1���₷�i�V���O���g�����g�p�j
            PlayerCoinUI.Instance.AddCoin(1);

            // ���̃R�C�����A�N�e�B�u�ɂ��ăI�u�W�F�N�g�v�[���֕ԋp
            CoinPoolManager.Instance.ReturnCoin(this.gameObject);
        }
    }
}
