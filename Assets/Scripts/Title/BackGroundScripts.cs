using UnityEngine;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour
{
    public GameObject backgroundPrefab;     // 背景プレハブ
    public Transform player;                // プレイヤー（右に進む）
    public float backgroundWidth = 20f;     // 背景画像の横幅（Unity単位）
    public int preloadCount = 3;            // 初期に表示しておく枚数
    public float deleteDistance = 30f;      // カメラからこれ以上離れたら削除

    private List<GameObject> backgrounds = new List<GameObject>();
    private float nextSpawnX = 0f;

    void Start()
    {
        for (int i = 0; i < preloadCount; i++)
        {
            SpawnBackground();
        }
    }

    void Update()
    {
        // プレイヤーが次の背景位置に近づいたら追加
        if (player.position.x + backgroundWidth * preloadCount > nextSpawnX)
        {
            SpawnBackground();
        }

        // 背景削除処理
        for (int i = backgrounds.Count - 1; i >= 0; i--)
        {
            if (player.position.x - backgrounds[i].transform.position.x > deleteDistance)
            {
                Destroy(backgrounds[i]);
                backgrounds.RemoveAt(i);
            }
        }
    }

    void SpawnBackground()
    {
        // プレハブと同じY座標を使うために、最初の背景の高さを記録して使うのがベスト
        float backgroundY = backgroundPrefab.transform.position.y;  // ← ここが重要！

        Vector3 spawnPos = new Vector3(nextSpawnX, backgroundY, 0f);
        GameObject bg = Instantiate(backgroundPrefab, spawnPos, Quaternion.identity);
        backgrounds.Add(bg);
        nextSpawnX += backgroundWidth;
    }
}
