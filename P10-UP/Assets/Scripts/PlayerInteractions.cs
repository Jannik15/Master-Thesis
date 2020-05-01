using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerInteractions : MonoBehaviour
{
    public List<GameObject> keyUI;
    public LayerMask layerMaskRay;
    public float health = 100;
    public Animator fadeAnimator;
 

    private Ray ray;
    private Camera playerCamera;
    private int keyHoldID;
    private int maxKeysList;
    private ProceduralLayoutGeneration proLG;


    private UIWatch uiWatch;
    // Start is called before the first frame update
    void Start()
    {
        uiWatch = GetComponentInChildren<UIWatch>();
        proLG = FindObjectOfType<ProceduralLayoutGeneration>();
        playerCamera = GetComponentInChildren<Camera>();
    }
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetKeyDown(KeyCode.F) && Physics.Raycast(ray, out hit, 1.5f, layerMaskRay) && hit.collider.gameObject.tag == "Button")
        {
            DoorAction(hit.collider);
        }
    }

    public void TakeDamage(float amount)
    {
        uiWatch.healthUpdate((int)amount);
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        fadeAnimator.SetTrigger("FadeToBlack");
        //Fade to black and restart
    }

    public void OnFadeComplete()
    {
        SceneManager.LoadScene("Main");
        fadeAnimator.SetTrigger("FadeIn");
    }

    public void DoorAction(Collider other)
    {
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
    }

    private void OnCollisionEnter(Collision collission)
    {
        Debug.Log("I've been hit by " + collission.gameObject);
        if (collission.gameObject.CompareTag("HostileProjectile"))
        {

            TakeDamage(10f);
        }
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
