using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject ball;

    void Update () {

        if (Input.GetKeyDown(KeyCode.O)) {
            Instantiate (ball, transform.position, transform.rotation);

        }
    }
}
