using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class WeaponGrab : MonoBehaviour
{
    OVRGrabber oVRGrabber;
    SimpleShoot simpleShoot;
    Rigidbody rb;
    public Shader highlightMaterial;
    private bool holdingGun = false;
    private Vector3 gunPosition = new Vector3(0.0481f,0.0184f,-0.01927197f);
    private Vector3 gunRotation = new Vector3(0f, 9.126f, 90f);
    private Shader defaultMaterial = null;
    
    private Transform _selection;
    [SerializeField]
    private GameObject gun = null;
    



    void PickUp()
    {
        holdingGun = true;
        simpleShoot.enabled = true; //on
        oVRGrabber.enabled = false; //off

    }

    void Drop()
    {
        holdingGun = false;
        simpleShoot.enabled = false; //off
        oVRGrabber.enabled = true; //on


    }


    void Start()
    {
        oVRGrabber = GetComponent<OVRGrabber>();

        if (holdingGun == false){
            oVRGrabber.enabled = true; //on
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
                simpleShoot = gun.GetComponent<SimpleShoot>();
                rb = gun.GetComponent<Rigidbody>();


                var selection = hit.transform;
                Renderer[] selectionRenderer = selection.GetComponentsInChildren<Renderer>();
                Debug.Log(defaultMaterial); 



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



        // Controls
        if (OVRInput.Get(OVRInput.Button.Two) && gun != null || Input.GetKeyDown(KeyCode.H) && gun != null) 
        {
            if(holdingGun == true){
                Drop();
                rb.useGravity = true;
                gun.transform.parent = null;
            }
        }
        if (OVRInput.Get(OVRInput.RawButton.RHandTrigger) && gun != null || Input.GetKeyDown(KeyCode.E) && gun != null)
        {
            if(holdingGun == false){
                PickUp();
                rb.useGravity = false;
                gun.transform.parent = transform;
                gun.transform.localPosition = gunPosition;
                gun.transform.localEulerAngles = gunRotation;
            }
        }
    }
}
