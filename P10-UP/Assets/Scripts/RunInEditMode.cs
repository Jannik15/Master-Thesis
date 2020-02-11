using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RunInEditMode : MonoBehaviour
{
    public Renderer[] objectsToSetMatrixFor;

    [SerializeField] private Transform portal;
    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying)
        {
            for (int i = 0; i < objectsToSetMatrixFor.Length; i++)
            {
                objectsToSetMatrixFor[i].material.SetMatrix("_WorldToPortal", portal.worldToLocalMatrix);
                //for (int j = 0; j < roomMaterials[i].materials.Length; j++)
                //{
                //    Debug.Log("Setting matrix for material of object: " + roomMaterials[i] + " with portal " + portal.name);
                //    roomMaterials[i].materials[j].SetMatrix("_WorldToPortal", portal.worldToLocalMatrix);
                //}
            }
        }
    }
}
