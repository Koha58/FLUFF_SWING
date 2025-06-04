using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Vector3 _initPos;

    private GameObject player;
    void Start()
    {
        _initPos = transform.position;
        player = GameObject.Find("player_0");

    }

    void Update()
    {
        _FollowPlayer();
    }
     private void _FollowPlayer()
    {
        float playerX = player.transform.position.x;
        float Clampedx = Mathf.Clamp(playerX, _initPos.x, playerX);
        transform.position = new Vector3(Clampedx, transform.position.y, transform.position.z);
    }
}
