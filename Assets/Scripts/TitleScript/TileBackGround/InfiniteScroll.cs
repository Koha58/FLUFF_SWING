using UnityEngine;

public class InfiniteScroll : MonoBehaviour
{
    public Transform layerParent; // Layer親オブジェクト
    private float spriteWidth; // スプライトの幅
    private Camera mainCamera;
    private Vector3 lastCameraPosition; // 前フレームのカメラの位置
    private float moveVolume;

    void Start()
    {
        mainCamera = Camera.main;
        spriteWidth = layerParent.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x;
        moveVolume = spriteWidth / (mainCamera.orthographicSize * 2 * mainCamera.aspect);

        lastCameraPosition = mainCamera.transform.position;
    }

    void Update()
    {
        Vector3 cameraMoveDelta = mainCamera.transform.position - lastCameraPosition;

        // カメラが移動したかをチェック
        bool isCameraMovingRight = cameraMoveDelta.x > 0;
        bool isCameraMovingLeft = cameraMoveDelta.x < 0;

        // カメラが右に移動している場合、一番左のスプライトをチェックして必要に応じて移動
        if (isCameraMovingRight)
        {
            Transform leftMostSprite = GetLeftMostSprite();
            float leftMostPosition = leftMostSprite.position.x;
            if (mainCamera.WorldToViewportPoint(new Vector3(leftMostPosition, 0, 0)).x < -moveVolume) // ビューポートの外にあるか
            {
                // 一番右のスプライトの右に移動
                Transform rightMostSprite = GetRightMostSprite();
                leftMostSprite.position = new Vector3(rightMostSprite.position.x + spriteWidth, leftMostSprite.position.y, leftMostSprite.position.z);
            }
        }
        // カメラが左に移動している場合、一番右のスプライトをチェックして必要に応じて移動
        else if (isCameraMovingLeft)
        {
            Transform rightMostSprite = GetRightMostSprite();
            float rightMostPosition = rightMostSprite.position.x;
            if (mainCamera.WorldToViewportPoint(new Vector3(rightMostPosition, 0, 0)).x > moveVolume) // ビューポートの外にあるか
            {
                // 一番左のスプライトの左に移動
                Transform leftMostSprite = GetLeftMostSprite();
                rightMostSprite.position = new Vector3(leftMostSprite.position.x - spriteWidth, rightMostSprite.position.y, rightMostSprite.position.z);
            }
        }

        lastCameraPosition = mainCamera.transform.position; // カメラ位置を更新
    }

    Transform GetLeftMostSprite()
    {
        Transform leftMost = null;
        float leftMostPositionX = float.MaxValue;

        foreach (Transform sprite in layerParent)
        {
            if (sprite.position.x < leftMostPositionX)
            {
                leftMost = sprite;
                leftMostPositionX = sprite.position.x;
            }
        }

        return leftMost;
    }

    Transform GetRightMostSprite()
    {
        Transform rightMost = null;
        float rightMostPositionX = float.MinValue;

        foreach (Transform sprite in layerParent)
        {
            if (sprite.position.x > rightMostPositionX)
            {
                rightMost = sprite;
                rightMostPositionX = sprite.position.x;
            }
        }

        return rightMost;
    }
}