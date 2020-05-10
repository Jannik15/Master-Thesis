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
                    if (!connectedDoor.isOpen)
                    {
                        connectedDoor.OpenDoor();
                    }
                }
                break;
            case EventObjectType.ThisType.WinCondition:
                if (collider.CompareTag("Player"))
                {
                    GameObject.FindGameObjectWithTag("FadeAnimator").gameObject.GetComponent<Animator>().SetTrigger("FadeToWhite");
                }
                break;
            case EventObjectType.ThisType.Keycard:
                if (collider.CompareTag("DropZone"))
                {
                    if (GetComponentInParent<WeaponGrab>() != null)
                    {
                        GetComponentInParent<WeaponGrab>().DropWeapon();
                    }

                    InteractableObject interact = GetComponentInChildren<InteractableObject>();

                    if (interact != null)
                    {
                        if (interact.inRoom != null)
                        {
                            interact.inRoom.RemoveObjectFromRoom(gameObject.transform);
                        }

                        interact.isHeld = true;
                    }
                    Debug.Log("Trying to find Keyholder: " + GameObject.FindGameObjectWithTag("KeyHolder"));
                    gameObject.transform.parent = GameObject.FindGameObjectWithTag("KeyHolder").transform;
                    gameObject.transform.localPosition = new Vector3(5.75f, 0, 14);
                    gameObject.transform.localEulerAngles = new Vector3(0, 180, 180);
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    GetComponentInParent<UIWatch>().gameObject.GetComponentInChildren<WristPlateUI>().ListAdder(gameObject);
                    gameObject.SetActive(false);
                    collider.gameObject.SetActive(false);
                }
                else if (collider.CompareTag("KeycardScanner") && collider.gameObject.GetComponentInParent<DoorLock>() == connectedDoor)
                {
                    Debug.Log("KeyScanned: opening door: " + connectedDoor.name);
                    connectedDoor.OpenDoor();

                    if (gameObject.GetComponentInParent<UIWatch>() != null)
                    {
                        Debug.Log("KeyScanned: UIWatch parent is not null, checking wristplateUI in child: " + GetComponentInParent<UIWatch>().GetComponentInChildren<WristPlateUI>().name);
                        GetComponentInParent<UIWatch>().GetComponentInChildren<WristPlateUI>().TakeOutCard(gameObject);
                    }
                    if (gameObject.GetComponentInParent<WeaponGrab>() != null && gameObject.GetComponentInParent<WeaponGrab>().weaponHeld == transform)
                    {
                        gameObject.GetComponentInParent<WeaponGrab>().DropWeapon();
                    }

                    GameObject dropZone = GameObject.FindGameObjectWithTag("DropZone");

                    if (dropZone != null && dropZone.activeSelf)
                    {
                        Debug.Log("Disabling dropZone: " + dropZone.name);
                        dropZone.SetActive(false);
                    }
                    Debug.Log("Destroying keycard...");
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

    public void TargetShot()
    {
        connectedDoor.isLocked = false;
        connectedDoor.OpenDoor();
    }
}
