using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponGrab : MonoBehaviour
{
    OVRGrabber oVRGrabber;
    SimpleShoot simpleShoot;
    Rigidbody rb;
    public GameObject gun;
    private bool holdingGun = false;
    private Vector3 gunPosition = new Vector3(0.0481f,0.0184f,-0.01927197f);
    private Vector3 gunRotation = new Vector3(0f, 9.126f, 90f);




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
        simpleShoot = gun.GetComponent<SimpleShoot>();
        rb = gun.GetComponent<Rigidbody>();

        if (holdingGun == false){
            simpleShoot.enabled = false; //off
            oVRGrabber.enabled = true; //on
        }
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.Two)) 
        {
            if(holdingGun == true){
                Drop();
                rb.useGravity = true;
                gun.transform.parent = null;
            }
        }
        if (OVRInput.Get(OVRInput.RawButton.RHandTrigger))
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
