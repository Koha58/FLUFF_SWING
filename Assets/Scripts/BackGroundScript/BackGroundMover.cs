#region 背景３層
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class BackGroundMover : MonoBehaviour
{
    private const float k_maxLength = 1f;
    private const string k_propName = "_MainTex";

    [Header("ワイヤーアクション状態管理")]
    [SerializeField] private WireActionScript wireActionScript;
    [SerializeField] private PlayerMove playerMove; // プレイヤーの移動状態を参照

    [Header("背景レイヤー (奥 → 手前)")]
    [SerializeField] private Image skyLayer;
    [SerializeField] private Image middleLayer;
    [SerializeField] private Image frontLayer;

    [Header("スクロール速度（奥ほど遅く）")]
    [SerializeField] private float skySpeed = 0.005f;
    [SerializeField] private float middleSpeed = 0.01f;
    [SerializeField] private float frontSpeed = 0.03f;

    private Material skyMat, middleMat, frontMat;
    private Vector2 skyOffset, middleOffset, frontOffset;

    private Vector3 previousPlayerPos;

    private void Start()
    {
        // 各レイヤーのマテリアルを複製して独立管理
        skyMat = new Material(skyLayer.material);
        middleMat = new Material(middleLayer.material);
        frontMat = new Material(frontLayer.material);

        skyLayer.material = skyMat;
        middleLayer.material = middleMat;
        frontLayer.material = frontMat;

        Assert.IsNotNull(wireActionScript);
        Assert.IsNotNull(playerMove);
        Assert.IsNotNull(skyLayer);
        Assert.IsNotNull(middleLayer);
        Assert.IsNotNull(frontLayer);

        previousPlayerPos = playerMove.transform.position;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        bool isConnected = wireActionScript.IsConnected;

        // --- ワイヤー使用中は背景を止める ---
        if (isConnected)
            return;

        // --- プレイヤーの実際の移動量を取得 ---
        float deltaX = playerMove.transform.position.x - previousPlayerPos.x;
        bool isActuallyMoving = Mathf.Abs(deltaX) > 0.0001f;

        // --- 入力方向 ---
        float moveDir = 0f;
        if (Input.GetKey(KeyCode.A))
            moveDir = 0.01f;
        else if (Input.GetKey(KeyCode.D))
            moveDir = -0.01f;

        // --- 入力があっても移動していなければ背景は動かさない ---
        if (!isActuallyMoving || moveDir == 0f)
        {
            previousPlayerPos = playerMove.transform.position;
            return;
        }

        // --- 背景をプレイヤーの移動方向と逆に動かす ---
        skyOffset.x -= moveDir * skySpeed;
        middleOffset.x -= moveDir * middleSpeed;
        frontOffset.x -= moveDir * frontSpeed;

        // --- ループ処理 ---
        skyOffset.x = Mathf.Repeat(skyOffset.x, k_maxLength);
        middleOffset.x = Mathf.Repeat(middleOffset.x, k_maxLength);
        frontOffset.x = Mathf.Repeat(frontOffset.x, k_maxLength);

        // --- 反映 ---
        skyMat.SetTextureOffset(k_propName, skyOffset);
        middleMat.SetTextureOffset(k_propName, middleOffset);
        frontMat.SetTextureOffset(k_propName, frontOffset);

        previousPlayerPos = playerMove.transform.position;
    }

    private void OnDestroy()
    {
        Destroy(skyMat);
        Destroy(middleMat);
        Destroy(frontMat);
    }
}
#endregion

#region 背景１層
//using UnityEngine;
//using UnityEngine.Assertions;
//using UnityEngine.InputSystem.XR.Haptics;
//using UnityEngine.UI;

//[RequireComponent(typeof(Image))]
//public class BackGroundMover : MonoBehaviour
//{
//    private const float k_maxLength = 1f;
//    private const string k_propName = "_MainTex";

//    // ワイヤーアクションの状態（接続状態など）を管理するスクリプト
//    [SerializeField]
//    private WireActionScript wireActionScript;

//    [SerializeField]
//    private Transform playerTransform;

//    // 背景をスクロールさせる速さ(数値が大きいほど早く移動)
//    private Vector2 m_offsetSpeed = new Vector2(0.01f, 0f);

//    // 加減速にかける時間（秒）
//    private float acceltionTime = 0.3f;

//    private Vector2 currentOffset = Vector2.zero;

//    // マテリアル複製用
//    private Material m_copiedMaterial;

//    // 現在のスクロール速度（Lerpで使用）
//    private Vector2 scrollVelocity = Vector2.zero;

//    private void Start()
//    {
//        var image = GetComponent<Image>();

//        // マテリアルの複製を作成して使用
//        m_copiedMaterial = new Material(image.material);
//        image.material = m_copiedMaterial;

//        // マテリアルがnullだったら例外が出る
//        Assert.IsNotNull(m_copiedMaterial);
//        Assert.IsNotNull(playerTransform);
//    }

//    private void Update()
//    {
//        if (Time.timeScale == 0f || m_copiedMaterial == null) return;

//        bool isConnected = wireActionScript.IsConnected;

//        // 入力に応じた目標スクロール速度を決定
//        Vector2 targetVelocity = Vector2.zero;

//        // ワイヤー不使用かつＡ、Ｄキー入力時
//        if (!isConnected)
//        {
//            if (Input.GetKey(KeyCode.A))
//            {
//                targetVelocity = -m_offsetSpeed;
//            }
//            else if (Input.GetKey(KeyCode.D))
//            {
//                targetVelocity = m_offsetSpeed;
//            }
//        }

//        // 現在の速度を目標速度に近づける（滑らかに変化）
//        scrollVelocity = Vector2.Lerp(scrollVelocity, targetVelocity, Time.deltaTime / acceltionTime);

//        // オフセットを更新
//        currentOffset += scrollVelocity * Time.deltaTime;
//        currentOffset.x = Mathf.Repeat(currentOffset.x, k_maxLength);

//        m_copiedMaterial.SetTextureOffset(k_propName, currentOffset);
//    }

//    private void OnDestroy()
//    {
//        // ゲームオブジェクト破壊時にマテリアルのコピーも消しておく
//        Destroy(m_copiedMaterial);
//        m_copiedMaterial = null;
//    }
//}
#endregion