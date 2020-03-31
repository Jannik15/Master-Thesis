using System.Collections;
using System.Collections.Generic;
using Oculus.Platform.Samples.VrHoops;
using UnityEngine;

public class KeyCard : MonoBehaviour
{
    private PlayerInteractions playerInteractions;

    public int keyID = 1;
    // Start is called before the first frame update
    void Start()
    {
        playerInteractions = FindObjectOfType<PlayerInteractions>();
        if (playerInteractions == null)
        {
            Debug.Log("no playerInteractions");
        }

        //keyID = 1; //TEST ONLY
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter()
    {
      
    }

    public void KeyTest(int keyID)
    {
        Debug.Log(keyID);
    }
}
