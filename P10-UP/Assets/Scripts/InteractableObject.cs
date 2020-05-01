using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    public bool isInteractable;
    [HideInInspector] public bool isHeld;

    // Rendering
    private GameObject duplicatedMeshObject;
    private Renderer[] renderers, duplicatedRenderers;
    private Text[] texts, duplicatedTexts;
    private TextMeshProUGUI[] TMPTexts, duplicatedTMPTexts;
    private Image[] images, duplicatedImages;

    private Room inRoom;
    private int storedLayer, interactableLayer, differentRoomLayer, inPortalLayer,
        mainStencil, duplicatedStencil, mainRQ, duplicatedRQ;
    private Transform storedParent;
    private ProceduralLayoutGeneration layout;

    private void Awake()
    {
        // Create and cache a duplicated version of the object, and remove unnecessary components from it
        if (transform.parent == null || transform.parent.GetComponent<InteractableObject>() == null)
        {
            // Start function for the object that will keep the script (not duplicated)
            renderers = GetComponentsInChildren<Renderer>();
            texts = GetComponentsInChildren<Text>();
            TMPTexts = GetComponentsInChildren<TextMeshProUGUI>();
            images = GetComponentsInChildren<Image>();

            duplicatedMeshObject = Instantiate(gameObject, transform.position, transform.rotation, transform);
            duplicatedRenderers = duplicatedMeshObject.GetComponentsInChildren<Renderer>();
            duplicatedTexts = duplicatedMeshObject.GetComponentsInChildren<Text>();
            duplicatedTMPTexts = duplicatedMeshObject.GetComponentsInChildren<TextMeshProUGUI>();
            duplicatedImages = duplicatedMeshObject.GetComponentsInChildren<Image>();

            #region Material instantiation
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
                    duplicatedRenderers[i].materials[j].SetColor("_MainColor", Color.red); // TODO: Remove this, its for VR Debugging
                }
            }
            //*/
            //* Text instantiation
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].material = Instantiate(texts[i].material);
            }
            for (int i = 0; i < duplicatedTexts.Length; i++)
            {
                duplicatedTexts[i].material = Instantiate(duplicatedTexts[i].material);
            }
            //*/
            //* TextMeshPro instantiation
            for (int i = 0; i < TMPTexts.Length; i++)
            {
                for (int j = 0; j < TMPTexts[i].fontMaterials.Length; j++)
                {
                    TMPTexts[i].fontMaterials[j] = Instantiate(TMPTexts[i].fontMaterials[j]);
                }
            }
            for (int i = 0; i < duplicatedTMPTexts.Length; i++)
            {
                for (int j = 0; j < duplicatedTMPTexts[i].fontMaterials.Length; j++)
                {
                    duplicatedTMPTexts[i].fontMaterials[j] = Instantiate(duplicatedTMPTexts[i].fontMaterials[j]);
                }
            }
            //*/
            //* Image instantiation
            for (int i = 0; i < images.Length; i++)
            {
                images[i].material = Instantiate(images[i].material);
            }
            for (int i = 0; i < duplicatedImages.Length; i++)
            {
                duplicatedImages[i].material = Instantiate(duplicatedImages[i].material);
            }
            //*/
            #endregion

            if (isInteractable)
            {
                // Register grab events
                OVRGrabber[] grabbers = Resources.FindObjectsOfTypeAll<OVRGrabber>();
                for (int i = 0; i < grabbers.Length; i++)
                {
                    grabbers[i].objectGrabEvent += GrabEventListener;
                }
                WeaponGrab[] weaponGrabbers = Resources.FindObjectsOfTypeAll<WeaponGrab>();
                for (int i = 0; i < weaponGrabbers.Length; i++)
                {
                    weaponGrabbers[i].gunGrabEvent += GrabEventListener;
                }

                FindObjectOfType<ProceduralLayoutGeneration>().roomSwitched += OnRoomSwitchWhenHeld;

                differentRoomLayer = LayerMask.NameToLayer("DifferentRoom");
                interactableLayer = LayerMask.NameToLayer("Interactable");
                inPortalLayer = LayerMask.NameToLayer("ObjectInPortal");
            }
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
                    allComponents[i].GetType() != typeof(Canvas) &&
                    allComponents[i].GetType() != typeof(CanvasRenderer) &&
                    allComponents[i].GetType() != typeof(Text) &&
                    allComponents[i].GetType() != typeof(Image) &&
                    allComponents[i].GetType() != typeof(GraphicRaycaster) &&
                    allComponents[i].GetType() != typeof(SkinnedMeshRenderer))
                {
                    Destroy(allComponents[i]);
                }
            }
            gameObject.SetActive(false);
            transform.ResetLocal();
        }
    }

    public void AssignRoom(Room room)
    {
        if (room != null)
        {
            inRoom = room;
            if (duplicatedMeshObject == null)
            {
                Debug.Log("Adding " + transform.name + " to " + inRoom.gameObject.name + " before duplicatedObjects have been instantiated");
            }
            inRoom.AddObjectToRoom(transform, false);
        }
        else if (inRoom != null)
        {
            inRoom.RemoveObjectFromRoom(transform);
            inRoom = null;
        }
    }

    public Room GetObjectRoom()
    {
        return inRoom;
    }

    private void GrabEventListener(GameObject grabbedObject, bool isBeingGrabbed)
    {
        // The object being grabbed is this object
        if (gameObject == grabbedObject)
        {
            isHeld = isBeingGrabbed;
            if (!isHeld)
            {
                gameObject.layer = storedLayer >= 0 ? storedLayer : gameObject.layer;
                storedLayer = -1;
                if (storedParent != null)
                {
                    transform.parent = storedParent;
                    storedParent = null;
                }
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

            if (mainStencil != portalConnectedRoom.GetStencilValue())
            {
                duplicatedStencil = portalConnectedRoom.GetStencilValue();
                duplicatedRQ = colliderPortal.GetRenderer().material.renderQueue + 200; // Always 2300
            }
            else
            {
                duplicatedStencil = portalRoom.GetStencilValue();
                duplicatedRQ = colliderPortal.GetRenderer().material.renderQueue - 100; // Always 2100
            }

            if (!isHeld && isInteractable)
            {
                storedLayer = gameObject.layer;
                gameObject.layer = inPortalLayer;
            }
            UpdateStencils(duplicatedStencil, duplicatedRQ, false, collider.transform);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Portal"))
        {
            Vector3 dir = (transform.position - collider.transform.position).normalized; // From portal to interactableObject
            dir.y = 0; // Remove y for angle comparison - portal y is always 0
            Portal colliderPortal = collider.GetComponentInChildren<Portal>();
            Room portalRoom = colliderPortal.GetRoom();
            Room portalConnectedRoom = colliderPortal.GetConnectedRoom();
            duplicatedMeshObject.SetActive(false);

            if (math.dot(collider.transform.forward, dir) >= 0 && Vector3.Angle(collider.transform.forward, dir) < 60.0f) // Exited in the connected room
            {
                mainStencil = portalConnectedRoom.GetStencilValue();
                mainRQ = colliderPortal.GetRenderer().material.renderQueue + 200;
                if (isInteractable)
                {
                    inRoom = portalConnectedRoom;
                    if (isHeld)
                    {
                        storedParent = portalConnectedRoom.gameObject.transform;
                        storedLayer = differentRoomLayer;
                        duplicatedMeshObject.SetActive(true);
                    }
                    else
                    {
                        portalConnectedRoom.AddObjectToRoom(transform, true);
                        portalRoom.RemoveObjectFromRoom(transform);
                        transform.parent = portalConnectedRoom.gameObject.transform;
                        gameObject.layer = differentRoomLayer;
                    }
                }
            }
            else // Exited in the room where the portal is
            {
                mainStencil = portalRoom.GetStencilValue();
                mainRQ = colliderPortal.GetRenderer().material.renderQueue - 100;
                if (isInteractable)
                {
                    inRoom = portalRoom;
                    if (isHeld)
                    {
                        storedParent = portalRoom.gameObject.transform;
                        storedLayer = interactableLayer;
                    }
                    else
                    {
                        portalRoom.AddObjectToRoom(transform, true);
                        portalConnectedRoom.RemoveObjectFromRoom(transform);
                        transform.parent = portalRoom.gameObject.transform;
                        gameObject.layer = interactableLayer;
                    }
                }
            }
            if (!isHeld)
            {
                Debug.Log(gameObject.name + " exited portal, has been assigned mainStencil: " + mainStencil);
                UpdateStencils(mainStencil, mainRQ, true, collider.transform);
            }
        }
    }

    private void OnRoomSwitchWhenHeld(Room newRoom, Portal portal)
    {
        if (isHeld)
        {
            if (newRoom == inRoom) // Player is in the same room as the object they are holding
            {
                // TODO: what about when you go through portal and the object is still in the portal - also do something for DuplicatedRenderers?
                UpdateStencils(0, 2000, true, portal == null ? null : portal.transform);
            }
            else
            {
                UpdateStencils(mainStencil, mainRQ, true, portal == null ? null : portal.transform);
            }
        }
    }

    private void UpdateStencils(int stencilValue, int renderQueue, bool mainObject, Transform portalMatrix)
    {
        if (mainObject)
        {
            CustomUtilities.UpdateStencils(renderers, portalMatrix, stencilValue, renderQueue);
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].material.SetInt("_Stencil", stencilValue);
                texts[i].material.renderQueue = renderQueue;
            }
            for (int i = 0; i < TMPTexts.Length; i++)
            {
                TMPTexts[i].material.SetInt("_Stencil", stencilValue);
                TMPTexts[i].material.renderQueue = mainRQ;
            }
            for (int i = 0; i < images.Length; i++)
            {
                images[i].material.SetInt("_Stencil", stencilValue);
                images[i].material.renderQueue = mainRQ;
            }
        }
        else
        {
            CustomUtilities.UpdateStencils(duplicatedRenderers, portalMatrix, stencilValue, renderQueue);
            for (int i = 0; i < duplicatedTexts.Length; i++)
            {
                duplicatedTexts[i].material.SetInt("_Stencil", stencilValue);
                duplicatedTexts[i].material.renderQueue = renderQueue;
            }
            for (int i = 0; i < duplicatedTMPTexts.Length; i++)
            {
                duplicatedTMPTexts[i].material.SetInt("_Stencil", stencilValue);
                duplicatedTMPTexts[i].material.renderQueue = renderQueue;
            }
            for (int i = 0; i < duplicatedImages.Length; i++)
            {
                duplicatedImages[i].material.SetInt("_Stencil", stencilValue);
                duplicatedImages[i].material.renderQueue = renderQueue;
            }
        }
    }
}
