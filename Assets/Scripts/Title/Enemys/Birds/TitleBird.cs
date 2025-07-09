using UnityEngine;

public class Bird : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // ��ʊO�ɏo����j��
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPoint.x > 1.2f) // �E�[�𒴂�����Destroy
        {
            Destroy(gameObject);
        }
    }
}
