using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLock : MonoBehaviour
{

    [Header("Freeze Local Position")]
    [SerializeField]
    bool x;
    [SerializeField]
    bool y;
    [SerializeField]
    bool z;

    public enum DoorEvent
    {
        Unlocked,
        PressurePlate,
        ShootTarget,
        KeyCard
    }
    public bool isLocked, isOpen;
    public Transform button;
    public Transform buttonTrigger;
    public Room inRoom;
    private Vector3 originalPosition;
    private Rigidbody rb;
    private Animator doorAnimator;
    private float minDistance, maxDistance;
    public DoorEvent lockEvent;
    public GameObject pairedKeyCard;
    public EventObjectBase pairedPressurePlate;
    private ProceduralLayoutGeneration layoutGeneration;
    private PlayerInteractions playerInteractions;
    [SerializeField] private Transform buttonGroup;

    Vector3 localPosition0;    //original local position

    void Awake()
    {
        doorAnimator = GetComponentInParent<Animator>();
        layoutGeneration = FindObjectOfType<ProceduralLayoutGeneration>();
        playerInteractions = FindObjectOfType<PlayerInteractions>();

        if (buttonTrigger != null)
        {
            SetOriginalLocalPosition();
            minDistance = Vector3.Distance(buttonTrigger.position, button.position);
            maxDistance = buttonTrigger.position.x;
            originalPosition = button.position;

            rb = button.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        if (buttonTrigger != null)
        {
            if (Vector3.Distance(buttonTrigger.position, button.position) >= minDistance)
            {
                button.position = originalPosition;
            }
            if (button.position.x <= maxDistance)
            {
                button.position = new Vector3(maxDistance, button.position.y, button.position.z);
            }

            //if (rb)
            //{
            //    Vector3 localVelocity = button.transform.InverseTransformDirection(rb.velocity);
            //    localVelocity.y = 0;
            //    localVelocity.z = 0;

            //    rb.velocity = button.transform.TransformDirection(localVelocity);
            //}

            float x, y, z;


            if (this.x)
                x = localPosition0.x;
            else
                x = button.transform.localPosition.x;

            if (this.y)
                y = localPosition0.y;
            else
                y = button.transform.localPosition.y;

            if (this.z)
                z = localPosition0.z;
            else
                z = button.transform.localPosition.z;


            button.transform.localPosition = new Vector3(x, y, z);
        }

    }

    public void SetOriginalLocalPosition()
    {
        localPosition0 = button.transform.localPosition;
    }


    public void Pair(DoorEvent doorEvent, GameObject pairedObject)
    {
        lockEvent = doorEvent;
        isLocked = true;
        switch (doorEvent)
        {
            case DoorEvent.PressurePlate:
                pairedPressurePlate = pairedObject.GetComponentInChildren<EventObjectBase>();
                pairedPressurePlate.connectedDoor = this;
                break;
            case DoorEvent.ShootTarget:
                break;
            case DoorEvent.KeyCard:
                pairedKeyCard = pairedObject;
                break;
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
        inRoom.RemoveObjectFromRoom(buttonGroup);
        Destroy(buttonGroup.gameObject);

    }

    public void OpenDoor()
    {
        doorAnimator.CrossFadeInFixedTime("DoorOpen", 0.9f, 0, 0);
        isOpen = true;
    }

    public void CloseDoor()
    {
        doorAnimator.CrossFadeInFixedTime("DoorClose", 0.9f, 0, 0);
        isOpen = false;
    }
}
