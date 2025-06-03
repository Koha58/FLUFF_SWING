using UnityEditor;
using UnityEngine;
using System.IO;
using System.Globalization;

/// <summary>
/// CSV�t�@�C������CharacterStatus��ScriptableObject�𐶐��E�X�V����G�f�B�^�g���N���X
/// </summary>
public class CsvToScriptableObjectImporter : EditorWindow
{
    // ���j���[�ɁuTools/Import Character Status CSV�v��ǉ����A��������Import���\�b�h�����s�\�ɂ���
    [MenuItem("Tools/Import Character Status CSV")]
    public static void Import()
    {
        // CSV�t�@�C���̃p�X�iAssets�t�H���_������Data�t�H���_���j
        string csvPath = Application.dataPath + "/Data/CharacterStatus.csv";
        // ScriptableObject�̕ۑ���p�X�iResources�t�H���_���j
        string assetPath = "Assets/Resources/CharacterStatus/";

        // �ۑ���t�H���_���Ȃ���΍쐬����
        if (!Directory.Exists(assetPath))
            Directory.CreateDirectory(assetPath);

        // ������ScriptableObject�����ׂč폜���A�d����h��
        foreach (string file in Directory.GetFiles(assetPath, "*.asset"))
        {
            AssetDatabase.DeleteAsset(file);
        }

        // CSV�t�@�C���̑S�s��ǂݍ��ށi1�s�ڂ̓w�b�_�[�Ƒz��j
        string[] lines = File.ReadAllLines(csvPath);

        // 2�s�ځi�C���f�b�N�X1�j�ȍ~�����[�v����
        for (int i = 1; i < lines.Length; i++) // skip header
        {
            // �s�̓��e���J���}�ŕ���
            string[] values = lines[i].Split(',');

            // �l���s�����Ă��邩�A�L�����N�^�[������̏ꍇ�͏������X�L�b�v���x�����O���o��
            if (values.Length < 5 || string.IsNullOrEmpty(values[1]))
            {
                Debug.LogWarning($"�X�L�b�v���ꂽ�s: {lines[i]}");
                continue;
            }

            // �V����CharacterStatus ScriptableObject���쐬
            CharacterStatus status = ScriptableObject.CreateInstance<CharacterStatus>();
            // CSV�̊e���Ή�����t�B�[���h�ɕϊ����ăZ�b�g
            status.id = int.Parse(values[0]);
            status.characterName = values[1];
            status.maxHP = int.Parse(values[2]);
            status.attack = int.Parse(values[3]);
            // float�̃p�[�X�̓J���`���[�ˑ����Ȃ��悤InvariantCulture���w��
            status.moveSpeed = float.Parse(values[4], CultureInfo.InvariantCulture);

            // �ۑ�����t�@�C�������L�����N�^�[���Ɋ�Â��č쐬
            string name = $"CharacterStatus_{status.characterName}";
            // �w��t�H���_��ScriptableObject�A�Z�b�g�Ƃ��ĕۑ�
            AssetDatabase.CreateAsset(status, $"{assetPath}{name}.asset");
        }

        // �A�Z�b�g�f�[�^�x�[�X�ɕύX��ۑ����AUnity�G�f�B�^�ɔ��f������
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CharacterStatus ScriptableObjects Imported!");
    }
}