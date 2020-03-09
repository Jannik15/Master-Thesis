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
        Debug.Log(gameObject.name + " (parent=" + transform.parent.name + ") entered collider: " + portalCollider.gameObject.name + " (tag=" + portalCollider.tag + ")");
        if (portalCollider.CompareTag("Portal"))
        {
            thisPortal = portalCollider.GetComponent<Portal>(); 

            thisPortal.SwitchActiveSubPortal();

            // Switch world
            proceduralLayout.SwitchWorld(thisPortal);
        }
    }

    private void OnTriggerExit(Collider portalCollider)
    {
        Debug.Log(gameObject.name + " (parent=" + transform.parent.name + ") exited collider: " + portalCollider.gameObject.name + " (tag=" + portalCollider.tag + ")");
        if (portalCollider.CompareTag("Portal"))
        {
            thisPortal.SwitchActiveSubPortal();
            Vector3 offset = transform.position - portalCollider.transform.position;
            if (Vector3.Dot(offset, portalCollider.transform.forward) > 0.0f) // Correctly exited portal
            {
                proceduralLayout.FinalizeWorldSwitch(thisPortal);
            }
            else // Incorrectly exited the portal, revert changes made OnTriggerEnter
            {
                proceduralLayout.UndoSwitchWorld(thisPortal);
            }
        }
    }
}
