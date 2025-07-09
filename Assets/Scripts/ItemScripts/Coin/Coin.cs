using UnityEngine;

/// <summary>
/// �v���C���[���G�ꂽ�Ƃ��ɃR�C���Ƃ��Ă̏������s���N���X�B
/// �R�C��UI���X�V���A�I�u�W�F�N�g�v�[���֕ԋp����B
/// �X�C���O���i���C���[�g�p���j�ł��m���Ɏ擾�ł���悤�A
/// FixedUpdate�Ŗ��t���[���ڐG������s���݌v�B
/// </summary>
public class Coin : MonoBehaviour
{
    // FixedUpdate�͕������Z�̍X�V�^�C�~���O�ŌĂ΂��
    // ���t���[���A�v���C���[�Ƃ̐ڐG���m�F
    private void FixedUpdate()
    {
        CheckForCoinOverlap();
    }

    /// <summary>
    /// �R�C���̒��S�𒆐S�Ƃ�����蔼�a���Ƀv���C���[�����݂��邩�𔻒肷��B
    /// �Y������ꍇ�A�R�C���l���������s���A�I�u�W�F�N�g�v�[���֕ԋp����B
    /// </summary>
    void CheckForCoinOverlap()
    {
        // ���a0.5f�̉~�͈͓��ɂ��邷�ׂĂ�Collider2D���擾
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        foreach (var hit in hits)
        {
            // "Player"�^�O�����I�u�W�F�N�g�i�v���C���[�j�Ƃ̐ڐG�����o
            if (hit.CompareTag("Player"))
            {
                // �R�C��UI�̃J�E���g��1���₷�i�V���O���g���p�^�[�����g�p�j
                PlayerCoinUI.Instance.AddCoin(1);

                // ���̃R�C�����A�N�e�B�u�����A�I�u�W�F�N�g�v�[���ɕԋp
                CoinPoolManager.Instance.ReturnCoin(this.gameObject);

                // 1�̃v���C���[�ɂ̂ݔ�������Ώ\���Ȃ̂Ń��[�v�𔲂���
                break;
            }
        }
    }
}
