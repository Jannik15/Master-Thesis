using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class WeaponGrab : MonoBehaviour
{
    OVRGrabber oVRGrabber;
    //SimpleShoot simpleShoot;
    Rigidbody rbR;
    Rigidbody rbL;
    public Gun gunScript;
    public LayerMask layerMask;
    public Transform trackingSpace;
    public Shader highlightMaterialR;
    public Shader highlightMaterialL;
    public Camera camera;
    public GameObject rightHandModel;
    public GameObject leftHandModel;
    public Material HandDefaultMaterial;
    public Material RightHandHighlightMaterial;
    public Material LeftHandHighlightMaterial;
    public bool handRight, isHeld;
    private bool holdingGunRight = false;
    private bool holdingGunLeft = false;
    private Vector3 gunPositionRight = new Vector3(0.024f, 0.014f, 0.0798f);
    private Vector3 gunRotationRight = new Vector3(90f, -80f, 9f);
    private Vector3 gunPositionLeft = new Vector3(-0.024f, 0.014f, 0.076f);
    private Vector3 gunRotationLeft = new Vector3(263.62f, -181.869f, 88.99999f);
    public Shader defaultMaterial;
    public Color rightHighlight, leftHighlight;
    private ProceduralLayoutGeneration layout;
    
    private Transform _selectionR;
    private Transform _selectionL;
    [SerializeField]
    private GameObject gun = null;
    [SerializeField]
    private GameObject currentGunRight = null;
    [SerializeField]
    private GameObject currentGunLeft = null;

    public event Action<GameObject, bool> gunGrabEvent;
    public float fireRate = 3f;
    private float nextTimeToFire = 0f;

    void PickUpRight(RaycastHit hitRight)
    {
        currentGunRight = hitRight.collider.transform.gameObject;
        currentGunRight.transform.parent = transform;
        currentGunRight.transform.localPosition = gunPositionRight;
        currentGunRight.transform.localEulerAngles = gunRotationRight;
        currentGunRight.GetComponentInChildren<InteractableObject>().AssignRoom(null);
        //simpleShoot = currentGunRight.GetComponent<SimpleShoot>();
        rbR = currentGunRight.GetComponent<Rigidbody>();
        rbR.isKinematic = true;

        holdingGunRight = true;
        //simpleShoot.enabled = true; //on
        oVRGrabber.enabled = false; //off
        gunGrabEvent?.Invoke(currentGunRight, true);
    }

    void DropRight()
    {
        holdingGunRight = false;
        //simpleShoot.enabled = false; //off
        oVRGrabber.enabled = true; //on
        rbR.isKinematic = false;
        currentGunRight.GetComponentInChildren<InteractableObject>().AssignRoom(layout.currentRoom);
        rbR.velocity = trackingSpace.rotation * OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
        rbR.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);
        gunGrabEvent?.Invoke(currentGunRight, false);
        gun = null;
        currentGunRight = null;
    }

    void PickUpLeft(RaycastHit hitLeft)
    {
        currentGunLeft = hitLeft.collider.transform.gameObject;
        currentGunLeft.transform.parent = transform;
        currentGunLeft.transform.localPosition = gunPositionLeft;
        currentGunLeft.transform.localEulerAngles = gunRotationLeft;
        currentGunLeft.GetComponent<InteractableObject>().AssignRoom(null);
        //simpleShoot = currentGunLeft.GetComponent<SimpleShoot>();
        rbL = currentGunLeft.GetComponent<Rigidbody>();
        rbL.isKinematic = true;

        holdingGunLeft = true;
        //simpleShoot.enabled = true; //on
        oVRGrabber.enabled = false; //off
        gunGrabEvent?.Invoke(currentGunLeft, true);
    }

    void DropLeft()
    {
        holdingGunLeft = false;
        //simpleShoot.enabled = false; //off
        oVRGrabber.enabled = true; //on
        rbL.isKinematic = false;
        currentGunLeft.GetComponent<InteractableObject>().AssignRoom(layout.currentRoom);
        rbL.velocity = trackingSpace.rotation * OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
        rbL.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.LTouch);
        gunGrabEvent?.Invoke(currentGunLeft, false);
        gun = null;
        currentGunLeft = null;
    }

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
        if (holdingGunRight == false){
            oVRGrabber.enabled = true; //on
        }
        if (holdingGunLeft == false){
            oVRGrabber.enabled = true;
        }
    }

    private void GrabEventListener(GameObject grabbedObject, bool isBeingGrabbed)
    {
        // The object being grabbed is this object
        if (handRight == true && gameObject == grabbedObject)
        {
            isHeld = isBeingGrabbed;
            Debug.Log("Grabbing Right Hand");
        }
        else if (handRight != true && gameObject == grabbedObject)
        {
            isHeld = isBeingGrabbed;
            Debug.Log("Grabbing Left Hand");

        }
    }

    void Update()
    {
        //Pickup Raycasting
        if (_selectionR != null)
        {
            Renderer[] selectionRendererR = _selectionR.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < selectionRendererR.Length; i++)
            {
                for (int j = 0; j < selectionRendererR[i].materials.Length; j++)
                {
                    selectionRendererR[i].materials[j].shader = null;
                    selectionRendererR[i].materials[j].shader = defaultMaterial;
                }
            }
            rightHandModel.GetComponent<Renderer>().material = HandDefaultMaterial;
            gun = null;
            _selectionR = null;
        }

        if (_selectionL != null)
        {
            Renderer[] selectionRendererL = _selectionL.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < selectionRendererL.Length; i++)
            {
                for (int j = 0; j < selectionRendererL[i].materials.Length; j++)
                {
                    selectionRendererL[i].materials[j].shader = null;
                    selectionRendererL[i].materials[j].shader = defaultMaterial;
                }
            }
            leftHandModel.GetComponent<Renderer>().material = HandDefaultMaterial;
            gun = null;
            _selectionL = null;
        }

        if (handRight && holdingGunRight != true) //right Hand cast
        {

            //var ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitRight;
            //if(Physics.Raycast(transform.position, transform.forward, out hitRight, 5, layerMask))   //Vr (Not tested)
            if (Physics.SphereCast(transform.position, 0.15f, transform.forward, out hitRight, 5, layerMask))
            //if (Physics.CapsuleCast(transform.position + (transform.up * 0.5f), transform.position + (transform.up * -0.5f), 0.15f, transform.forward, out hitRight, 5, layerMask))


            //if (Physics.SphereCast(camera.transform.position, 0.3f, camera.transform.forward, out hit, 10)) // Non VR
            {
                if (hitRight.transform.CompareTag("Weapon"))
                {
                    gun = hitRight.collider.transform.gameObject;

                    var selectionR = hitRight.transform;
                    Renderer[] selectionRendererR = selectionR.GetComponentsInChildren<Renderer>();



                    if (selectionRendererR != null)
                    {
                        for (int i = 0; i < selectionRendererR.Length; i++)
                        {
                            for (int j = 0; j < selectionRendererR[i].materials.Length; j++)
                            {
                                selectionRendererR[i].materials[j].shader = highlightMaterialR;
                                selectionRendererR[i].materials[j].SetColor("outlineColor", rightHighlight);
                            }
                        }
                    rightHandModel.GetComponent<Renderer>().material = RightHandHighlightMaterial;
                    }
                    _selectionR = selectionR;
                }

            }

             //Right Hand pickup
            if (OVRInput.Get(OVRInput.RawButton.RHandTrigger) && gun != null || Input.GetKeyDown(KeyCode.E) && gun != null)
            {
                if (holdingGunRight == false && isHeld == false)
                {
                    if (hitRight.transform.CompareTag("Weapon"))
                    {
                        if(hitRight.transform.parent == null || hitRight.transform.parent.GetComponent<WeaponGrab>() == null)
                        {
                            PickUpRight(hitRight);
                        }
                        else
                        {
                            hitRight.transform.parent.GetComponent<WeaponGrab>().DropLeft();
                            PickUpRight(hitRight);
                        }
                    }
                }
            }
        }

        if (!handRight && holdingGunLeft != true)
        {

            //var ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitLeft;
            //if(Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))   //Vr (Not tested)
            if (Physics.SphereCast(transform.position, 0.15f, transform.forward, out hitLeft, 5, layerMask))

            //if (Physics.SphereCast(camera.transform.position, 0.3f, camera.transform.forward, out hit, 10)) // Non VR
            {
                if (hitLeft.transform.CompareTag("Weapon"))
                {
                    gun = hitLeft.collider.transform.gameObject;

                    var selectionL = hitLeft.transform;
                    Renderer[] selectionRendererL = selectionL.GetComponentsInChildren<Renderer>();



                    if (selectionRendererL != null)
                    {
                        for (int i = 0; i < selectionRendererL.Length; i++)
                        {
                            for (int j = 0; j < selectionRendererL[i].materials.Length; j++)
                            {
                                selectionRendererL[i].materials[j].shader = highlightMaterialL;
                                selectionRendererL[i].materials[j].SetColor("outlineColor", leftHighlight);
                            }
                        }
                        leftHandModel.GetComponent<Renderer>().material = LeftHandHighlightMaterial;
                    }
                    _selectionL = selectionL;
                }

            }
            //Left hand Pickup
            if (OVRInput.Get(OVRInput.RawButton.LHandTrigger) && gun != null || Input.GetKeyDown(KeyCode.R) && gun != null)
            {
                if (holdingGunLeft == false && isHeld == false)
                {
                    if (hitLeft.transform.CompareTag("Weapon"))
                    {
                        if (hitLeft.transform.parent == null || hitLeft.transform.parent.GetComponent<WeaponGrab>() == null)
                        {
                            PickUpLeft(hitLeft);
                        }
                        else
                        {
                            hitLeft.transform.parent.GetComponent<WeaponGrab>().DropRight();
                            PickUpLeft(hitLeft);
                        }
                    }
                }
            }
        }

        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) && Time.time >= nextTimeToFire && currentGunRight != null)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            currentGunRight.GetComponent<Gun>().Shoot();
        }

        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) && Time.time >= nextTimeToFire && currentGunLeft != null)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            currentGunLeft.GetComponent<Gun>().Shoot();
        }


        //Right Hand Drop
        if (OVRInput.GetDown(OVRInput.Button.Two) && currentGunRight != null || Input.GetKeyDown(KeyCode.H) && currentGunRight != null)
        {
            if (holdingGunRight == true)
            {
                DropRight();
            }
        }

        //Left Hand Drop
        if (OVRInput.GetDown(OVRInput.Button.Four) && currentGunLeft != null || Input.GetKeyDown(KeyCode.J) && currentGunLeft != null)
        {
            if (holdingGunLeft == true)
            {
                DropLeft();
            }
        }

        // Jannik PC testing
        if (!holdingGunRight && handRight)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                currentGunRight = FindObjectOfType<Gun>().gameObject;
                currentGunRight.transform.parent = transform;
                currentGunRight.transform.localPosition = gunPositionRight;
                currentGunRight.transform.localEulerAngles = gunRotationRight;
                currentGunRight.GetComponentInChildren<InteractableObject>().AssignRoom(null);
                //simpleShoot = currentGunRight.GetComponent<SimpleShoot>();
                rbR = currentGunRight.GetComponent<Rigidbody>();
                rbR.isKinematic = true;

                holdingGunRight = true;
                //simpleShoot.enabled = true; //on
                oVRGrabber.enabled = false; //off
                gunGrabEvent?.Invoke(currentGunRight, true);
            }
        }
        
    }
}
