using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// CSV�t�@�C����ǂݍ���� SpawnDataSO�iScriptableObject�j��������������G�f�B�^�g���c�[���B
/// Tools > SpawnData Importer ����N���\�B
/// </summary>
public class SpawnDataImporter : EditorWindow
{
    // CSV�t�@�C���̃p�X�i�����l�j
    string csvPath = "Assets/Data/SpawnData.csv";

    // ScriptableObject�̕ۑ��p�X�i�����l�j
    string assetSavePath = "Assets/Resources/SpawnData/SpawnDataSO.asset";

    /// <summary>
    /// ���j���[����E�B���h�E���J��
    /// </summary>
    [MenuItem("Tools/SpawnData Importer")]
    public static void ShowWindow()
    {
        GetWindow<SpawnDataImporter>("SpawnData Importer");
    }

    /// <summary>
    /// �E�B���h�E��GUI�`��
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("CSV Path"); // ���x��
        csvPath = EditorGUILayout.TextField(csvPath); // CSV�t�@�C���p�X���͗�

        GUILayout.Label("Save Asset Path"); // ���x��
        assetSavePath = EditorGUILayout.TextField(assetSavePath); // �o�͐�p�X���͗�

        // �C���|�[�g�{�^��
        if (GUILayout.Button("Import CSV to ScriptableObject"))
        {
            ImportCSV();
        }
    }

    /// <summary>
    /// CSV�t�@�C����ǂݍ���� ScriptableObject �𐶐�����
    /// </summary>
    void ImportCSV()
    {
        // �t�@�C�����݃`�F�b�N
        if (!File.Exists(csvPath))
        {
            Debug.LogError("CSV�t�@�C����������܂���: " + csvPath);
            return;
        }

        // �S�s�ǂݍ���
        string[] lines = File.ReadAllLines(csvPath);

        if (lines.Length < 2)
        {
            Debug.LogError("CSV�t�@�C���̓��e���s�����Ă��܂��i�Œ�1�s�̃f�[�^���K�v�j");
            return;
        }

        // ScriptableObject�C���X�^���X�쐬
        SpawnDataSO dataSO = ScriptableObject.CreateInstance<SpawnDataSO>();
        dataSO.entries = new SpawnDataEntry[lines.Length - 1]; // 1�s�ڂ̓w�b�_�[

        // �e�f�[�^�s���p�[�X
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');

            SpawnDataEntry entry = new SpawnDataEntry();
            entry.type = cols[1].Trim();           // ��ށi��: Enemy, Coin�j
            entry.prefabName = cols[2].Trim();     // �v���n�u��

            float x = float.Parse(cols[3]);        // ���WX
            float y = float.Parse(cols[4]);        // ���WY
            float z = float.Parse(cols[5]);        // ���WZ

            entry.position = new Vector3(x, y, z); // Vector3�Ƃ��Ċi�[

            dataSO.entries[i - 1] = entry;         // �z��ɒǉ�
        }

        // �ۑ���f�B���N�g�������݂��Ȃ��ꍇ�͍쐬
        string dir = Path.GetDirectoryName(assetSavePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh(); // Unity�G�f�B�^�ɔ��f
        }

        // ScriptableObject ���A�Z�b�g�Ƃ��ĕۑ�
        AssetDatabase.CreateAsset(dataSO, assetSavePath);
        AssetDatabase.SaveAssets();

        Debug.Log("SpawnDataSO���쐬���܂���: " + assetSavePath);
    }
}
