using UnityEngine;

public class CameraManager : MonoBehaviour
{
    /*GameObject playerObj;
    PlayerMove  player;
    Transform playerTransform;
    */

    private Transform target;
    public GameObject Player; //Inspectorから取得してください


    void Start()
    {
       /* playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj.GetComponent<PlayerMove>();
        playerTransform = playerObj.transform;
       */

        //変数にPlayerオブジェクトのtransformコンポーネントを代入
        target = Player.transform;
    }
    void LateUpdate()
    {
        //MoveCamera();

        //カメラのx座標をPlayerオブジェクトのx座標から取得y座標とz座標は現在の状態を維持
        transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
    }
    /*void MoveCamera()
    {
        //横方向だけ追従
        transform.position = new Vector3(playerTransform.position.x, transform.position.y, transform.position.z);
    }*/
}
