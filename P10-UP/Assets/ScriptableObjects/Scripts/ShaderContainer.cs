using UnityEngine;

[CreateAssetMenu(fileName = "ShaderContainer", menuName = "Shader Container", order = 10)]
[System.Serializable]
public class ShaderContainer : ScriptableObject
{
    public Shader[] shaders;
}
