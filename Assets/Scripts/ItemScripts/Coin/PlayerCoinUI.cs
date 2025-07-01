using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// �v���C���[�̏����R�C������UI�ɕ\������N���X�i�V���O���g���j
/// </summary>
public class PlayerCoinUI : MonoBehaviour
{
    /// <summary>
    /// �V���O���g���C���X�^���X�i���X�N���v�g����A�N�Z�X�\�j�B
    /// </summary>
    public static PlayerCoinUI Instance { get; private set; }

    /// <summary>
    /// �R�C������\������TextMeshProUGUI�B
    /// </summary>
    [SerializeField] private TextMeshProUGUI coinText;

    /// <summary>
    /// ���݂̃R�C�����B
    /// </summary>
    private int coinCount = 0;

    /// <summary>
    /// �C���X�^���X�������i�V���O���g���ݒ��UI�X�V�j�B
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        UpdateUI();
    }

    /// <summary>
    /// �w�肵���R�C���������Z����UI���X�V�B
    /// </summary>
    /// <param name="amount">���Z����R�C����</param>
    public void AddCoin(int amount)
    {
        coinCount += amount;
        UpdateUI();
    }

    /// <summary>
    /// UI�e�L�X�g�����݂̃R�C�����ōX�V�i2���\���A�[�����߁j�B
    /// </summary>
    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = coinCount.ToString("D2"); // ��F01, 09, 10, 25 �Ȃ�
    }
}
