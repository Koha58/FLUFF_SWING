using UnityEngine;

/// <summary>
/// �L�����N�^�[�̋��ʊ�{�����Ǘ����钊�ۃN���X
/// ScriptableObject�Ƃ��ăf�[�^��ێ����A
/// �v���C���[��G�ȂǗl�X�ȃL�����N�^�[���p�����Ďg�p���邱�Ƃ�z�肵�Ă���
/// </summary>
public abstract class CharacterBase : ScriptableObject
{
    /// <summary>�L�����N�^�[�̈�ӎ���ID</summary>
    public int id;

    /// <summary>�L�����N�^�[��</summary>
    public string characterName;

    /// <summary>�L�����N�^�[�̈ړ����x</summary>
    public float moveSpeed;
}