using UnityEngine;

[CreateAssetMenu(fileName = "MaterialContainer", menuName = "Material Container", order = 7)]
[System.Serializable]
public class MaterialContainer : ScriptableObject
{
    public Material[] materials;
}
