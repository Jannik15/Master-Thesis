using UnityEngine;
using System.Collections;

public class ThrowObject : MonoBehaviour
{
    public Transform player;
    public Transform playerCam;
    public Transform holdPoint;
    public float throwForce = 10;
    bool hasPlayer = false;
    bool beingCarried = false;
    private bool touched = false;
    private float suckSpeed = 5f;
    private bool suck = true;

    void Start()
    {
        player = FindObjectOfType<CharacterController>().GetComponent<Transform>();
        playerCam = player.GetChild(0).GetComponent<Transform>();
        holdPoint = player.GetChild(1).GetComponent<Transform>();
    }

    
    void Update()
    {
        float dist = Vector3.Distance(gameObject.transform.position, player.position);
        if (dist <= 5.5f)
        {
            hasPlayer = true;
        }
        else
        {
            hasPlayer = false;
        }
        if (hasPlayer && Input.GetKeyDown(KeyCode.E))
        {
            GetComponent<Rigidbody>().useGravity = false;
            transform.parent = playerCam;

            beingCarried = true;
            suck = true;
        }
        if (beingCarried)
        {   
            
            if(Vector3.Distance(transform.position, holdPoint.position) >= 0.05f && suck){
                transform.position = Vector3.Lerp(transform.position, holdPoint.position, Time.deltaTime * suckSpeed);
                Debug.Log("Lerping");            
            } 
            else
                suck = false;

            Debug.Log("Im being held");
            if (touched)
            {
                GetComponent<Rigidbody>().isKinematic = false;
                transform.parent = null;
                beingCarried = false;
                touched = false;
            }
            if (Input.GetMouseButtonDown(0))
                {
                    GetComponent<Rigidbody>().useGravity = true;
                    transform.parent = null;
                    beingCarried = false;
                    GetComponent<Rigidbody>().AddForce(playerCam.forward * throwForce);

                }
                else if (Input.GetMouseButtonDown(1))
                {
                GetComponent<Rigidbody>().isKinematic = false;
                    transform.parent = null;
                beingCarried = false;
                }
            }
        }
}