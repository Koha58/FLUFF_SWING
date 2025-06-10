using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    public Transform player;

    // �v���C���[����̉������̃I�t�Z�b�g
    public float horizontalOffset = 2f;

    [SerializeField]
    private WireActionScript wireActionScript;

    // �X���[�W���O�̎���(�����傫���قǂ������ړ�)
    public float smoothTime = 0.3f;

    // SmoothDamp�p
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (player == null || wireActionScript == null) return;

        bool isConnected = wireActionScript.IsConnected;

        // ���C���[�ݒu�ꏊ�̍��W�������Ă���
        Vector2 wirePos = wireActionScript.HookedPosition;

        // �^�[�Q�b�g�ʒu������iX���W�̂݊��炩�ɒǏ]�j
        float targetX;

        // ���C���[�g�p��
        if (isConnected)
        {
            targetX = wirePos.x;
        }
        // ���C���[�s�g�p��
        else
        {
            targetX = player.position.x + horizontalOffset;
        }

        // ���݂̃J�����ʒu����Ƀ^�[�Q�b�g�ʒu�փX���[�Y�Ɉړ�
        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(targetX, currentPos.y, currentPos.z);

        transform.position = Vector3.SmoothDamp(currentPos, targetPos, ref velocity, smoothTime);
    }
}