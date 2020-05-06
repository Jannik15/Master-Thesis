using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLock : MonoBehaviour
{
    #region Button (deprecated)
    /*
    [Header("Freeze Local Position")]
    [SerializeField]
    bool x;
    [SerializeField]
    bool y;
    [SerializeField]
    bool z;
    public Transform button;
    public Transform buttonTrigger;
    [SerializeField] private Transform buttonGroup;
    private float minDistance, maxDistance;
    private Vector3 originalPosition;
    private Rigidbody rb;
    */
    #endregion
    public enum DoorEvent
    {
        Unlocked,
        PressurePlate,
        ShootTarget,
        KeyCard
    }
    public bool isLocked, isOpen;
    public Room inRoom;
    private Animator doorAnimator;
    public DoorEvent lockEvent;
    public GameObject pairedKeyCard;
    public EventObjectBase pairedEvent;
    private ProceduralLayoutGeneration layoutGeneration;
    private PlayerInteractions playerInteractions;
    public Transform keycardScanner;

    void Awake()
    {
        doorAnimator = GetComponentInParent<Animator>();
        layoutGeneration = FindObjectOfType<ProceduralLayoutGeneration>();
        playerInteractions = FindObjectOfType<PlayerInteractions>();

        /*
        if (buttonTrigger != null)
        {
            SetOriginalLocalPosition();
            minDistance = Vector3.Distance(buttonTrigger.position, button.position);
            maxDistance = buttonTrigger.position.x;
            originalPosition = button.position;

            rb = button.GetComponent<Rigidbody>();
        }
        */
    }

    //public void SetOriginalLocalPosition()
    //{
    //    localPosition0 = button.transform.localPosition;
    //}


    public void Pair(DoorEvent doorEvent, GameObject pairedObject)
    {
        lockEvent = doorEvent;
        isLocked = true;
        if (doorEvent == DoorEvent.KeyCard)
        {
            pairedKeyCard = pairedObject;

        }
        else
        {
            pairedEvent = pairedObject.GetComponentInChildren<EventObjectBase>();
            pairedEvent.connectedDoor = this;
        }
    }

    public void TryToUnlock()
    {
        switch (lockEvent)
        {
            case DoorEvent.PressurePlate:
                break;
            case DoorEvent.ShootTarget:
                break;
            case DoorEvent.KeyCard:
                isLocked = pairedKeyCard != null;
                if (!isLocked)
                {
                    // Only works for linear layouts
                    for (int i = 0; i < playerInteractions.keyUI.Count; i++)
                    {
                        if (playerInteractions.keyUI[i].activeSelf)
                        {
                            playerInteractions.keyUI[i].SetActive(false);
                            break;
                        }
                    }
                }
                break;
        }
    }

    public void RemoveButton()
    {
        //inRoom.RemoveObjectFromRoom(buttonGroup);
        //DestroyImmediate(buttonGroup.gameObject); // Destroy immediate should be used since Destroy is too slow for procedural generation
        inRoom.RemoveObjectFromRoom(keycardScanner);
        DestroyImmediate(keycardScanner.gameObject); // Destroy immediate should be used since Destroy is too slow for procedural generation
    }

    public void OpenDoor()
    {
        doorAnimator.CrossFadeInFixedTime("DoorOpen", 0.9f, 0, 0);
        isOpen = true;
        // TODO: Enable portal here
    }

    public void CloseDoor()
    {
        doorAnimator.CrossFadeInFixedTime("DoorClose", 0.9f, 0, 0);
        isOpen = false;
        // TODO: Disable portal here
    }
}
