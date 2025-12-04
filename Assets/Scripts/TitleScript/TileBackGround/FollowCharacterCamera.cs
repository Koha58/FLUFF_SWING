using UnityEngine;

public class FollowCharacterCamera : MonoBehaviour
{
    [SerializeField] Transform characterTransform; // 追従するキャラクターのTransform
    [SerializeField] float smoothSpeed = 0.125f; // カメラの移動の滑らかさを調整
    private Vector3 offset; // キャラクターとカメラの間のオフセット
    public bool followHorizontal = true; // 横方向に追従するかどうか
    public bool followVertical = false; // 縦方向に追従するかどうか

    void Start()
    {
        // カメラとキャラクターの初期位置の差分をオフセットとして設定
        offset = transform.position - characterTransform.position;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = characterTransform.position + offset;

        // 横方向の追従が有効な場合
        if (followHorizontal)
        {
            transform.position = new Vector3(
                Mathf.Lerp(transform.position.x, targetPosition.x, smoothSpeed),
                transform.position.y,
                transform.position.z
            );
        }

        /* 縦方向の追従が有効な場合
        if (followVertical)
        {
            transform.position = new Vector3(
                transform.position.x,
                Mathf.Lerp(transform.position.y, targetPosition.y, smoothSpeed),
                transform.position.z
            );
        }
        */
    }
}