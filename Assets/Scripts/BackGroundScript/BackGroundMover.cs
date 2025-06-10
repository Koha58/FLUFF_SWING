using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackGroundMover : MonoBehaviour
{
    private const float k_maxLength = 1f;
    private const string k_propName = "_MainTex";

    // ワイヤーアクションの状態（接続状態など）を管理するスクリプト
    [SerializeField]
    private WireActionScript wireActionScript;

    private Vector2 currentOffset = Vector2.zero;

    // 背景をスクロールさせる速さ
    [SerializeField]
    private Vector2 m_offsetSpeed;

    [SerializeField]
    private Transform playerTransform;

    // マテリアル複製用
    private Material m_copiedMaterial;

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
        if (Time.timeScale == 0f)
        {
            return;
        }

        bool isConnected = wireActionScript.IsConnected;

        // ワイヤー不使用かつＡ、Ｄキー入力時
        if (!isConnected)
        {
            if (Input.GetKey(KeyCode.A))
            {
                currentOffset += -m_offsetSpeed * Time.deltaTime;
                currentOffset.x = Mathf.Repeat(currentOffset.x, k_maxLength);
                currentOffset.y = Mathf.Repeat(currentOffset.y, k_maxLength);
                m_copiedMaterial.SetTextureOffset(k_propName, currentOffset);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                currentOffset += m_offsetSpeed * Time.deltaTime;
                currentOffset.x = Mathf.Repeat(currentOffset.x, k_maxLength);
                currentOffset.y = Mathf.Repeat(currentOffset.y, k_maxLength);
                m_copiedMaterial.SetTextureOffset(k_propName, currentOffset);
            }
        }
    }

    private void OnDestroy()
    {
        // ゲームオブジェクト破壊時にマテリアルのコピーも消しておく
        Destroy(m_copiedMaterial);
        m_copiedMaterial = null;
    }
}