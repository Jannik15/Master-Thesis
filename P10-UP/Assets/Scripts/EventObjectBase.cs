using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventObjectBase : MonoBehaviour
{
    public EventObjectType eventType;
    public Room room;
    public DoorLock connectedDoor;
    public List<GameObject> keycardList = new List<GameObject>();

    void OnTriggerEnter(Collider collider)
    {
        switch (eventType.thisEventType)
        {
            case EventObjectType.ThisType.PressurePlate:
                if (collider.CompareTag("Button"))
                {
                    if (!connectedDoor.isOpen)
                    {
                        connectedDoor.OpenDoor();
                    }
                }
                break;
            case EventObjectType.ThisType.WinCondition:
                if (collider.CompareTag("Player"))
                {
                    //Debug.Log(FindObjectOfType<PlayerInteractions>().fadeAnimator.gameObject.name);
                    //FindObjectOfType<PlayerInteractions>().fadeAnimator.SetTrigger("FadeToWhite");
                    GameObject.FindGameObjectWithTag("FadeAnimator").gameObject.GetComponent<Animator>().SetTrigger("FadeToWhite");
                }
                break;
            case EventObjectType.ThisType.Keycard:
                if (collider.CompareTag("DropZone"))
                {
                    gameObject.GetComponentInParent<WeaponGrab>().DropWeapon();
                    gameObject.transform.parent = GameObject.FindGameObjectWithTag("KeyHolder").transform;
                    gameObject.transform.localPosition = new Vector3(5.75f, 0, 14);
                    gameObject.transform.localEulerAngles = new Vector3(0, 180, 180);
                    gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    GetComponentInParent<UIWatch>().gameObject.GetComponentInChildren<WristPlateUI>().ListAdder(gameObject);
                    gameObject.SetActive(false);
                    collider.gameObject.SetActive(false);
                }
                else if (collider.CompareTag("KeycardScanner") && collider.gameObject.GetComponentInParent<DoorLock>() == connectedDoor)
                {
                    connectedDoor.OpenDoor();
                    gameObject.GetComponentInParent<WeaponGrab>().DropWeapon();
                    Destroy(gameObject);
                }
                break;
        }
    }
    void OnTriggerExit(Collider collider)
    {
        switch (eventType.thisEventType)
        {
            case EventObjectType.ThisType.PressurePlate:
                if (collider.CompareTag("Button"))
                {
                    if (connectedDoor.isOpen)
                    {
                        connectedDoor.CloseDoor();
                    }
                }
                break;
        }
    }

    public void AssignRoom(Room room, bool playerCanCollide)
    {
        this.room = room;
        room.AddObjectToRoom(transform, playerCanCollide);
    }

    public void TargetShot() // TODO: Add logic for calling when target is shot 
    {
        connectedDoor.isLocked = false;
        connectedDoor.OpenDoor();
    }
}
