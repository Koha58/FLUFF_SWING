using UnityEngine;

/// <summary>
/// 背景レイヤーの親オブジェクトをカメラのY軸移動に追従させ、上下の視差効果を実現するスクリプト。
/// </summary>
public class ParallaxBackgroundMover : MonoBehaviour
{
    // BackgroundFollowScriptがアタッチされているオブジェクト (Main Cameraなど)
    [Tooltip("追跡対象のカメラのTransform")]
    [SerializeField] private Transform cameraTransform;

    [Header("Y軸の追従率")]
    [Tooltip("0.0なら背景はY軸方向には全く動きません。1.0ならカメラと完全に一致します。")]
    [SerializeField] private float followRatioY = 1.0f; // 垂直視差の度合いを制御

    // 背景オブジェクトの初期Y座標 (スクリプト実行開始時の背景のY座標)
    private float initialY;

    // カメラの初期Y座標 (スクリプト実行開始時のカメラのY座標)
    private float initialCameraY;

    void Start()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transformが設定されていません。背景追従は無効です。");
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

        // 1. カメラが初期位置からどれだけY軸方向に移動したか（変位）を計算
        float deltaY = cameraTransform.position.y - initialCameraY;

        // 2. カメラの変位に followRatioY を乗算し、目的のY座標（targetY）を計算
        // followRatioYが1.0未満の場合、カメラの動きより遅くなり、視差効果が生まれます。
        float targetY = initialY + deltaY * followRatioY;

        // 3. 背景全体のY座標を更新
        Vector3 newPos = transform.position;
        newPos.y = targetY; // 計算された targetY を適用

        transform.position = newPos;
    }
}