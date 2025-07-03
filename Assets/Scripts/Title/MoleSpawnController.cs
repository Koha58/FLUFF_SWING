using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MoleSpawnController : MonoBehaviour
{
    public GameObject molePrefab;
    public float spawnInterval = 3f;
    public float cameraAheadOffset = 15f; // カメラより少し前でも出していい距離

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
            camPos.x - width / 2,
            camPos.y - height / 2,
            width + cameraAheadOffset,
            height
        );

        // 画面内にあるTilemapたちを調べる
        MoleSpawnPoint[] allSpawnPoints = Object.FindObjectsByType<MoleSpawnPoint>(FindObjectsSortMode.None);
        List<MoleSpawnPoint> visiblePoints = new List<MoleSpawnPoint>();

        foreach (var point in allSpawnPoints)
        {
            visiblePoints.Add(point); // 強制的に全部通す
            Vector3 pos = point.transform.position;
            bool isInside = spawnRect.Contains(new Vector2(pos.x, pos.y));
            Debug.Log($"SpawnPoint {point.name}: pos={pos}, inCamera={isInside}");

            if (!point.isOccupied && isInside)
            {
                visiblePoints.Add(point);
            }
        }

        Debug.Log("Visible spawn points: " + visiblePoints.Count);


        if (visiblePoints.Count > 0)
        {
            MoleSpawnPoint selected = visiblePoints[Random.Range(0, visiblePoints.Count)];
            GameObject mole = Instantiate(molePrefab, selected.transform.position, Quaternion.identity);

            // 生成直後はAnimatorを無効化
            Animator anim = mole.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = false;
            }

            // Mole 側で spawnPoint を記憶
            Mole moleScript = mole.GetComponent<Mole>();
            if (moleScript != null)
            {
                moleScript.mySpawnPoint = selected;
                selected.isOccupied = true;
            }
           
        }

    }
}
   
    