using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class WeaponGrab : MonoBehaviour
{
    OVRGrabber oVRGrabber;
    SimpleShoot simpleShoot;
    Rigidbody rb;
    public Shader highlightMaterial;
    private bool holdingGunRight = false;
    private bool holdingGunLeft = false;
    private Vector3 gunPositionRight = new Vector3(0.0481f,0.0184f,-0.01927197f);
    private Vector3 gunRotationRight = new Vector3(0f, 9.126f, 90f);
    private Vector3 gunPositionLeft = new Vector3(-0.03830f,0.03422651f,-0.03557706f);
    private Vector3 gunRotationLeft = new Vector3(0f, -9.126f, -90f);
    private Shader defaultMaterial = null;
    
    private Transform _selection;
    [SerializeField]
    private GameObject gun = null;
    [SerializeField]
    private GameObject currentGunRight = null;
    [SerializeField]
    private GameObject currentGunLeft = null;
    



    void PickUpRight()
    {
        holdingGunRight = true;
        simpleShoot.enabled = true; //on
        oVRGrabber.enabled = false; //off

    }

    void DropRight()
    {
        holdingGunRight = false;
        simpleShoot.enabled = false; //off
        oVRGrabber.enabled = true; //on
    }

    void PickUpLeft()
    {
        holdingGunLeft = true;
        simpleShoot.enabled = true; //on
        oVRGrabber.enabled = false; //off

    }

    void DropLeft()
    {
        holdingGunLeft = false;
        simpleShoot.enabled = false; //off
        oVRGrabber.enabled = true; //on
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

        if(_selection != null){
            Renderer[] selectionRenderer = _selection.GetComponentsInChildren<Renderer>();
            
            for (int i = 0; i < selectionRenderer.Length; i++)
            {
                for (int j = 0; j < selectionRenderer[i].materials.Length; j++)
                {
                    selectionRenderer[i].materials[j].shader = defaultMaterial;
                }
            }
            gun = null;
            _selection = null;
        }

       //var ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))   //Vr (Not tested)
        //if(Physics.Raycast(ray, out hit))
        {
            if(hit.transform.CompareTag("Weapon"))
            {
                gun = hit.collider.transform.parent.gameObject;

                var selection = hit.transform;
                Renderer[] selectionRenderer = selection.GetComponentsInChildren<Renderer>();



                if(selectionRenderer != null)
                {   
                    defaultMaterial = selectionRenderer[0].material.shader;
                    for (int i = 0; i < selectionRenderer.Length; i++)
                    {
                        for (int j = 0; j < selectionRenderer[i].materials.Length; j++)
                        {
                            selectionRenderer[i].materials[j].shader = highlightMaterial;
                        }
                    }                
                }
                _selection = selection;
            }

        }

        //Right Hand Drop
        if (OVRInput.GetDown(OVRInput.Button.Two) && currentGunRight != null || Input.GetKeyDown(KeyCode.H) && gun != null) 
        {
            if(holdingGunRight == true){
                DropRight();
                rb.isKinematic = false;
                currentGunRight.transform.parent = null;
                gun = null;
                currentGunRight = null;
            }
        } //Right Hand pickup
        if (OVRInput.Get(OVRInput.RawButton.RHandTrigger) && gun != null || Input.GetKeyDown(KeyCode.E) && gun != null)
        {
            if(holdingGunRight == false)
            {
                if(hit.transform.CompareTag("Weapon"))
                {
                    currentGunRight = hit.collider.transform.parent.gameObject;
                    currentGunRight.transform.parent = transform;
                    currentGunRight.transform.localPosition = gunPositionRight;
                    currentGunRight.transform.localEulerAngles = gunRotationRight;
                    simpleShoot = currentGunRight.GetComponent<SimpleShoot>();
                    rb = currentGunRight.GetComponent<Rigidbody>();
                    rb.isKinematic = true;
                    PickUpRight();
                }
            }
        }
        //Left Hand Drop
        if (OVRInput.GetDown(OVRInput.Button.Four) && currentGunLeft != null || Input.GetKeyDown(KeyCode.J) && gun != null) 
        {
            if(holdingGunLeft == true){
                DropLeft();
                rb.isKinematic = false;
                currentGunLeft.transform.parent = null;
                gun = null;
                currentGunLeft = null;
            }
        }
        
         //Left hand Pickup
        if (OVRInput.Get(OVRInput.RawButton.LHandTrigger) && gun != null || Input.GetKeyDown(KeyCode.R) && gun != null)
        {
            if(holdingGunLeft == false)
            {
                if(hit.transform.CompareTag("Weapon"))
                {
                    currentGunLeft = hit.collider.transform.parent.gameObject;
                    currentGunLeft.transform.parent = transform;
                    currentGunLeft.transform.localPosition = gunPositionLeft;
                    currentGunLeft.transform.localEulerAngles = gunRotationLeft;
                    simpleShoot = currentGunLeft.GetComponent<SimpleShoot>();
                    rb = currentGunLeft.GetComponent<Rigidbody>();
                    rb.isKinematic = true;
                    PickUpLeft();
                }
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
