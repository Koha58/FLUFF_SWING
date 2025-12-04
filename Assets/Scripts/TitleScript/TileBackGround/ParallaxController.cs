using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public int layerNumber; // レイヤー番号
    public Transform layerTransform; // このレイヤーのTransform
    public float relativeMoveSpeed; // カメラの移動と比べた相対的移動速度
}

public class ParallaxController : MonoBehaviour
{
    [SerializeField] private List<ParallaxLayer> layers;
    private Camera mainCamera;
    private Vector3 lastCameraPosition;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("ParallaxController: MainCamera not found.");
            return;
        }

        // AutoScroll コンポーネントの存在を確認
        if (!mainCamera.GetComponent<TitleCameraScroll>() || !mainCamera.GetComponent<TitleCameraScroll>())
        {
            Debug.LogWarning("ParallaxController: CameraAutoScroll or TitleCameraScroll component is not attached to MainCamera.");
        }

        lastCameraPosition = mainCamera.transform.position;
    }

    void Update()
    {
        Vector3 deltaMovement = mainCamera.transform.position - lastCameraPosition;
        foreach (var layer in layers)
        {
            float parallaxFactor = deltaMovement.x * layer.relativeMoveSpeed;
            layer.layerTransform.Translate(Vector3.right * parallaxFactor);
        }
        lastCameraPosition = mainCamera.transform.position;
    }
}