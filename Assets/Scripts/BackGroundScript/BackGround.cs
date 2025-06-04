//ワイヤー切断後の背景移動は
//ワイヤー設置前の位置から再開する

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

    // ワイヤー開始時のプレイヤー座標
    private Vector2 m_wireStartPosition;
    // 直前のワイヤー接続状態
    private bool m_wasConnected = false;

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

        bool isConnected = wireActionScript.IsConnected;

        // ワイヤーが新たに接続された瞬間にプレイヤーの位置を記録する
        if (isConnected && !m_wasConnected)
        {
            m_wireStartPosition = playerTransform.position;
        }

        // ワイヤーが接続されていないときに背景を動かす
        if (!isConnected)
        {
            // 差分を計算（現在位置 - ワイヤー開始時の位置）
            Vector2 delta = (Vector2)playerTransform.position - m_wireStartPosition;

            // 差分にオフセットスピードを掛けてリピート（0〜1）
            float x = Mathf.Repeat(delta.x * m_offsetSpeed.x, k_maxLength);
            float y = Mathf.Repeat(delta.y * m_offsetSpeed.y, k_maxLength);

            var offset = new Vector2(x, y);
            m_copiedMaterial.SetTextureOffset(k_propName, offset);
        }

        // 次フレーム用に接続状態を保持
        m_wasConnected = isConnected;
    }

    private void OnDestroy()
    {
        // ゲームオブジェクト破壊時にマテリアルのコピーも消しておく
        Destroy(m_copiedMaterial);
        m_copiedMaterial = null;
    }
}