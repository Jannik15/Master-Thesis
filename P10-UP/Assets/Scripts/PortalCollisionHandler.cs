using UnityEngine;
using System;

public class PortalCollisionHandler : MonoBehaviour
{
    private Portal thisPortal;
    private ProceduralLayoutGeneration proceduralLayout;

    // Start is called before the first frame update
    void Start()
    {
        proceduralLayout = FindObjectOfType<ProceduralLayoutGeneration>();
    }

    private void OnTriggerEnter(Collider portalCollider)
    {
        if (portalCollider.CompareTag("Portal"))
        {
            thisPortal = portalCollider.GetComponent<Portal>(); 

            thisPortal.SwitchActiveSubPortal();

            // Switch world
            proceduralLayout.SwitchCurrentRoom(thisPortal.GetConnectedRoom(), thisPortal);
        }
    }

    private void OnTriggerExit(Collider portalCollider)
    {
        if (portalCollider.CompareTag("Portal"))
        {
            thisPortal.SwitchActiveSubPortal();
            Vector3 offset = transform.position - portalCollider.transform.position;
            if (Vector3.Dot(offset, portalCollider.transform.forward) > 0.0f) // Correctly exited portal
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
