/// <summary>
/// �^�C���ɋ��ʂ��� TileType ����񋟂��邽�߂̃C���^�[�t�F�[�X�B
/// CustomTile �� TaggedRuleTile �Ɏ��������邱�ƂŁA
/// �G�f�B�^�g���Ȃǂŗ��҂����ʂ̌^�Ƃ��Ĉ�����悤�ɂ���B
/// </summary>
public interface ITileWithType
{
    /// <summary>
    /// �^�C���̃^�C�v�i��: Ground, Hazard �Ȃǁj���擾�܂��͐ݒ肷��B
    /// </summary>
    CustomTile.TileType tileType { get; set; }
}
