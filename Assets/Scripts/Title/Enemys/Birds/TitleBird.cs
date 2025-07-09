using UnityEngine;

public class Bird : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // 画面外に出たら破棄
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPoint.x > 1.2f) // 右端を超えたらDestroy
        {
            Destroy(gameObject);
        }
    }
}
