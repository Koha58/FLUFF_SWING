using UnityEditor;
using UnityEngine;

public class BackGroundScripts : MonoBehaviour
{
    public int spriteCount = 1; //Insprctorから手入力できます

    float rigthOffset = 1.6f; //微調整してください
    float leftOffset = -0.6f; //微調整してください

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
        //座標交換
        Vector3 myViewport = Camera.main.WorldToViewportPoint(bgTfm.position);

        //背景の回り込み(カメラがX軸プラス方向に移動時)
        if (myViewport.x < leftOffset)
        {
            bgTfm.position += Vector3.right * (wigth * spriteCount);
        }

        //背景の回り込み(カメラがX軸マイナス方向に移動時)
        else if (myViewport.x > rigthOffset)
        {
            bgTfm.position -= Vector3.right * (wigth * spriteCount);
        }
    }
}
