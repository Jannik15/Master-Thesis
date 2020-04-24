using System.Linq.Expressions;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Room inRoom;

    public float Health = 10f;


    public void AssignRoom(Room room, bool playerCanCollide)
    {
        this.inRoom = room;
        room.AddObjectToRoom(transform, playerCanCollide);
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0f)
        {
            Die();
        }
    }

    public void InteractableTarget(GameObject target)
    {
        if (target.GetComponent<EventObjectBase>() != null)
        {
            target.GetComponent<EventObjectBase>().TargetShot();
        }
    }

    void Die()
    {
        if (inRoom != null)
        {
            inRoom.RemoveObjectFromRoom(gameObject.transform);
        }
        Destroy(gameObject, 1f);
    }

    void Disable()
    {

    }

    void OpenDoor()
    {

    }

}
