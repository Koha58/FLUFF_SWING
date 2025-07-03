using UnityEngine;

public class Mole : MonoBehaviour
{

    public float lifeTime = 2f; // 表示される時間

    public MoleSpawnPoint mySpawnPoint;


    void Start()
    {
        
            GetComponent<Animator>().Play("TitleMole");


            // 一定時間後に自動で消える
            Destroy(gameObject, lifeTime);
        
    }
    public void Disappear()
    {
        if (mySpawnPoint != null)
        {
            mySpawnPoint.isOccupied = false; // 破棄時に開放
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if (onScreen && !GetComponent<Animator>().enabled)
        {
            GetComponent<Animator>().enabled = true; // カメラに入った瞬間アニメ再生開始
        }
    }
}
