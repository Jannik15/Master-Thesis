using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    [SerializeField] private ShaderContainer container;
    [SerializeField] private List<Material> materials;
    [SerializeField] private List<GameObject> objectsToChangeShader;
    [SerializeField] private int shaderIterator = 0;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ChangeMaterials(objectsToChangeShader, materials, container.shaders[shaderIterator]);
            if (shaderIterator < container.shaders.Length - 1)
            {
                shaderIterator++;
            }
            else
            {
                shaderIterator = 0;
            }
        }
    }

    void ChangeMaterials(List<GameObject> objectsToChange, List<Material> materialsToChange, Shader newShader)
    {
        for (int i = 0; i < objectsToChange.Count / 2; i++)
        {
            objectsToChange[i].GetComponent<Renderer>().material.shader = newShader;
        }
    }
}
