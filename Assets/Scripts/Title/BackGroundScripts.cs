using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public GameObject backgroundPrefab;  // ”wŒiƒvƒŒƒnƒu
    public Transform player;             // ƒvƒŒƒCƒ„[
    public int preloadCount = 2;         // –‘O‚É¶¬‚µ‚Ä‚¨‚­”
    public float backgroundWidth = 20f;  // ”wŒi‰æ‘œ‚Ì•
    public float deleteDistance = 30f;   // ˆê’è‹——£‚ğ‰ß‚¬‚½”wŒi‚Ííœ

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
        // ƒvƒŒƒCƒ„[‚ªˆê’è‹——£i‚ñ‚¾‚ç”wŒi‚ğ¶¬
        if (player.position.x + backgroundWidth * preloadCount > nextSpawnX)
        {
            SpawnBackground();
        }

        // ˆê’è‹——£‚ğ‰ß‚¬‚½”wŒi‚ğíœ
        for (int i = backgrounds.Count - 1; i >= 0; i--)
        {
            if (player.position.x > backgrounds[i].transform.position.x + deleteDistance)
            {
                Destroy(backgrounds[i]);
                backgrounds.RemoveAt(i);
            }
        }
    }

    void SpawnBackground()
    {
        // nextSpawnX‚ÌˆÊ’u‚É”wŒi‚ğ¶¬‚·‚éˆ—
        float backgroundY = backgroundPrefab.transform.position.y; // Œ³‚Ì”wŒi‚Æ“¯‚¶‚‚³
        Vector3 spawnPos = new Vector3(nextSpawnX, backgroundY, 0f);
        GameObject bg = Instantiate(backgroundPrefab, spawnPos, Quaternion.identity);
        backgrounds.Add(bg);
        nextSpawnX += backgroundWidth;
    }
}
