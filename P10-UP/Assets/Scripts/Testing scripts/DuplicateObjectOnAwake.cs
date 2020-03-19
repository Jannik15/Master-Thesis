using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicateObjectOnAwake : MonoBehaviour
{
    [SerializeField] private GameObject objectToDuplicate;
    [SerializeField] private int amountToDuplicate;
    [SerializeField] private int renderQueueIncrement;
    private void Awake()
    {
        for (int i = 2; i < amountToDuplicate + 2; i++)
        {
            GameObject duplicatedObject = Instantiate(objectToDuplicate, objectToDuplicate.transform.position, objectToDuplicate.transform.rotation, objectToDuplicate.transform.parent);
            duplicatedObject.name = objectToDuplicate.name.Split('_')[0] + "_" + i;
            Renderer[] renderers = duplicatedObject.GetComponentsInChildren<Renderer>();
            for (int j = 0; j < renderers.Length; j++)
            {
                for (int k = 0; k < renderers[j].materials.Length; k++)
                {
                    renderers[j].materials[k].SetInt("_StencilValue", i);
                    renderers[j].materials[k].renderQueue += renderQueueIncrement;
                }
            }
        }
    }
}
