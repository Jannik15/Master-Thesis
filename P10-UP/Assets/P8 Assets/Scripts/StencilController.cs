using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StencilController : MonoBehaviour
{
    public ProceduralLayoutGeneration layout;   // Assign gameobject with this script in the inspector
    TextMeshProUGUI startText, endText;
    Object[] resourceMaterials;
    void Start()
    {
        //resourceMaterials = Resources.LoadAll("Stencil-Materials/", typeof(Material));
        //foreach (Material mat in resourceMaterials)
        //{
        //    new Material(mat.shader);
        //}
        SetStencilShader(0);
    }

    public void SetStencilShader(int currentRoom)
    {
        /// Current room set to stencil ref 0
        foreach (Renderer r in layout.layoutList[currentRoom].GetComponentsInChildren<Renderer>())
        {
            /// Setting the shader of the material (copy of an existing)
            //r.material.shader = Shader.Find("Stencils/Materials/StencilBufferCurrent");
            /// Setting the shader of the current material across the project, meaning for all objects using that material (also prefabs)
            //r.sharedMaterial.shader = Resources.Load("Standard", typeof(Shader)) as Shader;
            /// Setting the material for all objects with that material.
            //r.sharedMaterial = Resources.Load("Materials/DefaultStencilCurrent", typeof(Material)) as Material;
            if (r.tag != "EntryPortal" && r.tag != "ExitPortal" && r.tag != "Stencil" && r.tag != "OppositeStencil")
            {
                string materialName = r.material.name.Substring(0, r.material.name.IndexOf('_'));
                r.material = Resources.Load("Stencil-Materials/" + materialName + "_Current", typeof(Material)) as Material;
            }
        }

        /// Next room set to stencil ref 2
        if (currentRoom < layout.layoutList.Count - 1)
        {
            foreach (Renderer r in layout.layoutList[currentRoom + 1].GetComponentsInChildren<Renderer>())
            {
                if (r.tag != "EntryPortal" && r.tag != "ExitPortal" && r.tag != "Stencil" && r.tag != "OppositeStencil")
                {
                    //Debug.Log("Material name: " + r.material.name + ", capped at _");
                    string materialName = r.material.name.Substring(0, r.material.name.IndexOf('_'));
                    r.material = Resources.Load("Stencil-Materials/" + materialName + "_Next", typeof(Material)) as Material;
                }
            }
        }

        /// Previous room set to stencil ref 1
        if (currentRoom > 0)
        {
            foreach (Renderer r in layout.layoutList[currentRoom - 1].GetComponentsInChildren<Renderer>())
            {
                if (r.tag != "EntryPortal" && r.tag != "ExitPortal" && r.tag != "Stencil" && r.tag != "OppositeStencil")
                {
                    string materialName = r.material.name.Substring(0, r.material.name.IndexOf('_'));
                    r.material = Resources.Load("Stencil-Materials/" + materialName + "_Previous", typeof(Material)) as Material;
                }
            }
        }

        /// Same, but for text materials
        if (currentRoom <= 1)
        {
            if (startText == null)
                startText = GameObject.FindGameObjectWithTag("StartText").GetComponent<TextMeshProUGUI>();

            string materialName = startText.fontMaterial.name.Substring(0, startText.fontMaterial.name.IndexOf('_'));
            if (currentRoom == 0) // Assign current to start text
                startText.fontMaterial = Resources.Load("Stencil-Materials/" + materialName + "_Current", typeof(Material)) as Material;
            else if (currentRoom == 1) // Assign previous to start text
                startText.fontMaterial = Resources.Load("Stencil-Materials/" + materialName + "_Previous", typeof(Material)) as Material;
        }
        if (currentRoom >= layout.maxRooms - 2)
        {
            if (endText == null)
                endText = GameObject.FindGameObjectWithTag("EndText").GetComponent<TextMeshProUGUI>();

            string materialName = endText.fontMaterial.name.Substring(0, endText.fontMaterial.name.IndexOf('_'));
            if (currentRoom == layout.maxRooms - 1)
                endText.fontMaterial = Resources.Load("Stencil-Materials/" + materialName + "_Current", typeof(Material)) as Material;
            else if (currentRoom == layout.maxRooms - 2) // Assign next to end text
                endText.fontMaterial = Resources.Load("Stencil-Materials/" + materialName + "_Next", typeof(Material)) as Material;
        }   

    }
}
