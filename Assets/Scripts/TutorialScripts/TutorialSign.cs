using UnityEngine;

public class TutorialSign : MonoBehaviour
{
    [SerializeField] private GameObject demoObject; // 表示するチュートリアルデモ
    [SerializeField] private Transform player;      // プレイヤーのTransform参照
    [SerializeField] private float showDistance = 3f; // この距離以内で表示

    private bool isVisible = false;

    private void Start()
    {
        if (demoObject != null)
            demoObject.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 距離判定
        if (distance <= showDistance && !isVisible)
        {
            ShowDemo(true);
        }
        else if (distance > showDistance && isVisible)
        {
            ShowDemo(false);
        }
    }

    private void ShowDemo(bool show)
    {
        isVisible = show;
        if (demoObject != null)
            demoObject.SetActive(show);
    }

#if UNITY_EDITOR
    // 距離範囲をシーンビューで可視化
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, showDistance);
    }
#endif
}
