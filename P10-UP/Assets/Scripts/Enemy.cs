using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool tutorialMode;
    public GameObject prevSlide;
    public GameObject nextSlide;

    public Room inRoom;

    public float Health = 1f;

    public AudioClip deathSFX;

    private AudioSource auS;

    void Awake()
    {
        SetKinematic(true);

        if (GetComponent<AudioSource>() != null)
        {
            auS = GetComponent<AudioSource>();
        }
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
        if (auS != null && deathSFX != null)
        {
            auS.PlayOneShot(deathSFX);
        }

        GetComponent<Animator>().enabled = false;
        Rigidbody rbMain = GetComponent<Rigidbody>();
        rbMain.isKinematic = false;
        Destroy(GetComponentInChildren<EnemyAI>().lineInstanced);
        SetKinematic(false);
        foreach (var item in Physics.OverlapSphere(hitpoint2,0.5f))
        {
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddExplosionForce(450, hitpoint2, 0.5f);
            }
        }
        
        inRoom?.RemoveObjectFromRoom(gameObject.transform);

        if (!tutorialMode)
        {
            Destroy(gameObject, 20f);
        }
        else
        {
            if (prevSlide.activeSelf)
            {
                prevSlide.SetActive(false);
                nextSlide.SetActive(true);
            }
            Destroy(gameObject, 5f);
            
        }



    }
}
