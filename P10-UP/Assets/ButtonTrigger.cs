using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent onButtonPressed;

    private bool pressedInProgress = false;

    private PlayerInteractions playerInteractions;

    void Start()
    {
        playerInteractions = FindObjectOfType<PlayerInteractions>();
        if (playerInteractions == null)
        {
            Debug.Log("no playerInteractions");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Button" && !pressedInProgress)
        {
            pressedInProgress = true;
            playerInteractions.DoorAction(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Button")
        {
            pressedInProgress = false;
        }
    }


}
