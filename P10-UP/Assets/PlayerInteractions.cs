using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    public GameObject[] keyUI = new GameObject[2];

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

        if (Input.GetKeyDown(KeyCode.F) && Physics.Raycast(ray, out hit, 0.5f) && hit.collider.gameObject.tag == "Button")
        {
            if (keyArray[hit.collider.gameObject.GetComponentInParent<KeyPad>().KeyPadID] == true)
            {
                if (!open)
                {
                    hit.collider.gameObject.GetComponentInParent<Animator>().SetTrigger("Open");
                    open = true;
                }
                else if (open)
                {
                    hit.collider.gameObject.GetComponentInParent<Animator>().SetTrigger("Close");
                    open = false;
                }
            }
            else
            {
                Debug.Log("Door is Locked, find the Key");
            }

            
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
