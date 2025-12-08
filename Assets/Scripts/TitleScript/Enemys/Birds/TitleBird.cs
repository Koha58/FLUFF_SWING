using UnityEngine;

public class Bird : MonoBehaviour
{

    void Update()
    {
        // オブジェクトのワールド座標をビューポート座標に変換
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);

        // 画面外（メインカメラの左側）に出たら破棄
        // ビューポート座標のX成分が -0.2f より小さくなったらDestroy
        // 画面左端は 0.0f なので、-0.2f は画面の左側を大きく超えた位置を意味します。
        if (screenPoint.x < -0.2f)
        {
            Destroy(gameObject);
        }
    }
}
