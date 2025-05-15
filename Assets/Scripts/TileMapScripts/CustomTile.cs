using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// �J�X�^���^�C���N���X�B
/// Unity �� Tilemap �Ŏg�p����^�C�����g�����A
/// �Ǝ��̑����i�^�C�v�j��t�^�ł���悤�ɂ���B
/// </summary>
[CreateAssetMenu(fileName = "New Custom Tile", menuName = "Tiles/Custom Tile")]
public class CustomTile : Tile
{
    /// <summary>
    /// �^�C���̎�ނ�\���񋓌^�B
    /// - Ground : �v���C���[���ڑ��\�Ȓn��
    /// - Hazard : �댯�G���A�i�����I�ȗp�r�Ȃǂ�z��j
    /// </summary>
    public enum TileType
    {
        Ground,  // �n�ʃ^�C��
        Hazard   // �댯�^�C��
    }

    /// <summary>
    /// ���̃^�C���̃^�C�v�i�C���X�y�N�^�[����I���\�j�B
    /// �^�C�����Ƃ� Ground �� Hazard �Ȃǂ��w��ł���B
    /// </summary>
    public TileType tileType;
}
