using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    [TagSelector] [SerializeField] private string portalTag;
    [HideInInspector] public List<Portal> portals;
    void Start()
    {
        portals = new List<Portal>();
        
        FindAllPortals();

    }

    private void FindAllPortals() // Used for testing, when procedurally generating portals, instead store the portals as they are generated
    {
        GameObject[] potentialPortals = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < potentialPortals.Length; i++)
        {
            if (potentialPortals[i].CompareTag(portalTag))
            {
                portals.Add(new Portal(potentialPortals[i]));
            }
        }
    }

    public string GetPortalTag()
    {
        return portalTag;
    }
}
