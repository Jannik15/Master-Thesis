using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class InstanceMaterialOnStart : MonoBehaviour
{
    private Material[] materials, storedMaterials;
    void Start()
    {
        storedMaterials = GetComponent<Renderer>().materials;
        materials = GetComponent<Renderer>().materials;
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = Material.Instantiate(materials[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
