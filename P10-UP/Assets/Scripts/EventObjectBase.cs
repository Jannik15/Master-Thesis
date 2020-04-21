using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventObjectBase : MonoBehaviour
{
    public EventObjectType eventType;
    public Room room;
    public DoorLock connectedDoor;

    void OnTriggerEnter(Collider collider)
    {
        switch (eventType.thisEventType)
        {
            case EventObjectType.ThisType.PressurePlate:
                if (collider.CompareTag("Button"))
                {
                    connectedDoor.isLocked = false;
                }
                break;
            case EventObjectType.ThisType.WinCondition:
                if (collider.CompareTag("Player"))
                {
                    Debug.Log("You Win!");
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // TODO: Proper win screen
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
                    connectedDoor.isLocked = true;
                    if (connectedDoor.isOpen)
                    {
                        connectedDoor.CloseDoor();
                    }
                }
                break;
        }
    }
}
