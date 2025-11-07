using UnityEngine;

public class IgnoreParentMovement : MonoBehaviour
{
    private Vector3 worldPosition;

    void Start()
    {
        // 最初のワールド座標を記録
        worldPosition = transform.position;
    }

    void LateUpdate()
    {
        // 親の移動に関係なくワールド座標を維持
        transform.position = worldPosition;
    }
}
