using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackGroundMover : MonoBehaviour
{
    private const float k_maxLength = 1f;
    private const string k_propName = "_MainTex";

    // ���C���[�A�N�V�����̏�ԁi�ڑ���ԂȂǁj���Ǘ�����X�N���v�g
    [SerializeField]
    private WireActionScript wireActionScript;

    [SerializeField]
    private Vector2 m_offsetSpeed;

    [SerializeField]
    private Transform playerTransform; // �v���C���[��Transform

    private Material m_copiedMaterial;

    private void Start()
    {
        var image = GetComponent<Image>();
        // �}�e���A���̕������쐬���Ďg�p
        m_copiedMaterial = new Material(image.material);
        image.material = m_copiedMaterial;

        // �}�e���A����null���������O���o�܂��B
        Assert.IsNotNull(m_copiedMaterial);
        Assert.IsNotNull(playerTransform);
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        // x��y�̒l��0 �` 1�Ń��s�[�g����悤�ɂ���
        var x = Mathf.Repeat(playerTransform.position.x * m_offsetSpeed.x, k_maxLength);
        var y = Mathf.Repeat(playerTransform.position.y * m_offsetSpeed.y, k_maxLength);

        // �v���C���[�̈ړ��ɍ��킹��
        // �v���C���[�����C���[�ړ����͔w�i�͓������Ȃ�
        // ���C���[�ؒf��̍��W�ɔw�i������������̂����P����
        if (!wireActionScript.IsConnected)
        {
            var offset = new Vector2(x, y);
            m_copiedMaterial.SetTextureOffset(k_propName, offset);
        }
    }

    private void OnDestroy()
    {
        // �Q�[���I�u�W�F�N�g�j�󎞂Ƀ}�e���A���̃R�s�[�������Ă���
        Destroy(m_copiedMaterial);
        m_copiedMaterial = null;
    }
}