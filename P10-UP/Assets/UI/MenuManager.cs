using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    public Animator animatorDoor;

    public GameObject optionsMenu;
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
        
    }

    public void TestClose()
    {
        animatorDoor.SetTrigger("Close");
    }

    public void QuitGame()
    {
        Debug.Log("If this was a build, it would close application");
        Application.Quit();
    }
}
