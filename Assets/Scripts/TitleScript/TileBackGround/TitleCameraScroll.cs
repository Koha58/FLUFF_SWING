using UnityEngine;

public class TitleCameraScroll : MonoBehaviour
{
    // スクロール方向の列挙型
    public enum ScrollDirection
    {
        Right,
        Left,
        Up,
        Down
    }

    [SerializeField]
    private ScrollDirection scrollDirection = ScrollDirection.Right; // スクロール方向（デフォルトは右）

    [SerializeField]
    private float scrollSpeed = 1f; // スクロール速度（ユニット/秒）

    void Update()
    {
        // スクロール方向に基づいてカメラを移動
        Vector3 moveDirection = Vector3.zero;
        switch (scrollDirection)
        {
            case ScrollDirection.Right:
                moveDirection = Vector3.right;
                break;
            case ScrollDirection.Left:
                moveDirection = Vector3.left;
                break;
            case ScrollDirection.Up:
                moveDirection = Vector3.up;
                break;
            case ScrollDirection.Down:
                moveDirection = Vector3.down;
                break;
        }
        transform.Translate(moveDirection * scrollSpeed * Time.deltaTime);
    }
}