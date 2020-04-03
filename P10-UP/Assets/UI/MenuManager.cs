using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class MenuManager : MonoBehaviour
{

    public Animator animatorDoor;

    public GameObject optionsMenu;

    public GameObject handler;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TestStart(){

        //animatorDoor.SetTrigger("Open");
        optionsMenu.SetActive(false);
        handler.GetComponent<ProceduralLayoutGeneration>().ProcedurallyGenerateRooms();
        handler.GetComponent<ProceduralLayoutGeneration>().SwitchCurrentRoom();

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
