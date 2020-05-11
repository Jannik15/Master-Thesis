using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class PlayerCollisionHandler : MonoBehaviour
{
    private ProceduralLayoutGeneration proceduralLayout;
    [HideInInspector] public bool inPortal;
    [HideInInspector] public Portal thisPortal;
    private Transform playerCam;

    // Start is called before the first frame update
    void Start()
    {
        proceduralLayout = FindObjectOfType<ProceduralLayoutGeneration>();
    }

    private void OnTriggerEnter(Collider triggerCollider)
    {
        if (triggerCollider.CompareTag("Portal"))
        {
            inPortal = true;
            thisPortal = triggerCollider.GetComponent<Portal>(); 
            thisPortal.SwitchActiveSubPortal();
            // Switch world
            proceduralLayout.SwitchCurrentRoom(thisPortal.GetConnectedRoom(), thisPortal);
        }
    }

    private void OnTriggerExit(Collider triggerCollider)
    {
        if (triggerCollider.CompareTag("Portal"))
        {
            thisPortal.SwitchActiveSubPortal();
            Vector3 offset = transform.position - triggerCollider.transform.position;
            if (Vector3.Dot(offset, triggerCollider.transform.forward) > 0.0f) // Correctly exited portal
            {
                proceduralLayout.FinalizeRoomSwitch(thisPortal);
            }
            else // Incorrectly exited the portal, revert changes made OnTriggerEnter
            {
                proceduralLayout.SwitchCurrentRoom(proceduralLayout.previousRoom, null);
            }
            inPortal = false;
        }
    }
}
