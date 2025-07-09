using System.Collections;
using UnityEngine;

public class BirdSpawnController : MonoBehaviour
{
    public GameObject birdPrefab; // Inspector��Prefab���w��
    public Transform spawnPoint; // �����o������ʒu�i��̃I�u�W�F�N�g��z�u���Ďw��j
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
