using UnityEditor;
using UnityEngine;

public class BackGroundScripts : MonoBehaviour
{
    public int spriteCount = 1; //Insprctor�������͂ł��܂�

    float rigthOffset = 1.6f; //���������Ă�������
    float leftOffset = -0.6f; //���������Ă�������

    Transform bgTfm;
    SpriteRenderer mySpriteRndr;
    float wigth;

    void Start()
    {
        bgTfm = transform;
        mySpriteRndr = GetComponent<SpriteRenderer>();
        wigth = mySpriteRndr.bounds.size.x;
    }

    void Update()
    {
        //���W����
        Vector3 myViewport = Camera.main.WorldToViewportPoint(bgTfm.position);

        //�w�i�̉�荞��(�J������X���v���X�����Ɉړ���)
        if (myViewport.x < leftOffset)
        {
            bgTfm.position += Vector3.right * (wigth * spriteCount);
        }

        //�w�i�̉�荞��(�J������X���}�C�i�X�����Ɉړ���)
        else if (myViewport.x > rigthOffset)
        {
            bgTfm.position -= Vector3.right * (wigth * spriteCount);
        }
    }
}
