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

    [SerializeField]
    private Vector2 m_offsetSpeed;

    [SerializeField]
    private Transform playerTransform; // プレイヤーのTransform

    private Material m_copiedMaterial;

    private void Start()
    {
        var image = GetComponent<Image>();
        // マテリアルの複製を作成して使用
        m_copiedMaterial = new Material(image.material);
        image.material = m_copiedMaterial;

        // マテリアルがnullだったら例外が出ます。
        Assert.IsNotNull(m_copiedMaterial);
        Assert.IsNotNull(playerTransform);
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        // xとyの値が0 ～ 1でリピートするようにする
        var x = Mathf.Repeat(playerTransform.position.x * m_offsetSpeed.x, k_maxLength);
        var y = Mathf.Repeat(playerTransform.position.y * m_offsetSpeed.y, k_maxLength);

        // プレイヤーの移動に合わせる
        // プレイヤーがワイヤー移動中は背景は動かさない
        // ワイヤー切断後の座標に背景が引っ張られるのを改善する
        if (!wireActionScript.IsConnected)
        {
            var offset = new Vector2(x, y);
            m_copiedMaterial.SetTextureOffset(k_propName, offset);
        }
    }

    private void OnDestroy()
    {
        // ゲームオブジェクト破壊時にマテリアルのコピーも消しておく
        Destroy(m_copiedMaterial);
        m_copiedMaterial = null;
    }
}