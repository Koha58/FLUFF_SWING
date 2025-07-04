using UnityEngine;

public class Mole : MonoBehaviour
{

    public float lifeTime = 2f; // �\������鎞��

    public MoleSpawnPoint mySpawnPoint;

    private bool hasStarted = false;


    void Start()
    {

        // Animator�͍ŏ��͖��������Ă����AUpdate�ŉ�ʓ��ɓ�������Đ�����
        GetComponent<Animator>().enabled = false;

    }
    void OnDestroy()
    {
        if (mySpawnPoint != null)
        {
            mySpawnPoint.isOccupied = false; // �j�����ɊJ��
        }
    }

    private void Update()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if (onScreen && !GetComponent<Animator>().enabled)
        {
            var animator = GetComponent<Animator>();
            animator.enabled = true;
            animator.Play("TitleMole"); // �A�j���[�V�������𐳊m��

            // Destroy �������ŌĂԂ��ƂŁA��ʂɓ����Ă���lifeTime�b��ɏ�����
            Destroy(gameObject, lifeTime);
        }
    }
}
