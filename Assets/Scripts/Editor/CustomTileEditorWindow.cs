using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// CustomTile ���G�f�B�^��ňꊇ�ҏW���邽�߂̃J�X�^���G�f�B�^�E�B���h�E�B
/// - �v���W�F�N�g���� CustomTile ��S�ēǂݍ���
/// - �S�^�C���� TileType ���ꊇ�ύX
/// </summary>
public class CustomTileEditorWindow : EditorWindow
{
    // �V�����ݒ肷�� TileType�iGUI ��őI���\�j
    private CustomTile.TileType newTileType = CustomTile.TileType.Ground;

    // �ǂݍ��� CustomTile �̔z��
    private CustomTile[] tiles;

    /// <summary>
    /// ���j���[�ɁuTools/Custom Tile Editor�v��ǉ��B
    /// �N���b�N����ƃE�B���h�E���\�������B
    /// </summary>
    [MenuItem("Tools/Custom Tile Editor")]
    public static void ShowWindow()
    {
        // �G�f�B�^�E�B���h�E���J���i�܂��̓t�H�[�J�X�j
        GetWindow<CustomTileEditorWindow>("Custom Tile Editor");
    }

    /// <summary>
    /// �G�f�B�^�E�B���h�E�� GUI �`�揈��
    /// </summary>
    private void OnGUI()
    {
        // ���o���\��
        GUILayout.Label("Custom Tile Type Editor", EditorStyles.boldLabel);

        // �^�C���ǂݍ��݃{�^��
        if (GUILayout.Button("Load All CustomTiles"))
        {
            LoadTiles();
        }

        // �^�C�������[�h�ς݂��`�F�b�N
        if (tiles != null && tiles.Length > 0)
        {
            // TileType �̑I�� UI
            newTileType = (CustomTile.TileType)EditorGUILayout.EnumPopup("New TileType", newTileType);

            // �S�^�C���ɑ΂��ĐV�����^�C�v���ꊇ�ݒ�
            if (GUILayout.Button("Set TileType for All Loaded Tiles"))
            {
                foreach (var tile in tiles)
                {
                    // Undo �ɑΉ�������iCtrl+Z�Ή��j
                    Undo.RecordObject(tile, "Change Tile Type");

                    // �^�C���̃^�C�v��ύX
                    tile.tileType = newTileType;

                    // �ύX���G�f�B�^�ɒʒm�i�C���X�y�N�^�X�V�Ȃǁj
                    EditorUtility.SetDirty(tile);
                }

                // �ύX��ۑ�
                AssetDatabase.SaveAssets();

                Debug.Log($"Set {newTileType} to {tiles.Length} tiles.");
            }

            // �ǂݍ��񂾃^�C�����\��
            GUILayout.Label($"{tiles.Length} tiles loaded.");
        }
        else
        {
            // �^�C�������[�h����Ă��Ȃ��ꍇ�̃��b�Z�[�W
            GUILayout.Label("No tiles loaded.");
        }
    }

    /// <summary>
    /// �v���W�F�N�g�����炷�ׂĂ� CustomTile �A�Z�b�g���������ēǂݍ���
    /// </summary>
    private void LoadTiles()
    {
        // �^�C�v CustomTile �� GUID ��S����
        string[] guids = AssetDatabase.FindAssets("t:CustomTile");

        // GUID ����p�X���擾���A���ۂ̃A�Z�b�g�Ƃ��ă��[�h
        tiles = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<CustomTile>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToArray();
    }
}
