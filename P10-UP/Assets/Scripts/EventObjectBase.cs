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
                    Debug.Log("You Win!");
                    collider.gameObject.GetComponentInChildren<Animator>().SetTrigger("FadeToWhite");
                }
                break;
            case EventObjectType.ThisType.Keycard:
                if (collider.CompareTag("DropZone"))
                {
                    gameObject.GetComponentInParent<WeaponGrab>().DropWeapon();
                    gameObject.transform.parent = GameObject.FindGameObjectWithTag("KeyHolder").transform;
                    gameObject.transform.position = new Vector3(3.55f, 0, 1);
                    gameObject.SetActive(false);
                    keycardList.Add(gameObject);

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
