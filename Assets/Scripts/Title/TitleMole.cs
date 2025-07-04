using UnityEngine;

public class Mole : MonoBehaviour
{

    public float lifeTime = 2f; // 表示される時間

    public MoleSpawnPoint mySpawnPoint;

    private bool hasStarted = false;


    void Start()
    {

        // Animatorは最初は無効化しておき、Updateで画面内に入ったら再生する
        GetComponent<Animator>().enabled = false;

    }
    void OnDestroy()
    {
        if (mySpawnPoint != null)
        {
            mySpawnPoint.isOccupied = false; // 破棄時に開放
        }
    }

    private void Update()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if (onScreen && !GetComponent<Animator>().enabled)
        {
            var animator = GetComponent<Animator>();
            animator.enabled = true;
            animator.Play("TitleMole"); // アニメーション名を正確に

            // Destroy をここで呼ぶことで、画面に入ってからlifeTime秒後に消える
            Destroy(gameObject, lifeTime);
        }
    }
}
