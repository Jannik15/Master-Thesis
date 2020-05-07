using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MenuManager : MonoBehaviour
{
    public Animator animatorDoor;
    public GameObject optionsMenu;
    public ProceduralLayoutGeneration handler;
    public Transform dispensePoint;
    public GameObject gun;
    private GameObject spawnedGun;
    private Room inRoom;
    private List<Transform> spawnedGuns = new List<Transform>();
    public int currentStock = 5;
    public TMP_Text stockAmount;
    public bool dispenser;
    public CharacterController steeringController;
    public OVRPlayerController ovrSteeringController;

    // Start is called before the first frame update
    void Start()
    {
        if (handler == null)
        {
            handler = FindObjectOfType<ProceduralLayoutGeneration>();
        }

        if (dispenser)
        {
            handler.proceduralGenerationFinished += OnProceduralGeneration;
        }
    }

    void OnProceduralGeneration()
    {
        //inRoom = CustomUtilities.FindParentRoom(GetComponentInParent<Grid>().gameObject, handler.rooms);
        //for (int i = 0; i < spawnedGuns.Count; i++)
        //{
        //    inRoom.AddObjectToRoom(spawnedGuns[i], false);
        //    spawnedGuns[i].GetComponentInChildren<InteractableObject>().AssignRoom(inRoom);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (stockAmount != null)
        {
            stockAmount.text = currentStock.ToString();
        }

        if (dispenser)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                Dispense();
            }
        }

        if (OVRInput.Get(OVRInput.RawButton.Start))
        {
            if (!optionsMenu.activeSelf)
            {
                optionsMenu.SetActive(true);
            }
            else if (optionsMenu.activeSelf)
            {
                optionsMenu.SetActive(false);
            }
        }
    }

    public void Dispense()
    {
        if (currentStock > 0)
        {
            float randomRange = Random.Range(40f, 100f);
            spawnedGun = Instantiate(gun, dispensePoint.position, Quaternion.identity);
            spawnedGun.GetComponent<Rigidbody>().AddForce(dispensePoint.forward * randomRange);
            currentStock--;

            if (inRoom != null)
            {
                spawnedGun.GetComponentInChildren<InteractableObject>().AssignRoom(inRoom);
            }
            else
            {
                //spawnedGuns.Add(spawnedGun.transform);
                spawnedGun.transform.parent = handler.rooms[0].gameObject.transform;
            }
        }
    }

    public void TestStart(){
        if (SceneManager.GetActiveScene().name == "Main")
        {
            //animatorDoor.SetTrigger("Open");
            optionsMenu.SetActive(false);
            handler.ProcedurallyGenerateRooms();
            handler.SwitchCurrentRoom();
        } else if (SceneManager.GetActiveScene().name == "EndScreen")
        {
            SceneManager.LoadScene("Main");
        }

    }

    public void TestClose()
    {
        animatorDoor.SetTrigger("Close");
    }

    public void SetSteering(bool b)
    {
        if (steeringController == null)
        {
            Debug.Log("Steering controller was null, attempting to find...");
            steeringController = FindObjectOfType<CharacterController>();
            ovrSteeringController = steeringController.GetComponent<OVRPlayerController>();
        }
        steeringController.enabled = b;
        ovrSteeringController.enabled = b;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#else
        Application.Quit();
#endif
    }
}
