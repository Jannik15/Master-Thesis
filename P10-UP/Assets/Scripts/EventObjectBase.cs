﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventObjectBase : MonoBehaviour
{
    public bool tutorialMode = false;
    public GameObject prevSlide;
    public GameObject nextSlide;
    public GameObject prevSlide2;
    public GameObject nextSlide2;
    public GameObject part10;
    public GameObject part11;
    public GameObject prevSlide3;
    public GameObject nextSlide3;


    public EventObjectType eventType;
    public Room room;
    public DoorLock connectedDoor;

    public AudioClip winSFX;
    public AudioClip wrongCardSFX;
    public AudioClip correctCardSFX;

    private AudioSource audioSourceKeycard;
    private AudioSource audioSourceDoor;

    void Start()
    {

    }

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
                    Debug.Log("Trying to find Keyholder: " + GameObject.FindGameObjectWithTag("KeyHolder"));
                    audioSourceKeycard.Play();
                    gameObject.transform.parent = GameObject.FindGameObjectWithTag("KeyHolder").transform;
                    gameObject.transform.localPosition = new Vector3(5.75f, 0, 14);
                    gameObject.transform.localEulerAngles = new Vector3(0, 180, 180);
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    GetComponentInParent<UIWatch>().gameObject.GetComponentInChildren<WristPlateUI>().ListAdder(gameObject);
                    gameObject.SetActive(false);
                    collider.gameObject.SetActive(false);

                    if (tutorialMode && GetComponentInParent<UIWatch>().gameObject
                        .GetComponentInChildren<WristPlateUI>().keysList.Count >= 2)
                    {
                        if (prevSlide.activeSelf)
                        {
                            prevSlide.SetActive(false);
                            nextSlide.SetActive(true);
                        }
                    }
                }
                else if (collider.CompareTag("KeycardScanner") && collider.gameObject.GetComponentInParent<DoorLock>() == connectedDoor)
                {

                    if (collider.gameObject.GetComponent<AudioSource>() != null && correctCardSFX != null)
                    {
                        audioSourceDoor = collider.gameObject.GetComponent<AudioSource>();
                    }

                    audioSourceDoor.PlayOneShot(correctCardSFX);

                    Debug.Log("KeyScanned: opening door: " + connectedDoor.name);

                    if (tutorialMode && prevSlide2.activeSelf == true)
                    {
                        prevSlide2.SetActive(false);
                        nextSlide2.SetActive(true);
                        part11.SetActive(true);
                        Destroy(part10, 5f);
                    }

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
        if (tutorialMode)
        {
            if (prevSlide3.activeSelf)
            {
                prevSlide3.SetActive(false);
                nextSlide3.SetActive(true);
            }
        }
        else
        {
            connectedDoor.isLocked = false;
            connectedDoor.OpenDoor();
        }

    }
}
