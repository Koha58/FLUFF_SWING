using UnityEngine;

public class Mole : MonoBehaviour
{

    public float lifeTime = 2f; // �\������鎞��

    public MoleSpawnPoint mySpawnPoint;


    void Start()
    {
        
            GetComponent<Animator>().Play("TitleMole");


            // ��莞�Ԍ�Ɏ����ŏ�����
            Destroy(gameObject, lifeTime);
        
    }
    public void Disappear()
    {
        if (mySpawnPoint != null)
        {
            mySpawnPoint.isOccupied = false; // �j�����ɊJ��
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if (onScreen && !GetComponent<Animator>().enabled)
        {
            GetComponent<Animator>().enabled = true; // �J�����ɓ������u�ԃA�j���Đ��J�n
        }
    }
}
