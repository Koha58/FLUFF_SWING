using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �v���C���[�̃��C�t�i�n�[�g�jUI�𐧌䂷��N���X�B
/// - ���C�t���ɉ����ăn�[�g�A�C�R���𐶐��E�z�u
/// - ����HP�ɉ����ăn�[�g�̕\��/��\����؂�ւ���
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    /// <summary>
    /// �n�[�g�̃v���n�u�iImage�Ȃǂ�UI�v�f�j
    /// </summary>
    [SerializeField] private GameObject heartPrefab;

    /// <summary>
    /// �n�[�g����ׂ�Panel�iHorizontalLayoutGroup �Ȃǂ��g��Ȃ��ꍇ�j
    /// </summary>
    [SerializeField] private RectTransform heartPanel;

    /// <summary>
    /// �n�[�g���m�̉��̊Ԋu�iX���j
    /// </summary>
    private float heartSpacing = 70f;

    /// <summary>
    /// �ŏ��̃n�[�g��X���W�I�t�Z�b�g�i�p�l���̒��S����̂���j
    /// </summary>
    private float startOffsetX = 100f;

    /// <summary>
    /// �n�[�g��Y���W�i�ʒu���Œ肵�����ꍇ�Ɏg�p�j
    /// </summary>
    private float startOffsetY = -65f;

    /// <summary>
    /// ���ݕ\�����̃n�[�g�A�C�R���̃��X�g
    /// </summary>
    private List<GameObject> heartIcons = new List<GameObject>();

    /// <summary>
    /// �ő�HP�ɉ����ăn�[�g�A�C�R��������������B
    /// ���łɂ���A�C�R���͍폜���A�w�萔�Ԃ񐶐��E�z�u����B
    /// </summary>
    /// <param name="maxHP">�v���C���[�̍ő�HP</param>
    public void SetMaxHealth(int maxHP)
    {
        // �Â��n�[�gUI�����ׂč폜
        foreach (var icon in heartIcons)
        {
            Destroy(icon);
        }
        heartIcons.Clear();

        // �V�����n�[�gUI�𐶐����ĉ��ɕ��ׂ�
        for (int i = 0; i < maxHP; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartPanel);
            RectTransform rt = heart.GetComponent<RectTransform>();

            // �n�[�g�̈ʒu��ݒ�iX�͊Ԋu�����炷�AY�͌Œ�j
            rt.anchoredPosition = new Vector2(startOffsetX + i * heartSpacing, startOffsetY);

            // ���X�g�ɒǉ����ĊǗ�
            heartIcons.Add(heart);
        }
    }

    /// <summary>
    /// ���݂�HP�ɉ����ăn�[�g�̕\��/��\�����X�V����B
    /// �c��HP�ȏ�̃n�[�g�͔�\���ɂ���B
    /// </summary>
    /// <param name="currentHP">���݂̃v���C���[HP</param>
    public void UpdateHealth(int currentHP)
    {
        for (int i = 0; i < heartIcons.Count; i++)
        {
            // ����HP�ȉ��Ȃ�\���A����ȍ~�͔�\����
            heartIcons[i].SetActive(i < currentHP);
        }
    }
}
