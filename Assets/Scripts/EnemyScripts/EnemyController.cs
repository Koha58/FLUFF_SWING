using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private CharacterStatus status;
    private int currentHP;

    private void OnEnable()
    {
        currentHP = status.maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHP}");

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
        Debug.Log($"{gameObject.name} died.");
        // オブジェクトプールに返却
        EnemyPool.Instance.ReturnToPool(this);
    }
}
