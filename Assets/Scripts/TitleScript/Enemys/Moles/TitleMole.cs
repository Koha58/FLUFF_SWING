using UnityEngine;

public class Mole : MonoBehaviour
{
    public float startDelay = 0.5f; // 画面内に入ってからアニメーションを開始するまでの遅延時間
    private bool hasStarted = false; // 出現処理が開始されたか
    private bool hasAnimated = false; // アニメーションが一度完了したか

    void Start()
    {
        // Animatorは最初は無効化
        GetComponent<Animator>().enabled = false;
        // lifeTimeやmySpawnPointは使用しないため削除
    }

    // OnDestroyはSpawnPoint管理がなくなったため不要（削除しても良いが、ここでは残しません）
    // void OnDestroy() {} 

    private void Update()
    {
        // 1. 画面内判定
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        // ビューポート座標 (0,0)～(1,1) かつカメラ前方(z>0) にあるか
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        // --- フェーズ 1: 画面イン後の遅延起動 ---
        if (onScreen && !hasStarted)
        {
            // 画面内に入ったら、startDelay秒後にアニメーション開始メソッドを呼び出す
            Invoke("StartMoleAnimation", startDelay);
            hasStarted = true;
        }

        // --- フェーズ 2: アニメーション後の待機と画面外破棄 ---
        // アニメーションが完了し、かつ画面外（左側）に出た場合
        if (hasAnimated && screenPoint.x < -0.2f)
        {
            // 画面左端（0.0f）を大きく超えたら破棄
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 遅延後に呼び出され、アニメーションを開始するメソッド。
    /// </summary>
    void StartMoleAnimation()
    {
        if (hasAnimated) return; // 既にアニメート済みなら何もしない

        var animator = GetComponent<Animator>();
        animator.enabled = true;
        animator.Play("TitleMole");

        // アニメーションが終了したかを監視するために、アニメーション完了を待つ処理を実装する必要があります。
        // 簡単のため、ここではアニメーションの再生直後にアニメーション完了フラグを立てます。
        // 実際のゲームでは、アニメーションイベントやコルーチンを使って正確に完了を待つべきです。
        // ここでは便宜上、すぐにフラグを立てて次の待機フェーズへ移行させます。
        hasAnimated = true;
    }

    /*
     * public float lifeTime = 2f; // 表示される時間

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

        if (onScreen && !hasStarted)
        {
            var animator = GetComponent<Animator>();
            animator.enabled = true;
            animator.Play("TitleMole"); // アニメーション名を正確に

            // Destroy をここで呼ぶことで、画面に入ってからlifeTime秒後に消える
            Destroy(gameObject, lifeTime);
        }
    }
    */
}
