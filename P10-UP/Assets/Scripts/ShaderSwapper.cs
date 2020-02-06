namespace RendererExtensions
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ShaderSwapper
    {
        // TODO: Expand for different types of shaders (currently only works for swapping stencil materials and masks)
        /// <summary>
        /// Basic shader swapping method for list of GameObjects. Currently doesn't factor
        /// different kinds of shaders, but assumes all shaders work for all materials.
        /// </summary>
        /// <param name="objectsToSwap"></param>
        /// <param name="shaderToSwapTo"></param>
        public void SwapShader(List<GameObject> objectsToSwap, Shader materialShader, Shader maskShader)
        {
            for (int i = 0; i < objectsToSwap.Count; i++)
            {
                Material[] objectMaterials = objectsToSwap[i].GetComponent<Renderer>().materials;
                if (objectsToSwap[i].tag == "Mask")
                {
                    for (int j = 0; j < objectMaterials.Length; j++)
                    {
                        objectMaterials[j].shader = maskShader;
                    }
                }
                else
                {
                    for (int j = 0; j < objectMaterials.Length; j++)
                    {
                        objectMaterials[j].shader = materialShader;
                    }
                }
            }
        }
        // Overload for arrays // TODO: UPDATE
        public void SwapShader(GameObject[] objectsToSwap, Shader shaderToSwapTo)
        {
            for (int i = 0; i < objectsToSwap.Length; i++)
            {
                Material[] objectMaterials = objectsToSwap[i].GetComponent<Renderer>().materials;
                for (int j = 0; j < objectMaterials.Length; j++)
                {
                    objectMaterials[j].shader = shaderToSwapTo;
                }
            }
        }
        // Overload for multidimensional lists (lists containing arrays)
        public void SwapShader(List<Renderer> objectsToSwap, Shader materialShader)
        {
            for (int i = 0; i < objectsToSwap.Count; i++)
            {
                Material[] objectMaterials = objectsToSwap[i].materials;
                for (int j = 0; j < objectMaterials.Length; j++)
                {
                    objectsToSwap[i].materials[i].shader = materialShader;
                }
            }
        }

    }

}