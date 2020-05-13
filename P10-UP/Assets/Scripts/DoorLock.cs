using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLock : MonoBehaviour
{
    public enum DoorEvent
    {
        Unlocked,
        PressurePlate,
        ShootTarget,
        KeyCard
    }

    public AudioClip door;
    private AudioSource auS;
    public bool isLocked, isOpen;
    public Room inRoom;
    private Animator doorAnimator;
    public DoorEvent lockEvent;
    public GameObject pairedKeyCard;
    public EventObjectBase pairedEvent;
    public Transform keycardScanner;
    public GameObject portal;
    private Portal doorPortal;

    void Awake()
    {
        auS = GetComponent<AudioSource>();
        doorAnimator = GetComponentInParent<Animator>();
    }


    public void Pair(DoorEvent doorEvent, GameObject pairedObject)
    {
        lockEvent = doorEvent;
        isLocked = true;
        pairedEvent = pairedObject.GetComponentInChildren<EventObjectBase>();
        pairedEvent.connectedDoor = this;
        if (doorEvent == DoorEvent.KeyCard)
        {
            pairedKeyCard = pairedObject;
        }

        doorPortal = portal.GetComponentInParent<Portal>();
        doorPortal.SetActive(false);
        doorPortal.doorLock = this;
    }

    public void RemoveButton()
    {
        inRoom.RemoveObjectFromRoom(keycardScanner);
        DestroyImmediate(keycardScanner.gameObject); // Destroy immediate should be used since Destroy is too slow for procedural generation
    }

    public void OpenDoor()
    {
        if (auS != null && door != null)
        {
            auS.PlayOneShot(door);
        }
        doorAnimator.CrossFadeInFixedTime("DoorOpen", 0.9f, 0, 0);
    }

    public void CloseDoor()
    {
        if (auS != null && door != null)
        {
            auS.PlayOneShot(door);
        }
        doorAnimator.CrossFadeInFixedTime("DoorClose", 0.9f, 0, 0);
    }

    public void EnablePortal()
    {
        isOpen = true;
        doorPortal.SetActive(true);
    }

    public void DisablePortal()
    {
        isOpen = false;
        doorPortal.SetActive(false);
    }
}
