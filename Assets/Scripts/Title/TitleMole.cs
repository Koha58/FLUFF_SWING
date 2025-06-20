using UnityEngine;

public class Mole : MonoBehaviour
{
    private Animator animator;

    public float lifeTime = 2f; // �\������鎞��

    public MoleSpawnPoint mySpawnPoint;


    void Start()
    {
        {
            GetComponent<Animator>().Play("TitleMole");


            // ��莞�Ԍ�Ɏ����ŏ�����
            Destroy(gameObject, lifeTime);
        }
    }
    void OnDestroy()
    {
        if (mySpawnPoint != null)
        {
            mySpawnPoint.isOccupied = false; // �j�����ɊJ��
        }
    }
}
