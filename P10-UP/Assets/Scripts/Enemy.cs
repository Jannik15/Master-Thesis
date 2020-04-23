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

    public void InteractableTarget()
    {
        OpenDoor();
    }

    void Die()
    {
        Destroy(gameObject, 1f);
    }

    void Disable()
    {

    }

    void OpenDoor()
    {

    }

}
