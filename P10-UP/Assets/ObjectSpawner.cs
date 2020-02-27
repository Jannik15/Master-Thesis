using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject ball;
    [SerializeField] private MaterialContainer materials;

    void Update () {


        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Spawning();
        }
        if (OVRInput.Get(OVRInput.Button.Two))
        {
            Spawning();
        }
        if (Input.GetKeyDown(KeyCode.O)) 
        {
            Spawning();
        }
    }

    private void Spawning()
    {
        GameObject newBall = Instantiate(ball, transform.position, transform.rotation);
        if (materials.materials.Length > 0)
        {
            Renderer ballRenderer = newBall.GetComponent<Renderer>();
            ballRenderer.material = materials.materials[Random.Range(0, materials.materials.Length)];
        }
    }
}
