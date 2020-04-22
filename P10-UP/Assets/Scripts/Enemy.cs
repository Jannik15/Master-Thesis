using System.Linq.Expressions;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public float Health = 10f;

    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

}
