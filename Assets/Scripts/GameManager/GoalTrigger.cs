using UnityEngine;

/// <summary>
/// ゴール地点に配置して、プレイヤーが到達したらクリア処理を呼ぶ
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.OnGoalReached(other.transform);
        }
    }
}
