using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    public GameObject[] keyUI = new GameObject[2];
    public LayerMask layerMaskRay;


    private Ray ray;
    private Camera playerCamera;
    private KeyCard keyCard;
    private int keyHoldID;

    private bool open = false;

    private bool[] keyArray = new bool[]{false,false};
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(keyArray[0]);
        Debug.Log(keyArray[1]);
        playerCamera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetKeyDown(KeyCode.F) && Physics.Raycast(ray, out hit, 1.5f, layerMaskRay))
        {
            Debug.Log(hit.collider);
        }

        if (Input.GetKeyDown(KeyCode.F) && Physics.Raycast(ray, out hit, 1.5f, layerMaskRay) && hit.collider.gameObject.tag == "Button")
        {
            if (hit.collider.gameObject.GetComponentInParent<DoorLock>().isLocked == true)
            {
                if (keyArray[hit.collider.gameObject.GetComponentInParent<KeyPad>().KeyPadID] == true)
                {
                    if (!hit.collider.gameObject.GetComponentInParent<KeyPad>().Open)
                    {
                        hit.collider.gameObject.GetComponentInParent<Animator>().SetTrigger("Open");
                        hit.collider.gameObject.GetComponentInParent<KeyPad>().Open = true;
                    }
                    else if (hit.collider.gameObject.GetComponentInParent<KeyPad>().Open)
                    {
                        hit.collider.gameObject.GetComponentInParent<Animator>().SetTrigger("Close");
                        hit.collider.gameObject.GetComponentInParent<KeyPad>().Open = false;
                    }
                }
                else
                {
                    Debug.Log("Door is Locked, find the Key");
                }
            }
            else
            {
                if (!hit.collider.gameObject.GetComponentInParent<KeyPad>().Open)
                {
                    hit.collider.gameObject.GetComponentInParent<Animator>().SetTrigger("Open");
                    hit.collider.gameObject.GetComponentInParent<KeyPad>().Open = true;
                }
                else if (hit.collider.gameObject.GetComponentInParent<KeyPad>().Open)
                {
                    hit.collider.gameObject.GetComponentInParent<Animator>().SetTrigger("Close");
                    hit.collider.gameObject.GetComponentInParent<KeyPad>().Open = false;
                }
            }
            
            Debug.Log("Open is now = " + open + "for " + gameObject.name);
            
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        //collider.CompareTag()
        if (collider.gameObject.tag == "Keycard")
        {
            keyHoldID = collider.gameObject.GetComponent<KeyCard>().keyID;

            for (int i = 0; i < keyArray.Length; i++)
            {
                if (i == keyHoldID)
                {
                    keyArray[i] = true;
                    keyUI[i].gameObject.SetActive(true);
                    break;
                }

            }

            Debug.Log("Picked up Keycard #" + keyHoldID);
            Destroy(collider.gameObject);
        }
    }

    void KeyCard()
    {

    }
}
