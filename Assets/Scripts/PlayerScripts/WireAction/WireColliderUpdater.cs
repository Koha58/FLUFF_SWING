using UnityEngine;

/// <summary>
/// LineRenderer�̌`��ɍ��킹��EdgeCollider2D�̌`��������X�V����X�N���v�g�B
/// Wire�i���[�v�Ȃǁj�̓����蔻���LineRenderer�Ɠ������������Ƃ��Ɏg�p����B
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class WireColliderUpdater : MonoBehaviour
{
    /// <summary>
    /// �����蔻��̌��ɂȂ�LineRenderer
    /// </summary>
    [SerializeField] private LineRenderer lineRenderer;

    /// <summary>
    /// ���ۂɌ`����X�V����EdgeCollider2D
    /// </summary>
    private EdgeCollider2D edgeCollider;

    /// <summary>
    /// �����������BEdgeCollider2D���擾����B
    /// </summary>
    void Awake()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        edgeCollider.enabled = false; // �O�̂��ߖ�����
    }

    /// <summary>
    /// ���t���[����LateUpdate�ŃR���C�_�[���X�V����B
    /// </summary>
    void LateUpdate()
    {
        UpdateCollider();
    }

    /// <summary>
    /// LineRenderer�̒��_��EdgeCollider2D�̃|�C���g�ɕϊ����Đݒ肷��B
    /// LineRenderer�������Ȃ�Collider������������B
    /// </summary>
    void UpdateCollider()
    {
        if (lineRenderer == null || !lineRenderer.enabled)
        {
            // LineRenderer����\���Ȃ�R���C�_�[��������
            edgeCollider.enabled = false;
            return;
        }

        int pointCount = lineRenderer.positionCount;

        if (pointCount < 2)
        {
            // ���_������Ȃ��ꍇ��������
            edgeCollider.points = new Vector2[0];
            edgeCollider.enabled = false;
            return;
        }

        // ���_��ݒ�
        Vector2[] points = new Vector2[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 worldPos = lineRenderer.GetPosition(i);
            points[i] = transform.InverseTransformPoint(worldPos);
        }

        edgeCollider.points = points;
        edgeCollider.enabled = true;
    }
}
