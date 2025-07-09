using System.Collections;
using UnityEngine;

public class BirdSpawnController : MonoBehaviour
{
    public GameObject birdPrefab; // InspectorでPrefabを指定
    public Transform spawnPoint; // 鳥が出現する位置（空のオブジェクトを配置して指定）
    public float spawnInterval = 5f;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnBird();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnBird()
    {
        Instantiate(birdPrefab, spawnPoint.position, Quaternion.identity);
    }
}
