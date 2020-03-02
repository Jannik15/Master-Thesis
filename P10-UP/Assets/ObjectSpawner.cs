using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject ball;
    public float shootForce = 1.0f;
    [SerializeField] private MaterialContainer materials;

   void Start(){


   }        

    void Update () {


        if (OVRInput.GetDown(OVRInput.Button.One))
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

    private void Shooting()
    {
        GameObject newBall = Instantiate(ball, transform.position, transform.rotation);
        Rigidbody rb = newBall.GetComponent<Rigidbody>();
    
        if (materials.materials.Length > 0)
        {
            Renderer ballRenderer = newBall.GetComponent<Renderer>();
            ballRenderer.material = materials.materials[Random.Range(0, materials.materials.Length)];
            if (rb != null)
                rb.AddForce(transform.forward * shootForce);            
        }
    }


}

