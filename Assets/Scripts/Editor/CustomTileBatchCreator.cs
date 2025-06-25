using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps; // Tile�N���X��Tilemap�nAPI�ɕK�v

/// <summary>
/// �J�X�^���^�C���iCustomTile�j���ꊇ�쐬����G�f�B�^�g���E�B���h�E�B
/// �w�肵���X�v���C�g�t�H���_���̃X�v���C�g����ACustomTile���ꊇ�Ő������܂��B
/// </summary>
public class CustomTileBatchCreator : EditorWindow
{
    // �X�v���C�g���i�[����Ă���t�H���_�i���[�U�[�w��j
    private Object spriteFolder;

    // �쐬�����^�C����ۑ�����p�X
    private string savePath = "Assets/Tiles/";

    // �쐬����^�C���̃f�t�H���g�^�C�v�iGround�܂���Hazard�j
    private CustomTile.TileType defaultTileType = CustomTile.TileType.Ground;

    // Hazard�^�C�v�̏ꍇ�̃f�t�H���g�_���[�W��
    private int defaultDamageAmount = 1;

    // ���j���[����J����悤�ɂ���
    [MenuItem("Tools/Custom Tile Batch Creator")]
    public static void ShowWindow()
    {
        GetWindow<CustomTileBatchCreator>("Custom Tile Batch Creator");
    }

    // �G�f�B�^�E�B���h�E��GUI�`��
    private void OnGUI()
    {
        GUILayout.Label("�J�X�^���^�C���ꊇ�쐬", EditorStyles.boldLabel);

        // �X�v���C�g�t�H���_�w��t�B�[���h
        spriteFolder = EditorGUILayout.ObjectField("�X�v���C�g�t�H���_", spriteFolder, typeof(Object), false);

        // �ۑ���p�X�w��t�B�[���h
        savePath = EditorGUILayout.TextField("�ۑ���p�X", savePath);

        // �f�t�H���g�̃^�C���^�C�v�I��
        defaultTileType = (CustomTile.TileType)EditorGUILayout.EnumPopup("�f�t�H���gTileType", defaultTileType);

        // �^�C���^�C�v��Hazard�Ȃ�_���[�W�ʂ��w��
        if (defaultTileType == CustomTile.TileType.Hazard)
        {
            defaultDamageAmount = EditorGUILayout.IntField("�f�t�H���g�_���[�W��", defaultDamageAmount);
        }

        // �ꊇ�쐬�{�^��
        if (GUILayout.Button("�ꊇ�쐬"))
        {
            CreateCustomTiles();
        }
    }

    /// <summary>
    /// �w�肳�ꂽ�X�v���C�g����CustomTile���ꊇ�������鏈��
    /// </summary>
    private void CreateCustomTiles()
    {
        // �X�v���C�g�t�H���_���w��̏ꍇ�̓G���[
        if (spriteFolder == null)
        {
            Debug.LogError("�X�v���C�g�t�H���_���w�肳��Ă��܂���B");
            return;
        }

        // �t�H���_�p�X���擾
        string folderPath = AssetDatabase.GetAssetPath(spriteFolder);

        // �w��t�H���_���̑S�X�v���C�g���擾
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

        // �ۑ���t�H���_�����݂��Ȃ��ꍇ�͍쐬
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // �e�X�v���C�g�ɑ΂��ă^�C�����쐬
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            // CustomTile�̃C���X�^���X�𐶐�
            CustomTile tile = ScriptableObject.CreateInstance<CustomTile>();

            // �X�v���C�g�ݒ�
            tile.sprite = sprite;

            // �R���C�_�[�̃^�C�v�i�X�v���C�g�`��Ɉ�v�j
            tile.colliderType = Tile.ColliderType.Sprite;

            // �f�t�H���g�̃^�C���^�C�v��ݒ�
            tile.tileType = defaultTileType;

            // Hazard�̏ꍇ�̓_���[�W�ʂ��ݒ�
            if (tile.tileType == CustomTile.TileType.Hazard)
            {
                tile.damageAmount = defaultDamageAmount;
            }

            // �t�@�C�����ƕۑ���p�X���\�z
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string tilePath = Path.Combine(savePath, fileName + ".asset");

            // �A�Z�b�g�Ƃ��ĕۑ�
            AssetDatabase.CreateAsset(tile, tilePath);
        }

        // �A�Z�b�g��ۑ��E�X�V
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("�J�X�^���^�C���̈ꊇ�쐬���������܂����B");
    }
}
