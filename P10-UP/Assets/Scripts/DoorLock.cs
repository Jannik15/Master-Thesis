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
    public GameObject buttonTrigger;
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

    Vector3 localPosition0;    //original local position

    void Awake()
    {
        SetOriginalLocalPosition();

        minDistance = Vector3.Distance(buttonTrigger.transform.position, transform.position);
        maxDistance = buttonTrigger.transform.position.x;
        originalPosition = transform.position;

        rb = GetComponent<Rigidbody>();
        doorAnimator = GetComponentInParent<Animator>();
        layoutGeneration = FindObjectOfType<ProceduralLayoutGeneration>();
        playerInteractions = FindObjectOfType<PlayerInteractions>();
    }

    void Update()
    {
        if (Vector3.Distance(buttonTrigger.transform.position, transform.position) >= minDistance)
        {
            transform.position = originalPosition;
        }
        if (transform.position.x <= maxDistance)
        {
            transform.position = new Vector3(maxDistance, transform.position.y, transform.position.z);
        }

        //if (rb)
        //{
        //    Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        //    localVelocity.y = 0;
        //    localVelocity.z = 0;

        //    rb.velocity = transform.TransformDirection(localVelocity);
        //}

        float x, y, z;


        if (this.x)
            x = localPosition0.x;
        else
            x = transform.localPosition.x;

        if (this.y)
            y = localPosition0.y;
        else
            y = transform.localPosition.y;

        if (this.z)
            z = localPosition0.z;
        else
            z = transform.localPosition.z;


        transform.localPosition = new Vector3(x, y, z);

    }

    public void SetOriginalLocalPosition()
    {
        localPosition0 = transform.localPosition;
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
