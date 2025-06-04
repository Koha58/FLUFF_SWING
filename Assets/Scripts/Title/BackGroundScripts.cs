using UnityEditor;
using UnityEngine;

public class BackGroundScripts : MonoBehaviour
{
    public float scrollSpeedX = 0.1f;
    public float scrollSpeedY = 0f;

    private Renderer rend;
    private Vector2 offset;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        offset.x += scrollSpeedX * Time.deltaTime;
        offset.y += scrollSpeedY * Time.deltaTime;
        rend.material.mainTextureOffset = offset;
    }
    
}
