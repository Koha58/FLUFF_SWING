using UnityEngine;

/// <summary>
/// �v���C���[�̃��C���[�֘A�̐ݒ�f�[�^��ێ����� ScriptableObject
/// �C���X�y�N�^�[��Ŋe��p�����[�^�𒲐��\
/// </summary>
[CreateAssetMenu(fileName = "PlayerWireConfig", menuName = "Config/PlayerWireConfig")]
public class PlayerWireConfig : ScriptableObject
{
    /// <summary>���C���[�̌Œ蒷���i�P��: ���j�b�g�j</summary>
    public float fixedWireLength = 3.5f;

    /// <summary>�j�̔�ԑ��x�i�P��: ���j�b�g/�t���[���Ȃǁj</summary>
    public float needleSpeed = 0.3f;

    /// <summary>�X�C���O�J�n���̏����i�P��: ���j�b�g/�b�j</summary>
    public float swingInitialSpeed = 10f;

    /// <summary>���C���[�ڑ����̃v���C���[�̏d�̓X�P�[��</summary>
    public float playerGravityScale = 3f;

    /// <summary>�v���C���[�̕��������ɉe����^�����C��R�i���`�����j</summary>
    public float linearDamping = 0f;

    /// <summary>�v���C���[�̕��������ɉe����^�����]����</summary>
    public float angularDamping = 0f;
}
