using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGrab : MonoBehaviour
{
    #region Hardcoded tutorial
    public bool tutorialMode;
    public GameObject prevSlide;
    public GameObject nextSlide;
    public GameObject part4;

    public GameObject prevSlide2;
    public GameObject nextSlide2;
    public GameObject part6;

    public GameObject prevSlide3;
    public GameObject nextSlide3;
    #endregion

    public LayerMask layerMask;
    [SerializeField] private Transform trackingSpace;
    [SerializeField] private FingerPress finger;
    [SerializeField] private float fireRate = 3f, fireDelay;
    [SerializeField] private WeaponGrab otherHand;
    [SerializeField] private Vector3 weaponInHandPosition, weaponInHandRotation, cardInHandPosition, cardInHandRotation;
    [SerializeField] private Color weaponHighlight;
    [SerializeField] private Material handHighlightMaterial;
    [SerializeField] private GameObject handModel;
    [SerializeField] private bool handRight;
    [HideInInspector] public Transform weaponHit, weaponHeld;
    public event Action<GameObject, bool, OVRInput.Controller, bool> gunGrabEvent;

    private bool holdingNonWeaponObject;
    private Animator weaponAnim;
    private Rigidbody weaponRb;
    private Material handDefaultMaterial;
    private ProceduralLayoutGeneration layout;
    private OVRGrabber oVRGrabber;
    private Renderer[] weaponHitRends;
    private Renderer handModelRend;
    private List<List<Color>> storedColors = new List<List<Color>>();
    private Gun currentGun;
    private Transform newHit;

    #region Old

    /*
    [Header("Deprecated")]
    public Gun gunScript;
    private List<List<Color>> storedRightColors = new List<List<Color>>(), storedLeftColors = new List<List<Color>>();
    public Camera camera;
    public GameObject rightHandModel;
    public GameObject leftHandModel;
    public Material HandDefaultMaterial;
    public Material RightHandHighlightMaterial;
    public Material LeftHandHighlightMaterial;
    public bool isHeld;
    private bool holdingGun;
    public Vector3 gunPositionRight = new Vector3(0.024f, 0.014f, 0.0798f);
    public Vector3 gunRotationRight = new Vector3(90f, -80f, 9f);
    public Vector3 gunPositionLeft = new Vector3(-0.024f, 0.014f, 0.076f);
    public Vector3 gunRotationLeft = new Vector3(263.62f, -181.869f, 88.99999f);
    public Color rightHighlight, leftHighlight;
    Rigidbody rbR;
    Rigidbody rbL;
    [SerializeField] private GameObject gun = null;
    [SerializeField] private GameObject currentGunRight = null;
    [SerializeField] private GameObject currentGunLeft = null;
    private Transform objectHitR, objectHitL;
    private Renderer[] objHitRendsR, objHitRendsL;
    public event Action<Transform> weaponGrabDetection;
    //*/

    #endregion

    void Start()
    {
        // Register grab events
        OVRGrabber[] grabbers = Resources.FindObjectsOfTypeAll<OVRGrabber>();
        for (int i = 0; i < grabbers.Length; i++)
        {
            grabbers[i].objectGrabEvent += GrabEventListener;
        }

        oVRGrabber = GetComponent<OVRGrabber>();
        layout = FindObjectOfType<ProceduralLayoutGeneration>();
        if (handModel == null)
        {
            handModel = GetComponentInChildren<SkinnedMeshRenderer>().gameObject;
            Debug.LogError(gameObject.name +
                           " does not have a Hand Model assigned - searching for skinned mesh renderers in children.");
        }

        handModelRend = handModel.GetComponentInChildren<Renderer>();
        handDefaultMaterial = handModelRend.material;
    }

    private void GrabEventListener(GameObject grabbedObject, bool isBeingGrabbed, OVRInput.Controller controller, bool empty)
    {
        // The object being grabbed is this object
        if (handRight && controller == OVRInput.Controller.RTouch ||
            !handRight && controller == OVRInput.Controller.LTouch)
        {
            holdingNonWeaponObject = isBeingGrabbed;
        }
    }

    void Update()
    {
        if (holdingNonWeaponObject) return;

        if (weaponHeld == null) // Can't pick up new weapon if another weapon is already held
        {
            // RayCast pickup
            if (Physics.SphereCast(transform.position, 0.15f, transform.forward, out RaycastHit hit, 5, layerMask))
            {
                if (hit.transform.CompareTag("Weapon") && hit.transform != weaponHit &&
                    hit.transform != otherHand.weaponHit || hit.transform.CompareTag("Keycard") &&
                    hit.transform != weaponHit && hit.transform != otherHand.weaponHit)
                {
                    if (weaponHit != null) // When raycast hit's a new weapon, hit has had its materials reset
                    {
                        for (int i = 0; i < weaponHitRends.Length; i++)
                        {
                            for (int j = 0; j < weaponHitRends[i].materials.Length; j++)
                            {
                                weaponHitRends[i].materials[j].SetColor("_MainColor", storedColors[i][j]);
                            }
                        }
                    }

                    weaponHit = hit.transform;
                    weaponHitRends = weaponHit.GetComponentsInChildren<Renderer>();
                    storedColors.Clear();
                    if (weaponHitRends != null)
                    {
                        for (int i = 0; i < weaponHitRends.Length; i++)
                        {
                            storedColors.Add(new List<Color>(weaponHitRends[i].materials.Length));
                            for (int j = 0; j < weaponHitRends[i].materials.Length; j++)
                            {
                                storedColors[i].Add(weaponHitRends[i].materials[j].GetColor("_MainColor"));
                                weaponHitRends[i].materials[j].SetColor("_MainColor", weaponHighlight);
                            }
                        }

                        handModelRend.material = handHighlightMaterial;
                    }
                }
            }
            else if (weaponHit != null) // When raycast stops hitting the weapon
            {
                for (int i = 0; i < weaponHitRends.Length; i++)
                {
                    for (int j = 0; j < weaponHitRends[i].materials.Length; j++)
                    {
                        weaponHitRends[i].materials[j].SetColor("_MainColor", storedColors[i][j]);
                    }
                }

                handModelRend.material = handDefaultMaterial;
                weaponHit = null;
            }

            if (weaponHit != null)
            {
                // Register pickup input
                bool inputRegistered = false;
                if (handRight && (OVRInput.Get(OVRInput.RawButton.RHandTrigger) || Input.GetKeyDown(KeyCode.E)))
                {
                    inputRegistered = true;
                }
                else if (!handRight && (OVRInput.Get(OVRInput.RawButton.LHandTrigger) || Input.GetKeyDown(KeyCode.E)))
                {
                    inputRegistered = true;
                }

                if (inputRegistered && weaponHit.CompareTag("Weapon"))
                {
                    if (weaponHit == otherHand.weaponHeld)
                    {
                        otherHand.DropWeapon();
                    }

                    PickUpWeapon(weaponHit);
                }
                else if (inputRegistered && weaponHit.CompareTag("Keycard"))
                {
                    if (weaponHit == otherHand.weaponHeld)
                    {
                        otherHand.DropWeapon();
                    }

                    PickUpKeycard(weaponHit);
                }
            }
        }
        else // Weapon is held
        {
            if (Time.time >= fireDelay &&
                (handRight && OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) ||
                 !handRight && OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger)))
            {
                fireDelay = Time.time + 1f / fireRate;
                currentGun.Shoot();
            }

            if (handRight && (OVRInput.GetDown(OVRInput.Button.Two) || Input.GetKeyDown(KeyCode.H)) ||
                !handRight && (OVRInput.GetDown(OVRInput.Button.Four) || Input.GetKeyDown(KeyCode.J)))
            {
                DropWeapon();
            }
        }

        #region Old for left hand

        /*
        if (!handRight && !holdingGun)
        {
            if (Physics.SphereCast(transform.position, 0.15f, transform.forward, out RaycastHit hitLeft, 5, layerMask))
            {
                if (hitLeft.transform.CompareTag("Weapon") && hitLeft.transform != objectHitL && hitLeft.transform != objectHitR)
                {
                    if (objectHitL != null)
                    {
                        for (int i = 0; i < objHitRendsL.Length; i++)
                        {
                            for (int j = 0; j < objHitRendsL[i].materials.Length; j++)
                            {
                                objHitRendsL[i].materials[j].SetColor("_MainColor", storedLeftColors[i][j]);
                            }
                        }
                    }

                    objectHitL = hitLeft.transform;
                    weaponGrabDetection?.Invoke(objectHitL);
                    gun = hitLeft.collider.transform.gameObject;
                    objHitRendsL = objectHitL.GetComponentsInChildren<Renderer>();
                    storedLeftColors.Clear();
                    if (objHitRendsL != null)
                    {
                        for (int i = 0; i < objHitRendsL.Length; i++)
                        {
                            storedLeftColors.Add(new List<Color>(objHitRendsL[i].materials.Length));
                            for (int j = 0; j < objHitRendsL[i].materials.Length; j++)
                            {
                                storedLeftColors[i].Add(objHitRendsL[i].materials[j].GetColor("_MainColor"));
                                objHitRendsL[i].materials[j].SetColor("_MainColor", leftHighlight);
                            }
                        }

                        leftHandModel.GetComponent<Renderer>().material = LeftHandHighlightMaterial;
                    }
                }
            }
            else if (objectHitL != null)
            {
                for (int i = 0; i < objHitRendsL.Length; i++)
                {
                    for (int j = 0; j < objHitRendsL[i].materials.Length; j++)
                    {
                        objHitRendsL[i].materials[j].SetColor("_MainColor", storedLeftColors[i][j]);
                    }
                }
                leftHandModel.GetComponent<Renderer>().material = HandDefaultMaterial;
                gun = null;
                objectHitL = null;
                weaponGrabDetection?.Invoke(objectHitL);
            }

            //Left hand Pickup
            if (OVRInput.Get(OVRInput.RawButton.LHandTrigger) && gun != null ||
                Input.GetKeyDown(KeyCode.R) && gun != null)
            {
                if (objectHitL != null)
                {
                    if (objectHitL.parent != null && objectHitL.parent.GetComponent<WeaponGrab>() != null)
                    {
                        objectHitL.parent.GetComponent<WeaponGrab>().DropRight();
                    }
                    PickUpLeft(objectHitL);
                }
            }
        }
        //*/

        #endregion
    }

    private void PickUpKeycard(Transform cardToPickUp)
    {
        if (cardToPickUp.gameObject.GetComponentInParent<UIWatch>() != null)
        {
            cardToPickUp.gameObject.GetComponentInParent<UIWatch>().gameObject.GetComponentInChildren<WristPlateUI>()
                .TakeOutCard(cardToPickUp.gameObject);
        }

        weaponHeld = cardToPickUp;
        for (int i = 0; i < weaponHitRends.Length; i++)
        {
            for (int j = 0; j < weaponHitRends[i].materials.Length; j++)
            {
                weaponHitRends[i].materials[j].SetColor("_MainColor", storedColors[i][j]);
            }
        }

        handModelRend.material = handDefaultMaterial;

        if (!tutorialMode)
        {
            weaponHeld.GetComponentInChildren<InteractableObject>().inRoom.RemoveObjectFromRoom(weaponHeld);
        }

        weaponHeld.parent = transform;
        weaponHeld.localPosition = cardInHandPosition;
        weaponHeld.localEulerAngles = cardInHandRotation;
        weaponAnim = weaponHeld.GetComponent<Animator>();
        weaponAnim.enabled = false;
        weaponRb = weaponHeld.GetComponent<Rigidbody>();
        weaponRb.constraints = RigidbodyConstraints.FreezeAll;
        oVRGrabber.enabled = false;
        currentGun = null;
        gunGrabEvent?.Invoke(weaponHeld.gameObject, true, handRight ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch, false);
        finger?.gameObject.SetActive(false);

        if (tutorialMode)
        {
            if (prevSlide3.activeSelf)
            {
                prevSlide3.SetActive(false);
                nextSlide3.SetActive(true);
            }
        }

    }

    private void PickUpWeapon(Transform weaponToPickUp)
    {
        if (!tutorialMode)
        {
            weaponHeld = weaponToPickUp;
            for (int i = 0; i < weaponHitRends.Length; i++)
            {
                for (int j = 0; j < weaponHitRends[i].materials.Length; j++)
                {
                    weaponHitRends[i].materials[j].SetColor("_MainColor", storedColors[i][j]);
                }
            }

            handModelRend.material = handDefaultMaterial;
            weaponHeld.GetComponentInChildren<InteractableObject>().AssignRoom(null, false);
            weaponHeld.parent = transform;
            weaponHeld.localPosition = weaponInHandPosition;
            weaponHeld.localEulerAngles = weaponInHandRotation;
            weaponRb = weaponHeld.GetComponentInChildren<Rigidbody>();
            weaponRb.constraints = RigidbodyConstraints.FreezeAll;
            oVRGrabber.enabled = false;
            currentGun = weaponHeld.GetComponentInChildren<Gun>();
            finger?.gameObject.SetActive(false);
            gunGrabEvent?.Invoke(weaponHeld.gameObject, true,
                handRight ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch, false);
        }
        else
        {
            weaponHeld = weaponToPickUp;
            for (int i = 0; i < weaponHitRends.Length; i++)
            {
                for (int j = 0; j < weaponHitRends[i].materials.Length; j++)
                {
                    weaponHitRends[i].materials[j].SetColor("_MainColor", storedColors[i][j]);
                }
            }

            handModelRend.material = handDefaultMaterial;
            weaponHeld.parent = transform;
            weaponHeld.localPosition = weaponInHandPosition;
            weaponHeld.localEulerAngles = weaponInHandRotation;
            weaponRb = weaponHeld.GetComponentInChildren<Rigidbody>();
            weaponRb.constraints = RigidbodyConstraints.FreezeAll;
            oVRGrabber.enabled = false;
            currentGun = weaponHeld.GetComponentInChildren<Gun>();
            finger?.gameObject.SetActive(false);
            gunGrabEvent?.Invoke(weaponHeld.gameObject, true,
                handRight ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch, false);
            
            if (tutorialMode)
            {
                if (prevSlide.activeSelf)
                {
                    prevSlide.SetActive(false);
                    nextSlide.SetActive(true);
                    part4.SetActive(true);
                }
            }

        }
    }

    public void DropWeapon()
    {
        oVRGrabber.enabled = true;
        weaponRb.constraints = RigidbodyConstraints.None;

        if (!tutorialMode)
        {
            weaponHeld.GetComponentInChildren<InteractableObject>().AssignRoom(layout.currentRoom, false);
        }

        if (handRight)
        {
            weaponHeld.parent = null;
            weaponRb.velocity = trackingSpace.rotation * OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            weaponRb.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);
            gunGrabEvent?.Invoke(weaponHeld.gameObject, false, OVRInput.Controller.RTouch, false);
            finger?.gameObject.SetActive(true);
        }
        else
        {
            weaponHeld.parent = null;
            weaponRb.velocity = trackingSpace.rotation * OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
            weaponRb.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.LTouch);
            gunGrabEvent?.Invoke(weaponHeld.gameObject, false, OVRInput.Controller.LTouch, false);
        }
        weaponHeld = null;

        if (tutorialMode)
        {
            if (prevSlide2.activeSelf)
            {
                if (part4.activeSelf)
                {
                    part4.SetActive(false);
                }

                prevSlide2.SetActive(false);
                nextSlide2.SetActive(true);
                part6.SetActive(true);
            }
        }

    }
}