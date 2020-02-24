using UnityEngine;
using System;

public class PortalCollisionHandler : MonoBehaviour
{
    private PortalManager portalManager;
    private Vector3 playerPos;
    private Portal thisPortal;

    private GridPortalDemo proceduralLayout;

    // Start is called before the first frame update
    void Start()
    {
        proceduralLayout = FindObjectOfType<GridPortalDemo>();
        portalManager = FindObjectOfType<PortalManager>();
        if (portalManager == null)
            Debug.LogError("Missing portalManager in the scene!\nAdd a PortalManager to an empty GameObject to use this script.");


    }

    private void OnTriggerEnter(Collider portalCollider)
    {
        if (portalCollider.CompareTag(portalManager.GetPortalTag()))
        {
            //* TODO: Simply get the portals from procedural portal generator
            thisPortal = new Portal(portalCollider.gameObject); 
            //*/
            Debug.Log("Entered a portal");
            thisPortal.SwitchActiveSubPortal();



            // TODO: Switch the world here
            proceduralLayout.SwitchWorld();
            CustomUtilities.UpdateRoomStencil(proceduralLayout.rooms[proceduralLayout.currentRoom],0);    // Previous room to current rooms stencil value
            CustomUtilities.UpdateRoomStencil(proceduralLayout.rooms[proceduralLayout.currentRoom], 0);    // Current rooms stencil value to 0
        }
    }

    private void OnTriggerExit(Collider portalCollider)
    {
        if (portalCollider.CompareTag(portalManager.GetPortalTag()))
        {
            Debug.Log("Exited a portal");
            thisPortal.SwitchActiveSubPortal();
            Vector3 offset = transform.position - portalCollider.transform.position;
            if (Vector3.Dot(offset, portalCollider.transform.forward) > 0.0f) // Correctly exited portal
            {
                thisPortal.SetActive(false);
            }
            else // Incorrectly exited the portal, revert changes made OnTriggerEnter
            {
            }
            thisPortal = null;
        }
    }

    private void SwitchWorld()
    {

    }
}
