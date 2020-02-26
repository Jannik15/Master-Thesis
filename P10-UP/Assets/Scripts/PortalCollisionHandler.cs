using UnityEngine;
using System;

public class PortalCollisionHandler : MonoBehaviour
{
    private Vector3 playerPos;
    private Portal thisPortal;
    private GridPortalDemo proceduralLayout;

    // Start is called before the first frame update
    void Start()
    {
        proceduralLayout = FindObjectOfType<GridPortalDemo>();
    }

    private void OnTriggerEnter(Collider portalCollider)
    {
        if (portalCollider.CompareTag("Portal"))
        {
            Debug.Log("Entered collider");
            thisPortal = portalCollider.GetComponent<Portal>(); 

            thisPortal.SwitchActiveSubPortal();


            // Switch world
            proceduralLayout.SwitchWorld(thisPortal);
        }
    }

    private void OnTriggerExit(Collider portalCollider)
    {
        if (portalCollider.CompareTag("Portal"))
        {
            Debug.Log("Exited collider");
            thisPortal.SwitchActiveSubPortal();
            Vector3 offset = transform.position - portalCollider.transform.position;
            if (Vector3.Dot(offset, portalCollider.transform.forward) > 0.0f) // Correctly exited portal
            {
                thisPortal.SetActive(false);
                thisPortal.GetConnectedPortal().SetActive(true);
            }
            else // Incorrectly exited the portal, revert changes made OnTriggerEnter
            {
                proceduralLayout.UndoSwitchWorld();
            }
        }
    }
}
