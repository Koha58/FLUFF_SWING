using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackGroundScript : MonoBehaviour
{
    private const float k_maxLength = 1f;
    private const string k_propName = "_MainTex";

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
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

    }

    private void OnDestroy()
    {
        // ゲームオブジェクト破壊時にマテリアルのコピーも消しておく
        Destroy(m_copiedMaterial);
        m_copiedMaterial = null;
    }
}