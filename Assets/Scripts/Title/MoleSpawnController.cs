using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoleSpawnController : MonoBehaviour
{
    public GameObject molePrefab;
    public float spawnInterval = 3f;
    public float cameraAheadOffset = 5f; // カメラより少し前でも出していい距離

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
            TrySpawnMoleInCameraView();
        }
    }

    void TrySpawnMoleInCameraView()
    {
        // カメラのビュー内のRectを取得
        Vector3 camPos = mainCamera.transform.position;
        float height = 2f * mainCamera.orthographicSize;
        float width = height * mainCamera.aspect;

        // カメラ前方を含めた矩形領域
        Rect spawnRect = new Rect(
            camPos.x - width / 2f,
            camPos.y - height / 2f,
            width + cameraAheadOffset,
            height
        );

        // 画面内にあるTilemapたちを調べる
        MoleSpawnPoint[] allSpawnPoints = Object.FindObjectsByType<MoleSpawnPoint>(FindObjectsSortMode.None);
        List<MoleSpawnPoint> visiblePoints = new List<MoleSpawnPoint>();

        foreach (var point in allSpawnPoints)
        {
            Vector3 pos = point.transform.position;
            if (spawnRect.Contains(new Vector2(pos.x, pos.y)))
            {
                visiblePoints.Add(point);
            }
        }

        if (visiblePoints.Count > 0)
        {
            MoleSpawnPoint selected = visiblePoints[Random.Range(0, visiblePoints.Count)];
            Instantiate(molePrefab, selected.transform.position, Quaternion.identity);
        }
    }
}
   
    