namespace ThisProject.Utils
{
    using System.Collections.Generic;
    using UnityEngine;
    /// <summary>
    /// Shader utility class.
    /// </summary>
    public class ShaderUtils
    {
        public List<Portal> InitializePortals(string portalTag) // TODO: Add method summary
        {
            List<Portal> portals = new List<Portal>();
            GameObject[] allPortals = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)); // TODO: Pass Portal objects after generation
            for (int i = 0; i < allPortals.Length; i++)
            {
                if (allPortals[i].CompareTag(portalTag))
                {
                    portals.Add(new Portal(allPortals[i]));
                }
            }
            return portals;
        }

        public void UpdateShaderMatrix(List<Renderer> roomMaterials, Transform portal) // TODO: Add method summary
        {
            for (int i = 0; i < roomMaterials.Count; i++)
            {
                roomMaterials[i].material.SetMatrix("_WorldToPortal", portal.worldToLocalMatrix);
            }
        }
    }
}
