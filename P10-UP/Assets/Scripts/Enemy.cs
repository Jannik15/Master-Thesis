using System.Linq.Expressions;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Room inRoom;

    public float Health = 1f;

    void Awake()
    {
        SetKinematic(true);
    }

    public void AssignRoom(Room room, bool playerCanCollide)
    {
        inRoom = room;
        inRoom.AddObjectToRoom(transform, playerCanCollide);
    }

    public void TakeDamage(float amount, Vector3 hitpoint)
    {
        Health -= amount;
        if (Health <= 0f)
        {
            Die(hitpoint);
        }
    }

    public void SetKinematic(bool newValue)
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in bodies)
        {
            rb.isKinematic = newValue;
        }
    }

    public void InteractableTarget(GameObject target)
    {
        if (target.GetComponent<EventObjectBase>() != null)
        {
            target.GetComponent<EventObjectBase>().TargetShot();
        }
    }

    void Die(Vector3 hitpoint2)
    {
        GetComponent<Animator>().enabled = false;
        Rigidbody rbMain = GetComponent<Rigidbody>();
        rbMain.isKinematic = false;
        SetKinematic(false);
        foreach (var item in Physics.OverlapSphere(hitpoint2,0.5f))
        {
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddExplosionForce(450, hitpoint2, 0.5f);
            }
        }
        
        if (inRoom != null)
        {
            inRoom.RemoveObjectFromRoom(gameObject.transform);
        }
        Destroy(gameObject, 20f);
    }

    void Disable()
    {

    }

    void OpenDoor()
    {

    }

}
