using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// �^�O�t���̃��[���x�[�X�^�C���iRuleTile�j
/// RuleTile ���g�����ACustomTile.TileType ��ێ��ł���悤�ɂ����N���X�B
/// �G�f�B�^�c�[������ CustomTile �Ɠ��l�Ɉꊇ�� tileType ��ҏW�\�ɂ��邽�߁A
/// ITileWithType �C���^�[�t�F�[�X���������Ă���B
/// </summary>
[CreateAssetMenu(menuName = "Tiles/Tagged RuleTile")]
[System.Serializable]
public class TaggedRuleTile : RuleTile, ITileWithType
{
    // ���̃^�C���̃^�C�v�iGround, Hazard �Ȃǁj
    [SerializeField]
    private CustomTile.TileType _tileType = CustomTile.TileType.Ground;

    /// <summary>
    /// ITileWithType �ŗv������� tileType �v���p�e�B�B
    /// �J�X�^���G�f�B�^�Ȃǂŋ��ʂ̕��@�ŃA�N�Z�X�ł���悤�ɂ���B
    /// </summary>
    public CustomTile.TileType tileType
    {
        get => _tileType;
        set => _tileType = value;
    }
}
