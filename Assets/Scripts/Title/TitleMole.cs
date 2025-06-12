using UnityEngine;

public class TitleMole : MonoBehaviour
{
    public float lifetime = 2f;
    void Start()
    {
        Destroy(gameObject,lifetime);
    }

}
