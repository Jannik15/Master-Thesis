using UnityEngine;

[CreateAssetMenu(fileName = "StringData", menuName = "String Data Array", order = 10)]
[System.Serializable]
public class StringDataArray : ScriptableObject
{
    public string[] s;

    public void AddMissingElementsToArray(string[] newString)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (string.IsNullOrEmpty(s[i]) && !string.IsNullOrEmpty(newString[i]))
            {
                s[i] = newString[i];
            }
        }
    }
}
