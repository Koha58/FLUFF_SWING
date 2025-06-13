using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackGroundMover : MonoBehaviour
{
    private const float k_maxLength = 1f;
    private const string k_propName = "_MainTex";

    // ワイヤーアクションの状態（接続状態など）を管理するスクリプト
    [SerializeField]
    private WireActionScript wireActionScript;

    [SerializeField]
    private Transform playerTransform;

    // 背景をスクロールさせる速さ(数値が大きいほど早く移動)
    [SerializeField]
    private Vector2 m_offsetSpeed = new Vector2(0.01f, 0f);

    // 加減速にかける時間（秒）
    [SerializeField]
    private float acceltionTime = 0.3f;

    private Vector2 currentOffset = Vector2.zero;

    // マテリアル複製用
    private Material m_copiedMaterial;

    // 現在のスクロール速度（Lerpで使用）
    private Vector2 scrollVelocity = Vector2.zero;

    private void Start()
    {
        var image = GetComponent<Image>();

        // マテリアルの複製を作成して使用
        m_copiedMaterial = new Material(image.material);
        image.material = m_copiedMaterial;

        // マテリアルがnullだったら例外が出る
        Assert.IsNotNull(m_copiedMaterial);
        Assert.IsNotNull(playerTransform);
    }

    private void Update()
    {
        if (Time.timeScale == 0f || m_copiedMaterial == null) return;

        bool isConnected = wireActionScript.IsConnected;

        // 入力に応じた目標スクロール速度を決定
        Vector2 targetVelocity = Vector2.zero;

        // ワイヤー不使用かつＡ、Ｄキー入力時
        if (!isConnected)
        {
            if (Input.GetKey(KeyCode.A))
            {
                targetVelocity = -m_offsetSpeed;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                targetVelocity = m_offsetSpeed;
            }
        }

        // 現在の速度を目標速度に近づける（滑らかに変化）
        scrollVelocity = Vector2.Lerp(scrollVelocity, targetVelocity, Time.deltaTime / acceltionTime);

        // オフセットを更新
        currentOffset += scrollVelocity * Time.deltaTime;
        currentOffset.x = Mathf.Repeat(currentOffset.x, k_maxLength);

        m_copiedMaterial.SetTextureOffset(k_propName, currentOffset);
    }

    private void OnDestroy()
    {
        // ゲームオブジェクト破壊時にマテリアルのコピーも消しておく
        Destroy(m_copiedMaterial);
        m_copiedMaterial = null;
    }
}