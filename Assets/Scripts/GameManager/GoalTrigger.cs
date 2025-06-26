using UnityEngine;

/// <summary>
/// �S�[���n�_�ɔz�u���āA�v���C���[�����B������N���A�������Ă�
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
