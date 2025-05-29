using Unity.VisualScripting;
using UnityEngine;

public class Kitty : MonoBehaviour
{
    private float SPEED = 0.1f;
    private float JUMP = 2f;
    private int Ground = 0;
    void Update()
    {
        Vector2 position = transform.position;
        if (Input.GetKey(KeyCode.D))
        {
            position.x += SPEED;
        }
        if (Input.GetKey(KeyCode.A))
        {
            position.x -= SPEED;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (Ground == 0)
            {
                position.y += JUMP;
            }
        }

        transform.position = position;
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "Ground")
        {
            Ground = 0;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Ground") 
        {
            Ground = 1;
        }
    }
}
