using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private GameObject duplicatedMeshObject;
    private Renderer[] renderers, duplicatedRenderers;
    private Portal colliderPortal;

    private void Start()
    {
        // Create and cache a duplicated version of the object, and remove unnecessary components from it
        if (transform.parent == null || transform.parent.GetComponent<InteractableObject>() == null)
        {
            // Start function for object that will keep the script (not duplicated)
            renderers = GetComponentsInChildren<Renderer>();
            duplicatedMeshObject = Instantiate(gameObject, transform.position, transform.rotation, transform);
            duplicatedMeshObject.transform.localScale = Vector3.one;
            duplicatedRenderers = duplicatedMeshObject.GetComponentsInChildren<Renderer>();

            //* Disable this when the interactable objects are procedurally generated - since that should probably handle material instantiation
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    renderers[i].materials[j] = Instantiate(renderers[i].materials[j]);
                }
            }
            for (int i = 0; i < duplicatedRenderers.Length; i++)
            {
                for (int j = 0; j < duplicatedRenderers[i].materials.Length; j++)
                {
                    duplicatedRenderers[i].materials[j] = Instantiate(duplicatedRenderers[i].materials[j]);
                }
            }
            //*/
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
            colliderPortal = collider.GetComponentInChildren<Portal>();
            duplicatedMeshObject.SetActive(true);
            Vector3 dir = transform.position - collider.transform.position;
            if (math.dot(collider.transform.forward, dir) >= 0 && transform.parent != null && transform.parent == colliderPortal.GetConnectedRoom().gameObject.transform)
            {
                CustomUtilities.UpdateStencils(duplicatedRenderers, collider.transform, colliderPortal.GetRoom().GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue - 100);
            }
            else if ((math.dot(collider.transform.forward, dir) < 0 || Vector3.Angle(collider.transform.forward, dir) > 70)
                     &&
                     (transform.parent == null || transform.parent == colliderPortal.GetRoom().gameObject.transform))
            {
                CustomUtilities.UpdateStencils(duplicatedRenderers, collider.transform, colliderPortal.GetConnectedRoom().GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue + 200);
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Portal"))
        {
            Vector3 dir = (transform.position - collider.transform.position).normalized;
            dir.y = 0; // Remove y for angle comparison - portal y is always 0
            if (math.dot(collider.transform.forward, dir) >= 0 && Vector3.Angle(collider.transform.forward, dir) < 60.0f) // Exited in the connected room
            {
                CustomUtilities.UpdateStencils(renderers, collider.transform, colliderPortal.GetConnectedRoom().GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue + 200);
                CustomUtilities.UpdateStencils(duplicatedRenderers, collider.transform, colliderPortal.GetConnectedRoom().GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue + 200);
                transform.parent = colliderPortal.GetConnectedRoom().gameObject.transform;
            }
            else // Exited in the room where the portal is
            {
                CustomUtilities.UpdateStencils(renderers, collider.transform, colliderPortal.GetRoom().GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue - 100);
                CustomUtilities.UpdateStencils(duplicatedRenderers, collider.transform, colliderPortal.GetRoom().GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue - 100);
                transform.parent = colliderPortal.GetRoom().gameObject.transform;
            }
            duplicatedMeshObject.SetActive(false);
        }
    }
}
