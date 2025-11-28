using UnityEngine;

/// <summary>
/// 背景レイヤーの親オブジェクトをカメラのY軸移動に追従させ、上下の視差効果を実現します。
/// 【機能】背景オブジェクトのY座標をワールド座標でクランプ（制限）します。
/// </summary>
public class ParallaxBackgroundMover : MonoBehaviour
{
    // BackgroundFollowScriptがアタッチされているオブジェクト (Main Cameraなど)
    [Tooltip("追跡対象のカメラのTransform")]
    [SerializeField] private Transform cameraTransform;

    [Header("Y軸の追従率")]
    [Tooltip("0.0なら背景はY軸方向には全く動きません。1.0ならカメラと完全に一致します。")]
    [SerializeField] private float followRatioY = 1.0f;

    [Header("ワールド座標クランプ設定")]
    [Tooltip("背景オブジェクトのY座標の下限。この値より下には移動しません。")]
    [SerializeField] private float minWorldY;
    [Tooltip("背景オブジェクトのY座標の上限。この値より上には移動しません。")]
    [SerializeField] private float maxWorldY;

    // 背景オブジェクトの初期Y座標
    private float initialY;

    // カメラの初期Y座標
    private float initialCameraY;

    void Start()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transformが設定されていません。");
            enabled = false;
            return;
        }

        // 初期位置を保存
        initialY = transform.position.y;
        initialCameraY = cameraTransform.position.y;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 1. カメラが初期位置からどれだけY軸方向に移動したか
        float deltaY = cameraTransform.position.y - initialCameraY;

        // 2. カメラの移動量に followRatioY を乗算し、目的のY座標（targetY）を計算
        // これが視差効果を生み出す
        float targetY = initialY + deltaY * followRatioY;

        // 3. 【クランプ適用】計算された目的座標を、設定された最小値と最大値の間に制限
        float clampedY = Mathf.Clamp(targetY, minWorldY, maxWorldY);

        // 4. 背景全体のY座標を更新
        Vector3 newPos = transform.position;
        newPos.y = clampedY; // クランプされたY座標を適用

        transform.position = newPos;
    }
}