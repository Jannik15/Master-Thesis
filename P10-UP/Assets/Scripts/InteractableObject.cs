using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    [HideInInspector] public bool isHeld;

    // Rendering
    private GameObject duplicatedMeshObject;
    private Renderer[] renderers, duplicatedRenderers;
    private Text[] texts, duplicatedTexts;
    private TextMeshProUGUI[] TMPTexts, duplicatedTMPTexts;
    private Image[] images, duplicatedImages;

    private Portal inPortal;
    private Room portalRoom, portalConnectedRoom;
    public Room inRoom;
    private int storedLayer = -1, interactableLayer, differentRoomLayer, inPortalLayer;
    private Transform storedParent;
    private ProceduralLayoutGeneration layout;
    private PlayerCollisionHandler player;
    private bool _inInteractionCollider, _inPortalCollider, _roomChanged;
    private int activePortalCollisions, activePortalInteractionCollisions;
    [HideInInspector] public bool _alreadyDuplicated;

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

            if (!_alreadyDuplicated)
            {
                duplicatedMeshObject = Instantiate(gameObject, transform.position, transform.rotation, transform);
            }
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

            layout = FindObjectOfType<ProceduralLayoutGeneration>();
            layout.roomSwitched += OnRoomSwitch;
            layout.disabledPortal += PortalExit;
            player = FindObjectOfType<PlayerCollisionHandler>();

            differentRoomLayer = LayerMask.NameToLayer("DifferentRoom");
            interactableLayer = LayerMask.NameToLayer("Interactable");
            inPortalLayer = LayerMask.NameToLayer("ObjectInPortal");
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
        }
    }

    private void Start()
    {
        // Reset duplicated mesh transform
        duplicatedMeshObject.transform.ResetLocal();
    }

    public void AssignRoom(Room room, bool assignParent)
    {
        Transform transformToAssign = assignParent && transform.parent != null ? transform.parent : transform;
        if (room != null)
        {
            inRoom = room;
            if (duplicatedMeshObject == null)
            {
                _alreadyDuplicated = true;
                duplicatedMeshObject = Instantiate(gameObject, transform.position, transform.rotation, transform);
            }

            inRoom.AddObjectToRoom(transformToAssign, false);
        }
        else if (inRoom != null)
        {
            inRoom.RemoveObjectFromRoom(transformToAssign);
            inRoom = null;
            transformToAssign.parent = null;
        }
        else
        {
            transformToAssign.parent = null;
        }
    }

    public Room GetObjectRoom()
    {
        return inRoom;
    }

    private void OnDestroy()
    {
        // Unregister grab events
        OVRGrabber[] grabbers = Resources.FindObjectsOfTypeAll<OVRGrabber>();
        for (int i = 0; i < grabbers.Length; i++)
        {
            grabbers[i].objectGrabEvent -= GrabEventListener;
        }

        WeaponGrab[] weaponGrabbers = Resources.FindObjectsOfTypeAll<WeaponGrab>();
        for (int i = 0; i < weaponGrabbers.Length; i++)
        {
            weaponGrabbers[i].gunGrabEvent -= GrabEventListener;
        }
    }

    private void GrabEventListener(GameObject grabbedObject, bool isBeingGrabbed, OVRInput.Controller controller)
    {
        // The object being grabbed is this object
        if (gameObject == grabbedObject)
        {
            isHeld = isBeingGrabbed;
            if (isHeld) // Object picked up
            {

            }
            else // Object dropped
            {
                if (_inPortalCollider)
                {
                    gameObject.layer = inPortalLayer;
                    inRoom?.RemoveObjectFromRoom(transform);
                    inRoom = layout.currentRoom;
                    inRoom.AddObjectToRoom(transform, false);
                }
                else if (_inInteractionCollider)
                {
                    if (player.inPortal)
                    {
                        gameObject.layer = interactableLayer;
                        inRoom = layout.currentRoom;
                        inRoom.AddObjectToRoom(transform, false);
                    }
                    else
                    {
                        gameObject.layer = differentRoomLayer;
                        inRoom = portalConnectedRoom;
                        portalConnectedRoom.AddObjectToRoom(transform, false);
                        UpdateStencils(portalConnectedRoom.GetStencilValue(), 0, true, inPortal.transform);
                    }
                }
                else
                {
                    if (player.inPortal) // If player is inPortal, but object is not in InteractionCollider
                    {
                        gameObject.layer = differentRoomLayer;
                        inRoom = layout.previousRoom;
                        inRoom.AddObjectToRoom(transform, false);
                        UpdateStencils(inRoom.GetStencilValue(), 2300, true, player.thisPortal.transform);
                    }
                    else // If the player is not in a portal - dropped in current room
                    {
                        gameObject.layer = interactableLayer;
                        inRoom = layout.currentRoom;
                        if (inRoom != null)
                        {
                            inRoom.AddObjectToRoom(transform, false);
                        }
                        else // Only happens before procedural generation
                        {
                            transform.parent = layout.rooms[0].gameObject.transform;
                        }
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Portal"))
        {
            if (activePortalCollisions == 0) // If object has multiple colliders, make sure OnTriggerEnter is only executed for the first collider
            {
                if (isHeld)
                {
                    _inPortalCollider = true;
                }
                else
                {
                    inPortal = collider.GetComponentInChildren<Portal>();
                    portalRoom = inPortal.GetRoom();
                    portalConnectedRoom = inPortal.GetConnectedRoom();
                    _roomChanged = false;

                    UpdateStencils(portalConnectedRoom.GetStencilValue(), inPortal.GetRenderer().material.renderQueue + 200, false, inPortal.transform);
                    UpdateStencils(portalRoom.GetStencilValue(), inPortal.GetRenderer().material.renderQueue - 100, true, inPortal.transform);
                    gameObject.layer = inPortalLayer;
                    portalRoom.AddObjectToRoom(transform, false);
                }
            }
            activePortalCollisions++;
        }

        if (isHeld)
        {
            if (collider.CompareTag("InteractionHandler"))
            {
                if (activePortalInteractionCollisions == 0)
                {
                    inPortal = collider.GetComponentInParent<Portal>();
                    portalRoom = inPortal.GetRoom();
                    portalConnectedRoom = inPortal.GetConnectedRoom();
                    _inInteractionCollider = true;
                    if (player.inPortal)
                    {
                        UpdateStencils(portalRoom.GetStencilValue(), inPortal.GetRenderer().material.renderQueue + 200, false, inPortal.transform);
                    }
                    else
                    {
                        _roomChanged = false;
                        UpdateStencils(portalConnectedRoom.GetStencilValue(), inPortal.GetRenderer().material.renderQueue + 200, false, inPortal.transform);
                    }
                }
                activePortalInteractionCollisions++;
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Portal"))
        {
            activePortalCollisions--;
            if (activePortalCollisions == 0)
            {
                PortalExit(inPortal);
            }
        }
        if (isHeld)
        {
            if (collider.CompareTag("InteractionHandler"))
            {
                activePortalInteractionCollisions--;
                if (activePortalInteractionCollisions == 0)
                {
                    _inInteractionCollider = false;
                }
            }
        }
    }

    // External event based method for calling OnTriggerExit for colliders that get disabled before object exits
    private void PortalExit(Portal portalExited)
    {
        if (inPortal == portalExited)
        {
            if (isHeld)
            {
                _inPortalCollider = false;
                if (_roomChanged)
                {
                    activePortalInteractionCollisions = 0;
                    _inInteractionCollider = false;
                }
            }
            else
            {
                activePortalCollisions = 0; // If this was called from event, set activePortalCollisions to 0
                inRoom?.RemoveObjectFromRoom(transform);
                portalConnectedRoom.AddObjectToRoom(transform, false);
                if (!_roomChanged)
                {
                    UpdateStencils(portalConnectedRoom.GetStencilValue(), inPortal.GetRenderer().material.renderQueue + 200, true, inPortal.transform);
                    gameObject.layer = differentRoomLayer;
                }
                inPortal = null;
                portalRoom = null;
                portalConnectedRoom = null;
            }
        }
    }

    private void OnRoomSwitch(Room newRoom, Portal portal)
    {
        _roomChanged = true;
        if (isHeld)
        {
            if (portal != null)
            {
                activePortalCollisions = 0;
                _inPortalCollider = false;
                activePortalInteractionCollisions = 0;
                _inInteractionCollider = false;
            } // Else do nothing
        }
        else if (_inPortalCollider)
        {
            if (inPortal != null)
            {
                gameObject.layer = inPortalLayer;
                if (portal == inPortal)
                {
                    UpdateStencils(portalConnectedRoom.GetStencilValue(), inPortal.GetRenderer().material.renderQueue + 200, false, inPortal.transform);
                }
                else if (portal == null)
                {
                    if (newRoom == inRoom)
                    {
                        UpdateStencils(portalConnectedRoom.GetStencilValue(), inPortal.GetRenderer().material.renderQueue + 200, false, inPortal.transform);
                    }
                    else
                    {
                        inRoom.RemoveObjectFromRoom(transform);
                        inRoom = portalConnectedRoom;
                        inRoom.AddObjectToRoom(transform, false);
                        UpdateStencils(portalRoom.GetStencilValue(), inPortal.GetRenderer().material.renderQueue + 200, false, inPortal.transform);
                        UpdateStencils(portalConnectedRoom.GetStencilValue(), inPortal.GetRenderer().material.renderQueue - 100, true, inPortal.transform);
                    }
                }
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
                TMPTexts[i].material.renderQueue = renderQueue;
            }
            for (int i = 0; i < images.Length; i++)
            {
                images[i].material.SetInt("_Stencil", stencilValue);
                images[i].material.renderQueue = renderQueue;
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
