using Unity.VisualScripting;
using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    /// <summary>
    /// ���C���[�A�N�V�����̏�ԁi�ڑ���ԂȂǁj���Ǘ�����X�N���v�g
    /// </summary>
    [SerializeField]
    private WireActionScript wireActionScript;

    /// <summary>
    /// �v���C���[�̓������Ǘ�����X�N���v�g
    /// </summary>
    [SerializeField]
    private PlayerMove playerMove;

    public Transform player;

    /// <summary>
    /// �v���C���[����̉������̃I�t�Z�b�g
    /// </summary>
    public float horizontalOffset = 2f;

    /// <summary>
    /// �X���[�W���O�̎���
    /// (�����傫���قǂ������ړ�)
    /// </summary>
    public float smoothTime = 0.5f;

    /// <summary>
    /// SmoothDamp�p
    /// </summary>
    private Vector3 velocity = Vector3.zero;

    /// <summary>
    /// �O�t���[����isConnected�̏��
    /// </summary>
    private bool wasConnected = false;

    /// <summary>
    /// �x���̎�������(�b)
    /// (�����傫���قǒx�����Ԃ������Ȃ�)
    /// </summary>
    private float delayDuration = 0.3f;

    /// <summary>
    /// �x���^�C�}�[(0�����Ȃ疢�g�p)
    /// </summary>
    private float delayTimer = -1f; 

    void LateUpdate()
    {
        if (player == null || wireActionScript == null || playerMove == null) return;

        bool isConnected = wireActionScript.IsConnected;
        bool isGrounded = playerMove.IsGrounded;

        Vector3 currentPos = transform.position;

        // ���C���[�ؒf����̏���(�x���X�^�[�g)
        if (wasConnected && !isConnected)
        {
            delayTimer = delayDuration;
        }

        // �^�[�Q�b�g�ʒu������
        float targetX;
        float targetY;

        // ���C���[�g�p��
        if (isConnected)
        {
            // ���C���[�ݒu�ꏊ�̍��W�������Ă���
            Vector2 wirePos = wireActionScript.HookedPosition;

            targetX = wirePos.x;
            targetY = wirePos.y-1;
        }
        // ���C���[�s�g�p��
        else
        {
            if (!isGrounded)
            {

            }
            // �x���^�C�}�[���Ȃ炻�̏�ŐÎ~
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;

                // �x�����̓J�����ړ��X�L�b�v
                wasConnected = isConnected;
                return;
            }

            targetX = player.position.x + horizontalOffset;
            targetY = player.position.y + 1;
        }

        // ���݂̃J�����ʒu����Ƀ^�[�Q�b�g�ʒu�փX���[�Y�Ɉړ�
        Vector3 targetPos = new Vector3(targetX, targetY, currentPos.z);

        transform.position = Vector3.SmoothDamp(currentPos, targetPos, ref velocity, smoothTime);

        // ���݂̏�Ԃ�ۑ�
        wasConnected = isConnected;
    }
}

//�n�ʂɂ����玞�ԂɊւ�炸�ړ�������