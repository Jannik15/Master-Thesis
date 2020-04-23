using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    private GameObject duplicatedMeshObject;
    private Renderer[] renderers, duplicatedRenderers;
    private CanvasRenderer[] uiRenderers, uiDuplicatedRenderers;
    public Room inRoom;
    public bool isHeld;
    private LayerMask layer;
    private int currentRoomLayer, differentRoomLayer, interactableLayer;

    private void Start()
    {
        // Create and cache a duplicated version of the object, and remove unnecessary components from it
        if (transform.parent == null || transform.parent.GetComponent<InteractableObject>() == null)
        {
            // Start function for the object that will keep the script (not duplicated)
            renderers = GetComponentsInChildren<Renderer>();
            uiRenderers = GetComponentsInChildren<CanvasRenderer>();
            duplicatedMeshObject = Instantiate(gameObject, transform.position, transform.rotation, transform);
            duplicatedMeshObject.transform.localScale = Vector3.one;
            duplicatedRenderers = duplicatedMeshObject.GetComponentsInChildren<Renderer>();
            uiDuplicatedRenderers = duplicatedMeshObject.GetComponentsInChildren<CanvasRenderer>();

            //* Material instantiation
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
            for (int i = 0; i < uiRenderers.Length; i++)
            {
                for (int j = 0; j < uiRenderers[i].materialCount; j++)
                {
                    uiRenderers[i].SetMaterial(Instantiate(uiRenderers[i].GetMaterial(j)), j);
                }
            }
            for (int i = 0; i < uiDuplicatedRenderers.Length; i++)
            {
                for (int j = 0; j < uiDuplicatedRenderers[i].materialCount; j++)
                {
                    uiDuplicatedRenderers[i].SetMaterial(Instantiate(uiDuplicatedRenderers[i].GetMaterial(j)), j);
                }
            }
            //*/

            // Register grab events
            OVRGrabber[] grabbers = Resources.FindObjectsOfTypeAll<OVRGrabber>();
            for (int i = 0; i < grabbers.Length; i++)
            {
                grabbers[i].objectGrabEvent += GrabEventListener;
            }

            currentRoomLayer = LayerMask.NameToLayer("CurrentRoom"); // TODO: Change to default when testable in VR
            differentRoomLayer = LayerMask.NameToLayer("DifferentRoom");
            interactableLayer = LayerMask.NameToLayer("Interactable");
        }
        else
        {
            // Remove all components except for transform and renderer-related components. This also works for components in the children
            Component[] allComponents = GetComponentsInChildren(typeof(Component));
            for (int i = 0; i < allComponents.Length; i++)
            {
                if (allComponents[i].GetType() != typeof(Transform) &&
                    allComponents[i].GetType() != typeof(MeshFilter) &&
                    allComponents[i].GetType() != typeof(MeshRenderer) &&
                    allComponents[i].GetType() != typeof(RectTransform) &&
                    allComponents[i].GetType() != typeof(CanvasRenderer) &&
                    allComponents[i].GetType() != typeof(Text) &&
                    allComponents[i].GetType() != typeof(Image) &&
                    allComponents[i].GetType() != typeof(GraphicRaycaster))
                {
                    Destroy(allComponents[i]);
                }
            }
            gameObject.SetActive(false);
        }
    }

    private void GrabEventListener(GameObject grabbedObject, bool isBeingGrabbed)
    {
        // The object being grabbed is this object
        if (gameObject == grabbedObject)
        {
            isHeld = isBeingGrabbed;
            if (isHeld)
            {
                layer = gameObject.layer;
                gameObject.layer = interactableLayer;   // To avoid terrain collision
            }
            else
            {
                gameObject.layer = layer;
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Portal"))
        {
            Portal colliderPortal = collider.GetComponentInChildren<Portal>();
            duplicatedMeshObject.SetActive(true);
            Room portalRoom = colliderPortal.GetRoom();
            Room portalConnectedRoom = colliderPortal.GetConnectedRoom();

            Vector3 dir = (collider.transform.position - transform.position).normalized;
            float dotProduct = math.dot(collider.transform.forward, dir);
            // Enter from the correct side - set it to the next room
            if (inRoom == portalRoom)
            {
                CustomUtilities.UpdateStencils(duplicatedRenderers, collider.transform, portalConnectedRoom.GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue + 200);
                if (!isHeld)
                {
                    Debug.Log("Setting " + gameObject.name + " to Interactable layer");
                    gameObject.layer = interactableLayer;
                }
                layer = differentRoomLayer;
            }
            else if (inRoom == portalConnectedRoom) // TODO: How can you interact with something in a different room??
            {
                CustomUtilities.UpdateStencils(duplicatedRenderers, collider.transform, portalRoom.GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue - 100);
                if (!isHeld)
                {
                    Debug.Log("Setting " + gameObject.name + " to Interactable layer");
                    gameObject.layer = interactableLayer;
                }
                layer = currentRoomLayer;
            }
            else
            {
                Debug.LogError("An error has occured for " + name + " interactable collision. Click for details." +
                               "\nObject is in room: " + inRoom.gameObject.name + ". PortalRoom is " + portalRoom.gameObject.name + 
                               " and PortalConnectedRoom is " + portalConnectedRoom.gameObject.name);
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Portal"))
        {
            Vector3 dir = (collider.transform.position - transform.position).normalized;
            dir.y = 0; // Remove y for angle comparison - portal y is always 0
            Portal colliderPortal = collider.GetComponentInChildren<Portal>();
            Room portalRoom = colliderPortal.GetRoom();
            Room portalConnectedRoom = colliderPortal.GetConnectedRoom();

            if (math.dot(collider.transform.forward, dir) >= 0 && Vector3.Angle(collider.transform.forward, dir) < 60.0f) // Exited in the connected room
            {
                CustomUtilities.UpdateStencils(renderers, collider.transform, portalConnectedRoom.GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue + 200);
                CustomUtilities.UpdateStencils(duplicatedRenderers, collider.transform, portalConnectedRoom.GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue + 200);
                portalRoom.RemoveObjectFromRoom(transform);
                portalConnectedRoom.AddObjectToRoom(transform, true);
                transform.parent = portalConnectedRoom.gameObject.transform;
                inRoom = portalConnectedRoom;
                if (!isHeld)
                {
                    gameObject.layer = differentRoomLayer;
                }
            }
            else // Exited in the room where the portal is
            {
                CustomUtilities.UpdateStencils(renderers, collider.transform, portalRoom.GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue - 100);
                CustomUtilities.UpdateStencils(duplicatedRenderers, collider.transform, portalRoom.GetStencilValue(),
                    colliderPortal.GetRenderer().material.renderQueue - 100);
                portalConnectedRoom.RemoveObjectFromRoom(transform);
                portalRoom.AddObjectToRoom(transform, true);
                transform.parent = portalRoom.gameObject.transform;
                inRoom = portalRoom;
                if (!isHeld)
                {
                    gameObject.layer = currentRoomLayer;
                }
            }
            duplicatedMeshObject.SetActive(false);
        }
    }
}
