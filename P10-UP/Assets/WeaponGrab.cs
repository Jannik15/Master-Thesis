using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class WeaponGrab : MonoBehaviour
{
    OVRGrabber oVRGrabber;
    SimpleShoot simpleShoot;
    Rigidbody rbR;
    Rigidbody rbL;
    public Shader highlightMaterial;
    public Camera camera;
    public GameObject rightHandModel;
    public GameObject leftHandModel;
    public Material HandDefaultMaterial;
    public Material RightHandHighlightMaterial;
    public Material LeftHandHighlightMaterial;
    public bool handRight;
    private bool holdingGunRight = false;
    private bool holdingGunLeft = false;
    private Vector3 gunPositionRight = new Vector3(0.0481f,0.0184f,-0.01927197f);
    private Vector3 gunRotationRight = new Vector3(0f, 9.126f, 90f);
    private Vector3 gunPositionLeft = new Vector3(-0.03830f,0.03422651f,-0.03557706f);
    private Vector3 gunRotationLeft = new Vector3(0f, -9.126f, -90f);
    private Shader defaultMaterial = null;
    
    private Transform _selectionR;
    private Transform _selectionL;
    [SerializeField]
    private GameObject gun = null;
    [SerializeField]
    private GameObject currentGunRight = null;
    [SerializeField]
    private GameObject currentGunLeft = null;
    



    void PickUpRight(RaycastHit hitRight)
    {
        currentGunRight = hitRight.collider.transform.parent.gameObject;
        currentGunRight.transform.parent = transform;
        currentGunRight.transform.localPosition = gunPositionRight;
        currentGunRight.transform.localEulerAngles = gunRotationRight;
        simpleShoot = currentGunRight.GetComponent<SimpleShoot>();
        rbR = currentGunRight.GetComponent<Rigidbody>();
        rbR.isKinematic = true;

        holdingGunRight = true;
        simpleShoot.enabled = true; //on
        oVRGrabber.enabled = false; //off

    }

    void DropRight()
    {
        holdingGunRight = false;
        simpleShoot.enabled = false; //off
        oVRGrabber.enabled = true; //on
        rbR.isKinematic = false;
        currentGunRight.transform.parent = null;
        rbR.AddForce(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch));
        gun = null;
        currentGunRight = null;
    }

    void PickUpLeft(RaycastHit hitLeft)
    {
        currentGunLeft = hitLeft.collider.transform.parent.gameObject;
        currentGunLeft.transform.parent = transform;
        currentGunLeft.transform.localPosition = gunPositionLeft;
        currentGunLeft.transform.localEulerAngles = gunRotationLeft;
        simpleShoot = currentGunLeft.GetComponent<SimpleShoot>();
        rbL = currentGunLeft.GetComponent<Rigidbody>();
        rbL.isKinematic = true;

        holdingGunLeft = true;
        simpleShoot.enabled = true; //on
        oVRGrabber.enabled = false; //off

    }

    void DropLeft()
    {
        holdingGunLeft = false;
        simpleShoot.enabled = false; //off
        oVRGrabber.enabled = true; //on
        rbL.isKinematic = false;
        currentGunLeft.transform.parent = null;
        rbL.velocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
        gun = null;
        currentGunLeft = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Debug.DrawLine(transform.position, transform.position + camera.transform.forward * 5);
        Gizmos.DrawWireSphere(transform.position + camera.transform.forward * 5, 0.35f);
    }

    void Start()
    {
        oVRGrabber = GetComponent<OVRGrabber>();

        if (holdingGunRight == false){
            oVRGrabber.enabled = true; //on
        }
        if (holdingGunLeft == false){
            oVRGrabber.enabled = true;
        }
    }

    void Update()
    {
        //Pickup Raycasting
        if (_selectionR != null)
        {
            Renderer[] selectionRenderer = _selectionR.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < selectionRenderer.Length; i++)
            {
                for (int j = 0; j < selectionRenderer[i].materials.Length; j++)
                {
                    selectionRenderer[i].materials[j].shader = defaultMaterial;
                }
            }
            rightHandModel.GetComponent<Renderer>().material = HandDefaultMaterial;
            gun = null;
            _selectionR = null;
        }

        if (_selectionL != null)
        {
            Renderer[] selectionRenderer = _selectionL.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < selectionRenderer.Length; i++)
            {
                for (int j = 0; j < selectionRenderer[i].materials.Length; j++)
                {
                    selectionRenderer[i].materials[j].shader = defaultMaterial;
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
            //if(Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))   //Vr (Not tested)
            if (Physics.SphereCast(transform.position, 0.1f, transform.forward, out hitRight, 10))

            //if (Physics.SphereCast(camera.transform.position, 0.3f, camera.transform.forward, out hit, 10)) // Non VR
            {
                if (hitRight.transform.CompareTag("Weapon"))
                {
                    Debug.Log("Weapon Hit");
                    gun = hitRight.collider.transform.parent.gameObject;

                    var selectionR = hitRight.transform;
                    Renderer[] selectionRenderer = selectionR.GetComponentsInChildren<Renderer>();



                    if (selectionRenderer != null)
                    {
                        defaultMaterial = selectionRenderer[0].material.shader;
                        for (int i = 0; i < selectionRenderer.Length; i++)
                        {
                            for (int j = 0; j < selectionRenderer[i].materials.Length; j++)
                            {
                                selectionRenderer[i].materials[j].shader = highlightMaterial;
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
                if (holdingGunRight == false)
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
            if (Physics.SphereCast(transform.position, 0.1f, transform.forward, out hitLeft, 10))

            //if (Physics.SphereCast(camera.transform.position, 0.3f, camera.transform.forward, out hit, 10)) // Non VR
            {
                if (hitLeft.transform.CompareTag("Weapon"))
                {
                    Debug.Log("Weapon Hit");
                    gun = hitLeft.collider.transform.parent.gameObject;

                    var selectionL = hitLeft.transform;
                    Renderer[] selectionRenderer = selectionL.GetComponentsInChildren<Renderer>();



                    if (selectionRenderer != null)
                    {
                        defaultMaterial = selectionRenderer[0].material.shader;
                        for (int i = 0; i < selectionRenderer.Length; i++)
                        {
                            for (int j = 0; j < selectionRenderer[i].materials.Length; j++)
                            {
                                selectionRenderer[i].materials[j].shader = highlightMaterial;
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
                if (holdingGunLeft == false)
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

        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                currentGunRight.GetComponent<Animator>().SetTrigger("Fire");
            }
        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
            {
                currentGunLeft.GetComponent<Animator>().SetTrigger("Fire");
            }
    }
}
