using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

/// <summary>
/// カメラの移動に基づいて、背景レイヤー（Sky, Middle, Front）を視差（パララックス）スクロールさせるスクリプト。
/// カメラの移動量に応じて各レイヤーのテクスチャUVオフセットを更新し、視差効果とSkyレイヤーのループを実現します。
/// </summary>
public class ParallaxBackgroundController : MonoBehaviour
{
    // =================================================================================
    // 1. ⚙️ 内部設定・定数
    // =================================================================================
    private const float k_maxLength = 1f;       // UV座標の最大値 (0から1でループするため)
    private const string k_propName = "_MainTex"; // マテリアルが参照するテクスチャプロパティ名

    // =================================================================================
    // 2. 📢 公開設定フィールド (Inspectorで設定)
    // =================================================================================

    [Header("追従対象と状態管理")]
    [Tooltip("追跡対象のカメラ/背景親のTransform")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("ワイヤーアクションの状態をチェックするために必要")]
    [SerializeField] private WireActionScript wireActionScript;

    [Header("背景レイヤー (奥 → 手前)")]
    [SerializeField] private Image skyLayer; // 最奥のレイヤー (Y軸ループ対象)
    [SerializeField] private Image middleLayer;
    [SerializeField] private Image frontLayer;

    [Header("スクロール調整")]
    [Tooltip("Unityのワールド単位をUVオフセット単位に変換する係数。値を小さくするほど、背景の移動速度が遅くなります。")]
    [SerializeField] private float scrollScale = 0.01f;

    [Header("スクロール率（奥ほど小さい値。0.01〜0.5程度の範囲推奨）")]
    // X軸の移動に対する背景の追従率 (比率が大きいほどカメラの動きに近く、視差効果が小さくなる)
    [SerializeField] private float skyRatioX = 0.2f;
    [SerializeField] private float middleRatioX = 0.2f;
    [SerializeField] private float frontRatioX = 0.5f;

    // Y軸のRatio (Sky Layerの縦方向のスクロール速度を制御)
    [SerializeField] private float skyRatioY = 0.05f; // 上昇時にゆっくり動かすための値
    [SerializeField] private float middleRatioY = 0.4f;
    [SerializeField] private float frontRatioY = 0.8f;

    // =================================================================================
    // 3. 🛡️ 内部状態変数 (実行時に変化・キャッシュ)
    // =================================================================================

    // マテリアル参照 (Startで複製され、実行中にUVオフセットが変化)
    private Material skyMat, middleMat, frontMat;

    // 各レイヤーのUVオフセット (実行中に値が変化する状態変数)
    private Vector2 skyOffset, middleOffset, frontOffset;

    // 追跡対象（カメラ）の前フレームの座標を保持 (実行時に毎フレーム更新される状態変数)
    private Vector3 previousTargetPos;

    // =================================================================================
    // Unity イベント関数
    // =================================================================================

    private void Start()
    {
        // 必要なコンポーネントが設定されているか確認
        Assert.IsNotNull(cameraTransform);
        Assert.IsNotNull(wireActionScript);
        Assert.IsNotNull(skyLayer);
        Assert.IsNotNull(middleLayer);
        Assert.IsNotNull(frontLayer);

        // 各レイヤーの既存マテリアルを複製して独立管理（他のオブジェクトに影響を与えないため）
        skyMat = new Material(skyLayer.material);
        middleMat = new Material(middleLayer.material);
        frontMat = new Material(frontLayer.material);

        // 複製したマテリアルをImageコンポーネントに適用
        skyLayer.material = skyMat;
        middleLayer.material = middleMat;
        frontLayer.material = frontMat;

        // 初期位置を保存 (追跡対象をカメラに)
        previousTargetPos = cameraTransform.position;
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f || cameraTransform == null || wireActionScript == null) return;

        // --- ワイヤー使用中は背景のスクロールを停止し、位置をリセット ---
        if (wireActionScript.IsConnected)
        {
            // スクロールを再開するために現在の位置を保存して終了
            previousTargetPos = cameraTransform.position;
            return;
        }

        Vector3 currentTargetPos = cameraTransform.position;
        // カメラのフレーム間の実際の移動量（変位）を取得
        Vector3 deltaPos = currentTargetPos - previousTargetPos;

        // 移動がなければ処理をスキップ
        if (deltaPos.sqrMagnitude < 0.00001f)
        {
            previousTargetPos = currentTargetPos;
            return;
        }

        // --- 背景のオフセットを更新 ---
        // Sky Layer: Y軸ループ（isYLooping=true）を適用
        UpdateOffset(ref skyOffset, deltaPos, skyRatioX, skyRatioY, true);
        // Middle Layer: X軸ループのみ適用（isYLooping=false）
        UpdateOffset(ref middleOffset, deltaPos, middleRatioX, middleRatioY, false);
        // Front Layer: X軸ループのみ適用（isYLooping=false）
        UpdateOffset(ref frontOffset, deltaPos, frontRatioX, frontRatioY, false);

        // --- マテリアルに新しいUVオフセットを反映 ---
        skyMat.SetTextureOffset(k_propName, skyOffset);
        middleMat.SetTextureOffset(k_propName, middleOffset);
        frontMat.SetTextureOffset(k_propName, frontOffset);

        // --- 次のフレームのために現在の座標を保存 ---
        previousTargetPos = currentTargetPos;
    }

    private void OnDestroy()
    {
        // シーン終了時などに、Startで生成したマテリアルをクリーンアップ（メモリリーク防止）
        Destroy(skyMat);
        Destroy(middleMat);
        Destroy(frontMat);
    }

    // =================================================================================
    // プライベートメソッド
    // =================================================================================

    /// <summary>
    /// カメラの移動量に応じてUVオフセットを計算し、ループを適用します。
    /// </summary>
    /// <param name="offset">更新するUVオフセット</param>
    /// <param name="deltaPos">カメラの移動量</param>
    /// <param name="ratioX">X軸のスクロール比率</param>
    /// <param name="ratioY">Y軸のスクロール比率</param>
    /// <param name="isYLooping">Y軸方向のループを有効にするか</param>
    private void UpdateOffset(ref Vector2 offset, Vector3 deltaPos, float ratioX, float ratioY, bool isYLooping)
    {
        // X軸の更新とループ処理 (全レイヤー共通)
        offset.x += deltaPos.x * ratioX * scrollScale;
        // Mathf.RepeatでX軸を0〜1の間で繰り返す（横方向のループ）
        offset.x = Mathf.Repeat(offset.x, k_maxLength);

        // Y軸の更新とループ処理 (Sky Layerなど、isYLoopingがtrueの場合のみ)
        if (isYLooping)
        {
            // Y軸を更新
            offset.y += deltaPos.y * ratioY * scrollScale;

            // Y軸もループ処理を適用 (上下方向の連続描画のため)
            // Mathf.RepeatでY軸を0〜1の間で繰り返す
            offset.y = Mathf.Repeat(offset.y, k_maxLength);
        }
    }
}