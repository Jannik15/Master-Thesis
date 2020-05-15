using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentDataContainer : MonoBehaviour
{
    public StringDataArray container;
    private bool sceneLoaded;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (!sceneLoaded)
        {
            sceneLoaded = true;
            SceneManager.LoadScene(1);
        }
    }
}
