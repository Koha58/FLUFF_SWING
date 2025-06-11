using UnityEngine;

/// <summary>
/// �L�����N�^�[���Ƃ̌ʃX�e�[�^�X����ێ�����N���X
/// CharacterBase ���p�����A�ǉ��̃X�e�[�^�X�iHP��U���͂Ȃǁj���`����
/// ScriptableObject �Ƃ��ăA�Z�b�g������A�f�[�^�Ǘ��Ɏg�p�����
/// </summary>
[CreateAssetMenu(fileName = "CharacterStatus", menuName = "Master/CharacterStatus")]
public class CharacterStatus : CharacterBase
{
    /// <summary>�ő�HP</summary>
    public int maxHP;

    /// <summary>�U����</summary>
    public int attack;

    /// <summary>�ߋ����U���\�Ȕ͈́i�g��Ȃ��ꍇ��0�j</summary>
    public float meleeRange;

    /// <summary>�������U���\�ȍő�͈́i�g��Ȃ��ꍇ��0�j</summary>
    public float attackRadius;
}