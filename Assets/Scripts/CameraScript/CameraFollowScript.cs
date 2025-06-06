using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    public Transform player;

    // �v���C���[����̉������̃I�t�Z�b�g
    public float horizontalOffset = 0f;

    [SerializeField]
    private WireActionScript wireActionScript;

    void LateUpdate()
    {
        if (player == null) return;

        bool isConnected = wireActionScript.IsConnected;

        // ���C���[�s�g�p��
        if (!isConnected)
        {
            horizontalOffset = 3f;

            // �v���C���[�̈ʒu�ɃI�t�Z�b�g���������ʒu�ɃJ�������ړ�
            Vector3 newPosition = transform.position;

            // �v���C���[��x�ʒu + �I�t�Z�b�g�i��F�����ɕ\������Ȃ�}�C�i�X�j
            newPosition.x = player.position.x + horizontalOffset;

            // Y��Z�̓J�����̌��̍������ێ�
            transform.position = new Vector3(newPosition.x, transform.position.y, transform.position.z);
        }
        // ���C���[�g�p��
        else
        {
            horizontalOffset = 3f;

            // �v���C���[�̈ʒu�ɃI�t�Z�b�g���������ʒu�ɃJ�������ړ�
            Vector3 newPosition = transform.position;

            // ���������C���[�̃^�[�Q�b�g�̈ʒu�ɂ���
            //newPosition.x = targetPos.x;
            // �v���C���[��x�ʒu + �I�t�Z�b�g�i��F�����ɕ\������Ȃ�}�C�i�X�j
            newPosition.x = player.position.x + horizontalOffset;

            // Y��Z�̓J�����̌��̍������ێ�
            transform.position = new Vector3(newPosition.x, transform.position.y, transform.position.z);


            //return;
        }
    }
}