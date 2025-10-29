using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// CustomTile �� TaggedRuleTile ���ꊇ�ҏW���邽�߂̃J�X�^���G�f�B�^�E�B���h�E�B
/// �v���W�F�N�g���̊Y���^�C�����܂Ƃ߂ēǂݍ��݁ATileType ���ꊇ�ŕύX�ł��܂��B
/// </summary>
public class CustomTileEditorWindow : EditorWindow
{
    // GUI ��Őݒ肷��V���� TileType�iGround, Hazard �Ȃǁj
    private CustomTile.TileType newTileType = CustomTile.TileType.Ground;

    // �ǂݍ��񂾂��ׂẴ^�C���iCustomTile, TaggedRuleTile�j�̋��ʃC���^�[�t�F�[�X�z��
    private ITileWithType[] tiles;

    /// <summary>
    /// Unity ���j���[�ɁuTools/Custom Tile Editor�v��ǉ�����
    /// </summary>
    [MenuItem("Tools/Custom Tile Editor")]
    public static void ShowWindow()
    {
        // �E�B���h�E���J���i�܂��͊����̃E�B���h�E���A�N�e�B�u�ɂ���j
        GetWindow<CustomTileEditorWindow>("Custom Tile Editor");
    }

    /// <summary>
    /// �G�f�B�^�E�B���h�E��GUI�`�揈���i�{�^���A�Z���N�^�Ȃǁj
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("Custom Tile Type Editor", EditorStyles.boldLabel);

        // �^�C���ǂݍ��݃{�^��
        if (GUILayout.Button("Load All CustomTiles & TaggedRuleTiles"))
        {
            LoadTiles();
        }

        // �^�C�����ǂݍ��܂�Ă���ꍇ
        if (tiles != null && tiles.Length > 0)
        {
            // �^�C���^�C�v�I���t�B�[���h
            newTileType = (CustomTile.TileType)EditorGUILayout.EnumPopup("New TileType", newTileType);

            // �ꊇ�ݒ�{�^��
            if (GUILayout.Button("Set TileType for All Loaded Tiles"))
            {
                foreach (var tile in tiles)
                {
                    // Undo �Ή��iCtrl+Z �Ŗ߂���悤�Ɂj
                    Undo.RecordObject((Object)tile, "Change Tile Type");

                    // TileType ��ύX
                    tile.tileType = newTileType;

                    // �G�f�B�^�ɕύX��ʒm
                    EditorUtility.SetDirty((Object)tile);
                }

                // �A�Z�b�g�̕ύX��ۑ�
                AssetDatabase.SaveAssets();

                Debug.Log($"Set {newTileType} to {tiles.Length} tiles.");
            }

            // ���[�h���ꂽ�^�C�����̕\��
            GUILayout.Label($"{tiles.Length} tiles loaded.");
        }
        else
        {
            GUILayout.Label("No tiles loaded.");
        }
    }

    /// <summary>
    /// �v���W�F�N�g������ CustomTile �� TaggedRuleTile �������E�ǂݍ���
    /// </summary>
    private void LoadTiles()
    {
        // CustomTile �A�Z�b�g�� GUID ���擾
        var customTileGUIDs = AssetDatabase.FindAssets("t:CustomTile");

        // TaggedRuleTile �A�Z�b�g�� GUID ���擾
        var taggedRuleTileGUIDs = AssetDatabase.FindAssets("t:TaggedRuleTile");

        // GUID ����A�Z�b�g�����[�h���Anull �łȂ����̂� ITileWithType �ɃL���X�g
        var customTiles = customTileGUIDs
            .Select(guid => AssetDatabase.LoadAssetAtPath<CustomTile>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(tile => tile != null)
            .Cast<ITileWithType>();

        var ruleTiles = taggedRuleTileGUIDs
            .Select(guid => AssetDatabase.LoadAssetAtPath<TaggedRuleTile>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(tile => tile != null)
            .Cast<ITileWithType>();

        // ���҂��������� tiles �z��Ɋi�[
        tiles = customTiles.Concat(ruleTiles).ToArray();
    }
}
