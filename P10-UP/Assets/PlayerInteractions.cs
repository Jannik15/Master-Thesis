using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInteractions : MonoBehaviour
{
    public List<GameObject> keyUI;
    public LayerMask layerMaskRay;
    public GameObject handler;

    private Ray ray;
    private Camera playerCamera;
    private KeyCard keyCard;
    private int keyHoldID;
    private int maxKeysList;
    private ProceduralLayoutGeneration proLG;
    private bool open,beenOpened,counting,doneWaiting = false;
    private TMP_Text text;
    private float doneCounting;
    private GameObject buttonTemp;



    private bool[] keyArray = new bool[]{false,false,false,false,false,false, false, false, false, false};
    // Start is called before the first frame update
    void Start()
    {
        
        proLG = FindObjectOfType<ProceduralLayoutGeneration>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        if (counting)
        {
            doneCounting += Time.deltaTime;
            if (doneCounting > 5f)
            {
                doneWaiting = true;
                counting = false;
                doneCounting = 0;
            }
        }


        RaycastHit hit;
        ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.F) && Physics.Raycast(ray, out hit, 1.5f, layerMaskRay) && hit.collider.gameObject.tag == "Button")
        {
            //Debug.Log(hit.collider.gameObject.GetComponentInChildren<TextMeshPro>());
            //Debug.Log(hit.collider.gameObject.transform.GetChild(0).GetComponent<TextMeshPro>());
            //text = hit.collider.gameObject.GetComponentInChildren<TMP_Text>();
            if (hit.collider.gameObject.GetComponentInParent<DoorLock>().isLocked == true)
            {
                if (keyArray[hit.collider.gameObject.GetComponentInParent<KeyPad>().KeyPadID] == true)
                {
                    if (!hit.collider.gameObject.GetComponentInParent<KeyPad>().Open)
                    {
                        buttonTemp = hit.collider.gameObject.GetComponentInChildren<TextMeshPro>().gameObject;
                        if (!beenOpened)
                        {
                            hit.collider.gameObject.GetComponentInChildren<TextMeshPro>().text = "Access Granted";
                            hit.collider.gameObject.GetComponentInChildren<TextMeshPro>().enabled = true;
                            beenOpened = true;
                            counting = true;
                        }
                        hit.collider.gameObject.GetComponentInParent<Animator>().CrossFadeInFixedTime("DoorOpen", 0.9f, 0, 0);
                        hit.collider.gameObject.GetComponentInParent<KeyPad>().Open = true;
                    }
                    else if (hit.collider.gameObject.GetComponentInParent<KeyPad>().Open)
                    {

                        hit.collider.gameObject.GetComponentInParent<Animator>().CrossFadeInFixedTime("DoorClose", 0.9f, 0, 0);
                        hit.collider.gameObject.GetComponentInParent<KeyPad>().Open = false;
                    }
                }
                else
                {
                    buttonTemp = hit.collider.gameObject.GetComponentInChildren<TextMeshPro>().gameObject;
                    hit.collider.gameObject.GetComponentInChildren<TextMeshPro>().text = "Access Denied";
                    hit.collider.gameObject.GetComponentInChildren<TextMeshPro>().enabled = true;
                    counting = true;
                    if (doneWaiting)
                    {
                        hit.collider.gameObject.GetComponentInChildren<TextMeshPro>().enabled = false;
                    }
                }
            }
            else
            {
                if (!hit.collider.gameObject.GetComponentInParent<KeyPad>().Open)
                {
                    hit.collider.gameObject.GetComponentInParent<Animator>().CrossFadeInFixedTime("DoorOpen", 0.9f, 0, 0);
                    hit.collider.gameObject.GetComponentInParent<KeyPad>().Open = true;
                }
                else if (hit.collider.gameObject.GetComponentInParent<KeyPad>().Open)
                {
                    hit.collider.gameObject.GetComponentInParent<Animator>().CrossFadeInFixedTime("DoorClose", 0.9f, 0, 0);
                    hit.collider.gameObject.GetComponentInParent<KeyPad>().Open = false;
                }
            }
            
        }
        if (doneWaiting)
        {
            if (buttonTemp.GetComponent<TextMeshPro>() != null)
            {
                buttonTemp.GetComponent<TextMeshPro>().enabled = false;
                doneWaiting = false;
            }
            else 
                doneWaiting = false;
        }
    }



    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Keycard"))
        {
            maxKeysList = proLG.keysList.Count;
            keyHoldID = collider.gameObject.GetComponent<KeyCard>().keyID;

            for (int i = 0; i < maxKeysList; i++)
            {
                if (i == keyHoldID)
                {
                    keyArray[i] = true;
                    keyUI[i].gameObject.SetActive(true);
                    break;
                }

            }
            proLG.currentRoom.RemoveObjectFromRoom(collider.transform);
            Destroy(collider.gameObject);

        }
    }

    void KeyCard()
    {

    }
}
