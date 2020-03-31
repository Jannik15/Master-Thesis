﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollisionHandler : MonoBehaviour
{
    private Portal thisPortal;
    private ProceduralLayoutGeneration proceduralLayout;

    // Start is called before the first frame update
    void Start()
    {
        proceduralLayout = FindObjectOfType<ProceduralLayoutGeneration>();
    }

    private void OnTriggerEnter(Collider triggerCollider)
    {
        if (triggerCollider.CompareTag("Portal"))
        {
            thisPortal = triggerCollider.GetComponent<Portal>(); 

            thisPortal.SwitchActiveSubPortal();

            // Switch world
            proceduralLayout.SwitchCurrentRoom(thisPortal.GetConnectedRoom(), thisPortal);
        }
        else if (triggerCollider.CompareTag("WinCondition"))
        {
            Debug.Log("You Win!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        }
    }
}
