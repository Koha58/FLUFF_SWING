using UnityEngine;

public class CameraManager : MonoBehaviour
{
    /*GameObject playerObj;
    PlayerMove  player;
    Transform playerTransform;
    */

    private Transform target;
    public GameObject Player; //Inspector����擾���Ă�������


    void Start()
    {
       /* playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj.GetComponent<PlayerMove>();
        playerTransform = playerObj.transform;
       */

        //�ϐ���Player�I�u�W�F�N�g��transform�R���|�[�l���g����
        target = Player.transform;
    }
    void LateUpdate()
    {
        //MoveCamera();

        //�J������x���W��Player�I�u�W�F�N�g��x���W����擾y���W��z���W�͌��݂̏�Ԃ��ێ�
        transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
    }
    /*void MoveCamera()
    {
        //�����������Ǐ]
        transform.position = new Vector3(playerTransform.position.x, transform.position.y, transform.position.z);
    }*/
}
