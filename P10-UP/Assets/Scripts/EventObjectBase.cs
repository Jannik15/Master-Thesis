using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventObjectBase : MonoBehaviour
{
    public EventObjectType eventType;
    public Room room;
    public DoorLock connectedDoor;

    public AudioClip winSFX;
    public AudioClip wrongCardSFX;
    public AudioClip correctCardSFX;

    private AudioSource audioSourceKeycard;
    private AudioSource audioSourceDoor;

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
                    if (winSFX != null && GetComponent<AudioSource>())
                    {
                        GetComponent<AudioSource>().PlayOneShot(winSFX);
                    }
                    GameObject.FindGameObjectWithTag("FadeAnimator").gameObject.GetComponent<Animator>().SetTrigger("FadeToWhite");
                }
                break;
            case EventObjectType.ThisType.Keycard:
                if (collider.CompareTag("DropZone"))
                {
                    if (GameObject.FindGameObjectWithTag("WatchInterface").GetComponent<AudioSource>() != null)
                    {
                        audioSourceKeycard = GameObject.FindGameObjectWithTag("WatchInterface").GetComponent<AudioSource>();
                    }

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
                    audioSourceKeycard.Play();
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

                    if (collider.gameObject.GetComponent<AudioSource>() != null && correctCardSFX != null)
                    {
                        audioSourceDoor = collider.gameObject.GetComponent<AudioSource>();
                    }

                    audioSourceDoor.PlayOneShot(correctCardSFX);

                    connectedDoor.OpenDoor();

                    if (gameObject.GetComponentInParent<UIWatch>() != null)
                    {
                        GetComponentInParent<UIWatch>().GetComponentInChildren<WristPlateUI>().TakeOutCard(gameObject);
                    }
                    if (gameObject.GetComponentInParent<WeaponGrab>() != null && gameObject.GetComponentInParent<WeaponGrab>().weaponHeld == transform)
                    {
                        gameObject.GetComponentInParent<WeaponGrab>().DropWeapon();
                    }

                    GameObject dropZone = GameObject.FindGameObjectWithTag("DropZone");

                    if (dropZone != null && dropZone.activeSelf)
                    {
                        dropZone.SetActive(false);
                    }
                    Destroy(gameObject);
                } 
                else if (collider.CompareTag("KeycardScanner"))
                {
                    if (collider.gameObject.GetComponent<AudioSource>() != null && wrongCardSFX != null)
                    {
                        audioSourceDoor = collider.gameObject.GetComponent<AudioSource>();
                        audioSourceDoor.PlayOneShot(wrongCardSFX);
                    }

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
