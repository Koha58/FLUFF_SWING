using System.Collections.Generic;
using System.Collections; 
using UnityEngine;

public class BirdSpawnController : MonoBehaviour
{
    public GameObject birdPrefab;
    public float spawnInterval = 2f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnBird();
        }
    }

    void SpawnBird()
    {
        BirdSpawnPoint[] allPoints = Object.FindObjectsByType<BirdSpawnPoint>(FindObjectsSortMode.None);

        List<BirdSpawnPoint> visiblePoints = new List<BirdSpawnPoint>();

        foreach (var point in allPoints)
        {
            if (point == null) continue; // DestroyÏ‚Ý‘Îô

            Vector3 screenPoint = mainCamera.WorldToViewportPoint(point.transform.position);
            bool inCamera = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            if (inCamera)
            {
                visiblePoints.Add(point);
            }
        }

        if (visiblePoints.Count > 0)
        {
            var selected = visiblePoints[Random.Range(0, visiblePoints.Count)];
            Instantiate(birdPrefab, selected.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("‰æ–Ê“à‚ÉBirdSpawnPoint‚ª‚ ‚è‚Ü‚¹‚ñ");
        }
    }
}
