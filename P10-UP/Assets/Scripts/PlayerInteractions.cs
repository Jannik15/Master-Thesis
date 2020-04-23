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
    private int keyHoldID;
    private int maxKeysList;
    private ProceduralLayoutGeneration proLG;
    // Start is called before the first frame update
    void Start()
    {
        
        proLG = FindObjectOfType<ProceduralLayoutGeneration>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        #region Old
        /*/ 
        if (Input.GetKeyDown(KeyCode.F) && Physics.Raycast(ray, out hit, 1.5f, layerMaskRay) && hit.collider.gameObject.tag == "Button")
        {
            if (hit.collider.gameObject.GetComponentInParent<DoorLock>().isLocked == true)
            {
                if (keyArray[hit.collider.gameObject.GetComponentInParent<KeyPad>().KeyPadID] == true)
                {
                    if (!hit.collider.gameObject.GetComponentInParent<KeyPad>().Open)
                    {
                        if (!hit.collider.gameObject.GetComponentInParent<DoorLock>().beenUnlocked)
                        {
                            //Access Granted Sound
                            hit.collider.gameObject.GetComponentInParent<DoorLock>().beenUnlocked = true;
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
                    //Access Denied Sound
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
        //*/
        #endregion

        #region New
        //*/ 
        if (Input.GetKeyDown(KeyCode.F) && Physics.Raycast(ray, out hit, 1.5f, layerMaskRay) && hit.collider.gameObject.tag == "Button")
        {
            DoorAction(hit.collider);
        }
        //*/
        #endregion
    }

    public void DoorAction(Collider other)
    {
        #region Old
        /*/
        if (other.gameObject.GetComponentInParent<DoorLock>().isLocked == true)
        {
            if (keyArray[other.transform.parent.GetComponentInChildren<KeyPad>().KeyPadID] == true)
            {
                if (!other.transform.parent.GetComponentInChildren<KeyPad>().Open)
                {
                    if (!other.gameObject.GetComponentInParent<DoorLock>().beenUnlocked)
                    {
                        //Access Granted Sound
                        other.gameObject.GetComponentInParent<DoorLock>().beenUnlocked = true;
                    }
                    other.gameObject.GetComponentInParent<Animator>().CrossFadeInFixedTime("DoorOpen", 0.9f, 0, 0);
                    other.transform.parent.GetComponentInChildren<KeyPad>().Open = true;
                }
                else if (other.transform.parent.GetComponentInChildren<KeyPad>().Open)
                {

                    other.gameObject.GetComponentInParent<Animator>().CrossFadeInFixedTime("DoorClose", 0.9f, 0, 0);
                    other.transform.parent.GetComponentInChildren<KeyPad>().Open = false;
                }
            }
            else
            {
                //Access Denied Sound
            }
        }
        else
        {
            if (!other.transform.parent.GetComponentInChildren<KeyPad>().Open)
            {
                other.gameObject.GetComponentInParent<Animator>().CrossFadeInFixedTime("DoorOpen", 0.9f, 0, 0);
                other.transform.parent.GetComponentInChildren<KeyPad>().Open = true;
            }
            else if (other.gameObject.GetComponentInParent<KeyPad>().Open)
            {
                other.gameObject.GetComponentInParent<Animator>().CrossFadeInFixedTime("DoorClose", 0.9f, 0, 0);
                other.transform.parent.GetComponentInChildren<KeyPad>().Open = false;
            }
        }
        //*/
        #endregion

        #region New
        //*/
        DoorLock thisLock = other.gameObject.GetComponentInParent<DoorLock>();
        if (thisLock.isLocked)
        {
            thisLock.TryToUnlock();
            if (thisLock.isLocked)
                return;
        }
        if (thisLock.isOpen)
        {
            thisLock.CloseDoor();
        }
        else
        {
            thisLock.OpenDoor();
        }
        //*/
        #endregion
    }


    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Keycard"))
        {
            // Loop through the list of keyCards to enable the correct one in the UI.
            for (int i = 0; i < proLG.keysList.Count; i++)
            {
                if (proLG.keysList[i] == collider.transform.parent.gameObject)
                {
                    if (keyUI.Count > i)
                    {
                        keyUI[i].SetActive(true);
                    }
                    else
                    {
                        Debug.Log("Picked up keyCard #" + (i+1) + " but this number is not present in the UI and cannot be enabled!");
                    }
                }
            }
            // Be aware that this destroys THE PARENT of the collider (supposed to destroy keyCardHolder)
            proLG.currentRoom.RemoveObjectFromRoom(collider.transform.parent);
            Destroy(collider.transform.parent.gameObject);
        }
    }
}
