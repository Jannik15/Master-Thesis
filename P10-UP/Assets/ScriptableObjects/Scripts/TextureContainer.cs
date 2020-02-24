using UnityEngine;

[CreateAssetMenu(fileName = "TextureContainer", menuName = "Texture Container", order = 8)]
[System.Serializable]

public class TextureContainer : ScriptableObject
{
    public Texture[] textures;
}
