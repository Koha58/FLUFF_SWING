using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] GameObject player;
    void Start()
    {
        Debug.Log(player.transform.position);
    }

    void Update()
    {
        if (player.transform.position.x > 8f && player.transform.position.x < 32.9f)
        {
            transform.position = new Vector3(player.transform.position.x, 0.08f, -10);
        }
    }
}
