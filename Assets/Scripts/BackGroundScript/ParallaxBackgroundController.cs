using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

/// <summary>
/// カメラの移動に基づいて、背景レイヤーを視差（パララックス）スクロールさせるスクリプト。
/// 【変更点】Y軸のUVスクロール（上下移動）を削除し、X軸のループスクロールのみを処理します。
/// </summary>
public class ParallaxBackgroundController : MonoBehaviour
{
    private const float k_maxLength = 1f;
    private const string k_propName = "_MainTex";

    [Header("追従対象と状態管理")]
    [Tooltip("追跡対象のカメラ/背景親のTransform")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("ワイヤーアクションの状態をチェックするために必要")]
    [SerializeField] private WireActionScript wireActionScript;

    [Header("背景レイヤー (奥 → 手前)")]
    [SerializeField] private Image skyLayer;
    [SerializeField] private Image middleLayer;
    [SerializeField] private Image frontLayer;

    [Header("スクロール調整")]
    [Tooltip("Unityのワールド単位をUVオフセット単位に変換する係数。値を小さくするほど、背景の移動速度が遅くなります。")]
    [SerializeField] private float scrollScale = 0.01f;

    [Header("スクロール率（奥ほど小さい値。0.01〜0.5程度の範囲推奨）")]
    // X軸の移動に対する背景の追従率 (Y軸の比率変数は不要になりましたが、互換性のために残します)
    [SerializeField] private float skyRatioX = 0.05f;
    [SerializeField] private float middleRatioX = 0.2f;
    [SerializeField] private float frontRatioX = 0.5f;

    // Y軸のRatioは使用されなくなります
    [SerializeField] private float skyRatioY = 0.01f;
    [SerializeField] private float middleRatioY = 0.1f;
    [SerializeField] private float frontRatioY = 0.5f;

    private Material skyMat, middleMat, frontMat;
    // Inspectorで確認しやすいように [SerializeField] を残します
    [SerializeField] private Vector2 skyOffset, middleOffset, frontOffset;
    // 追跡対象（カメラ）の前フレームの座標を保持
    private Vector3 previousTargetPos;

    private void Start()
    {
        // アサーション（必要なコンポーネントが設定されているか確認）
        Assert.IsNotNull(cameraTransform);
        Assert.IsNotNull(wireActionScript);
        Assert.IsNotNull(skyLayer);
        Assert.IsNotNull(middleLayer);
        Assert.IsNotNull(frontLayer);

        // 各レイヤーのマテリアルを複製して独立管理
        skyMat = new Material(skyLayer.material);
        middleMat = new Material(middleLayer.material);
        frontMat = new Material(frontLayer.material);

        skyLayer.material = skyMat;
        middleLayer.material = middleMat;
        frontLayer.material = frontMat;

        // 初期位置を保存 (追跡対象をカメラに)
        previousTargetPos = cameraTransform.position;
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f || cameraTransform == null || wireActionScript == null) return;

        // --- ワイヤー使用中は背景を停止 ---
        if (wireActionScript.IsConnected)
        {
            previousTargetPos = cameraTransform.position;
            return;
        }

        Vector3 currentTargetPos = cameraTransform.position;
        // カメラの実際の移動量を取得
        Vector3 deltaPos = currentTargetPos - previousTargetPos;

        // 移動がなければ処理をスキップ (微細な移動は無視)
        if (deltaPos.sqrMagnitude < 0.00001f)
        {
            previousTargetPos = currentTargetPos;
            return;
        }

        // --- 背景のオフセットを更新 ---
        // X軸のスクロールのみを実行
        UpdateOffset(ref skyOffset, deltaPos, skyRatioX, skyRatioY);
        UpdateOffset(ref middleOffset, deltaPos, middleRatioX, middleRatioY);
        UpdateOffset(ref frontOffset, deltaPos, frontRatioX, frontRatioY);

        // --- マテリアルに反映 ---
        skyMat.SetTextureOffset(k_propName, skyOffset);
        middleMat.SetTextureOffset(k_propName, middleOffset);
        frontMat.SetTextureOffset(k_propName, frontOffset);

        // --- 次のフレームのために現在の座標を保存 ---
        previousTargetPos = currentTargetPos;
    }

    /// <summary>
    /// オフセットを更新し、X軸のみループ処理を行います。（Y軸の更新ロジックは削除）
    /// </summary>
    private void UpdateOffset(ref Vector2 offset, Vector3 deltaPos, float ratioX, float ratioY)
    {
        // X軸のみ更新
        offset.x += deltaPos.x * ratioX * scrollScale;

        // Y軸の更新ロジックは削除しました。offset.y はこのメソッドでは更新されません。
        // offset.y += deltaPos.y * ratioY * scrollScale;

        // X軸はループ処理を維持 (横方向の連続描画のため)
        offset.x = Mathf.Repeat(offset.x, k_maxLength);

        // Y軸のループ/クランプ処理は削除しました。
        // offset.y の値は、Startで初期化された後、変化しません。
    }

    private void OnDestroy()
    {
        // 生成したマテリアルをクリーンアップ
        Destroy(skyMat);
        Destroy(middleMat);
        Destroy(frontMat);
    }
}