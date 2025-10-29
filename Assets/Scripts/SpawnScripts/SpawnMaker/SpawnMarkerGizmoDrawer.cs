using UnityEngine;
using UnityEditor;

/// <summary>
/// SpawnMarker �ɑ΂��ăV�[���r���[��ŃM�Y���iGizmo�j��`�悷��J�X�^���G�f�B�^�B
/// - Marker �̎�ނɉ����ĐF��ύX�iEnemy=��, Coin=���j
/// - Marker �̈ʒu�ɋ��̂�`��
/// - Marker �̃v���n�u�������x���\��
/// </summary>
[CustomEditor(typeof(SpawnMarker))]
public class SpawnMarkerGizmoDrawer : Editor
{
    /// <summary>
    /// Scene�r���[�ŃM�Y����`�悷��R�[���o�b�N
    /// </summary>
    void OnSceneGUI()
    {
        // �Ώۂ� SpawnMarker ���擾
        SpawnMarker marker = (SpawnMarker)target;

        // ---------------- 1. �M�Y���̐F�� Marker type �ɉ����Đݒ� ----------------
        Color gizmoColor = Color.white; // �f�t�H���g��
        switch (marker.type.ToLower()) // �����������Ĕ�r
        {
            case "enemy":
                gizmoColor = Color.red; // �G�}�[�J�[�͐�
                break;
            case "coin":
                gizmoColor = Color.yellow; // �R�C���͉��F
                break;
        }

        // ---------------- 2. �M�Y���`�� ----------------
        Handles.color = gizmoColor;

        // ���̃M�Y����`��
        // ��1����: controlID (0�Ŗ��Ȃ�)
        // ��2����: �`��ʒu
        // ��3����: ��] (Quaternion.identity = ��]�Ȃ�)
        // ��4����: �傫�� (0.5f)
        // ��5����: �C�x���g�^�C�v�iRepaint �ŕ`��̂݁j
        Handles.SphereHandleCap(0, marker.transform.position, Quaternion.identity, 0.5f, EventType.Repaint);

        // ---------------- 3. ���x���`�� ----------------
        // �}�[�J�[�ʒu�̏�����Ƀv���n�u����\��
        Handles.Label(marker.transform.position + Vector3.up * 0.6f, marker.prefabName);
    }
}
