using UnityEngine;

public class PlayerAttack : MonoBehaviour, IDamageable
{
    [SerializeField] private CharacterStatus status;
    private int currentHP;

    private void Start()
    {
        currentHP = status.maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"Player took {damage} damage. HP: {currentHP}");

        if (currentHP <= 0)
        {
            OnDead();
        }
    }

    public void Attack(IDamageable target)
    {
        target.TakeDamage(status.attack);
    }

    private void OnDead()
    {
        Debug.Log("Player died.");
        // ゲームオーバー処理など
    }
}
