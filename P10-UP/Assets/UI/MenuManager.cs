using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class MenuManager : MonoBehaviour
{

    public Animator animatorDoor;

    public GameObject optionsMenu;

    public ProceduralLayoutGeneration handler;
    // Start is called before the first frame update
    void Start()
    {
        if (handler == null)
        {
            handler = FindObjectOfType<ProceduralLayoutGeneration>();
        }
    }

    // Update is called once per frame
    void Update()
    {
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

    public void TestStart(){

        //animatorDoor.SetTrigger("Open");
        optionsMenu.SetActive(false);
        handler.ProcedurallyGenerateRooms();
        handler.SwitchCurrentRoom();

    }

    public void TestClose()
    {
        animatorDoor.SetTrigger("Close");
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
