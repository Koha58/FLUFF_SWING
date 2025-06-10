using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackGroundScript : MonoBehaviour
{
    private const float k_maxLength = 1f;
    private const string k_propName = "_MainTex";

    // �}�e���A�������p
    private Material m_copiedMaterial;

    private void Start()
    {
        var image = GetComponent<Image>();
        // �}�e���A���̕������쐬���Ďg�p
        m_copiedMaterial = new Material(image.material);
        image.material = m_copiedMaterial;

        // �}�e���A����null���������O���o��
        Assert.IsNotNull(m_copiedMaterial);
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

    }

    private void OnDestroy()
    {
        // �Q�[���I�u�W�F�N�g�j�󎞂Ƀ}�e���A���̃R�s�[�������Ă���
        Destroy(m_copiedMaterial);
        m_copiedMaterial = null;
    }
}