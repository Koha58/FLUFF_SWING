using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MoneSpawner : MonoBehaviour
{
    public GameObject molePrefad;
    public float spawnInterval = 5f;
    public Tilemap tilemap;
    public TileBase moleSpawnTile;  //特定のタイルで出現ポイントを判別

    private List<Vector3> spawnPositions = new List<Vector3>();
    void Start()
    {
        FindSpawnPosition();
        StartCoroutine(SpawnMoleRoutine());
    }

    void FindSpawnPosition()
    {
        spawnPositions.Clear();
        BoundsInt bounds = tilemap.cellBounds;
        for(int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for(int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);
                if (tile == moleSpawnTile)
                {
                    Vector3 worldPos = tilemap.CellToWorld(pos) + tilemap.tileAnchor;
                    spawnPositions.Add(worldPos);
                }
            }
        }
    }

    IEnumerator SpawnMoleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (spawnPositions.Count > 0)
            {
                Vector3 spawnPos = spawnPositions[Random.Range(0, spawnPositions.Count)];
                Instantiate(molePrefad, spawnPos, Quaternion.identity);
            }
        }
    }
}
