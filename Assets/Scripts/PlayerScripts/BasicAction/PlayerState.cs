/// <summary>
/// �v���C���[�̏�Ԃ�\���񋓌^�B
/// �Q�[�����ł̃v���C���[�̍s����A�j���[�V��������ɗ��p�����B
/// </summary>
public enum PlayerState
{
    /// <summary>�ҋ@��ԁi�������Ă��Ȃ��A��~���j</summary>
    Idle,

    /// <summary>�����Ă�����</summary>
    Run,

    /// <summary>�W�����v���̏��</summary>
    Jump,

    /// <summary>���C���[�A�N�V�������i���C���[�ڑ��A�X�C���O�Ȃǁj</summary>
    Wire,

    /// <summary>���n��������̏�ԁi���n���[�V�����Ȃǁj</summary>
    Landing,

    /// <summary>�ߐڍU�����s���Ă�����</summary>
    MeleeAttack,

    /// <summary>�������U�����s���Ă�����</summary>
    RangedAttack,

    /// <summary>�_���[�W���󂯂Ă�����</summary>
    Damage,

    /// <summary>�S�[���i�X�e�[�W�N���A�j�������</summary>
    Goal,
}
