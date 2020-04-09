using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public GameObject duplicatedMeshObject;
    private Renderer[] renderers, duplicatedRenderers;
    public int newStencilValue, newRenderQueue;
    public Portal colliderPortal;

    private void Start()
    {
        // Create and cache a duplicated version of the object, and remove unnecessary components from it
        if (transform.parent == null || transform.parent.GetComponent<InteractableObject>() == null)
        {
            duplicatedMeshObject = Instantiate(gameObject, transform.position, transform.rotation, transform);
            duplicatedMeshObject.transform.localScale = Vector3.one;
            // Start function for object that will keep the script (not duplicated)
            renderers = GetComponentsInChildren<Renderer>();
            duplicatedRenderers = duplicatedMeshObject.GetComponentsInChildren<Renderer>();
        }
        else
        {
            // Remove all components except for transform and renderer-related components. This also works for components in the children
            Component[] allComponents = GetComponentsInChildren(typeof(Component));
            for (int i = 0; i < allComponents.Length; i++)
            {
                if (allComponents[i].GetType() != typeof(Transform) &&
                    allComponents[i].GetType() != typeof(MeshFilter) &&
                    allComponents[i].GetType() != typeof(MeshRenderer))
                {
                    Destroy(allComponents[i]);
                }
            }
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Portal"))
        {
            Debug.Log(gameObject.name + " entered a portal");
            colliderPortal = collider.GetComponentInChildren<Portal>();
            //CustomUtilities.UpdateStencils(duplicatedRenderers, collider.transform, colliderPortal.GetRenderer().material.GetInt("_StencilValue"), colliderPortal.GetRenderer().material.GetInt("_StencilValue"));
            duplicatedMeshObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Portal"))
        {
            Debug.Log(gameObject.name + " exited a portal");
            //CustomUtilities.UpdateStencils(renderers, collider.transform, colliderPortal.GetRenderer().material.GetInt("_StencilValue"),colliderPortal.GetRenderer().material.GetInt("_StencilValue"));
            transform.parent = collider.GetComponentInChildren<Portal>().GetConnectedRoom().gameObject.transform;
            duplicatedMeshObject.SetActive(false);
        }
    }
}
