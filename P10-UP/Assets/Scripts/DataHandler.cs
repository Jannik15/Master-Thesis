// Updated script - Google Forms data handler - Based on YT tutorial: https://www.youtube.com/watch?v=z9b5aRfrz7M

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class DataHandler : MonoBehaviour
{
    public bool OnlyOneLogPerDevice = false;
    public string baseURL = ""; // fill out this and entry IDs in inspector
    public string[] entryIds;
    public int[] sliderIndeces;
    public StringDataArray internalData;
    [HideInInspector] public int indexToModify;
    [HideInInspector] public bool toggleState;
    private string spec = "G";
    private CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");

    private void Start()
    {
        if (internalData.s.Length != entryIds.Length)
        {
            internalData.s = new string[entryIds.Length];
        }
    }

    // Unity UI events only allow 1 argument in the inspector, so adding helper methods for defining data index and toggle states
    public void ChangeDataIndex(int newIndex)
    {
        indexToModify = newIndex;
    }

    public void ChangeToggleState(bool newToggleState)
    {
        toggleState = newToggleState;
    }

    public void AssignData(string data)
    {
        internalData.s[indexToModify] = data;
    }

    public void AssignSliderData(float data)
    {
        internalData.s[indexToModify] = data.ToString(spec, ci);
    }

    public void AssignMultipleChoiceData(string data)
    {
        if (toggleState && (string.IsNullOrEmpty(internalData.s[indexToModify]) || !internalData.s[indexToModify].Contains(data)))
        {
            internalData.s[indexToModify] += data + ", ";
        }
        else if (!toggleState && internalData.s[indexToModify].Contains(data))
        {
            internalData.s[indexToModify] = internalData.s[indexToModify].Remove(internalData.s[indexToModify].IndexOf(data), data.Length + 2); // + 2 to include comma and space
        }
    }

    public void SendInternalData()
    {
        Debug.Log("Sending internal data:");
        for (int i = 0; i < internalData.s.Length; i++)
        {
            if (string.IsNullOrEmpty(internalData.s[i]))
            {
                if (i < internalData.s.Length - 16)
                {
                    internalData.s[i] = "None";
                }
                else if (sliderIndeces.Contains(i))
                {
                    internalData.s[i] = "3";    // When participants do not change the slider value, assign default value
                }
                else if (i < internalData.s.Length - 1)// Multiple choice data
                {
                    internalData.s[i] = "No selection";
                }
                else // Last question (Movement type from player prefs)
                {
                    if (PlayerPrefs.GetInt("MovementType") == 1)
                    {
                        internalData.s[i] = "Natural_walking";
                    }
                    else
                    {
                        internalData.s[i] = "Steering";
                    }
                }
            }
            else if (internalData.s[i].EndsWith(","))
            {
                internalData.s[i] = internalData.s[i].Remove(internalData.s[i].Length - 1);
            }
            Debug.Log("      Question " + (i + 1) + ": " + internalData.s[i]);
        }
        StartCoroutine(Post(internalData.s.ToList()));
    }

    public void SendData(List<float> data) // Call if sending float data only. Otherwise sending a string list is preferred.
    {
        List<string> tempConvertedData = new List<string>();

        // Culture specification to get . instead of , when converting to strings:

        foreach (float floatData in data)
        {
            tempConvertedData.Add(floatData.ToString(spec, ci));
        }

        StartCoroutine(Post(tempConvertedData));
    }

    public void SendData(List<string> data) // Preferred to use this function, and do the float conversion as seen above elsewhere if needed.
    {
        StartCoroutine(Post(data));
    }

    IEnumerator Post(List<string> finalData)
    {
        bool sendData = true;

        if (entryIds == null || finalData == null)
        {
            Debug.LogError("Result POST error: entry ID array or received data array is null!");
            sendData = false;
        }
        else if (finalData.Count != entryIds.Length)
        {
            Debug.LogError("Result POST error: data list received is not the same length as entry ID array. Make sure they have the same length.");
            sendData = false;
        }

        if (OnlyOneLogPerDevice)
        {
            if (PlayerPrefs.GetInt("dataSubmitted") == 1)
            {
                Debug.Log("Data already submitted by this user - post request is ignored");
                sendData = false;
            }
        }

        if (sendData)
        {
            WWWForm form = new WWWForm();

            for (int i = 0; i < finalData.Count; i++)
            {
                if (entryIds.Length > i)
                    form.AddField(entryIds[i], finalData[i]);
            }

            byte[] rawData = form.data;

            UnityWebRequest webRequest = new UnityWebRequest(baseURL, UnityWebRequest.kHttpVerbPOST);
            UploadHandlerRaw uploadHandler = new UploadHandlerRaw(rawData);
            uploadHandler.contentType = "application/x-www-form-urlencoded";
            webRequest.uploadHandler = uploadHandler;
            webRequest.SendWebRequest();

            if (OnlyOneLogPerDevice)
                PlayerPrefs.SetInt("dataSubmitted", 1);

            yield return webRequest;
        }
        else
            yield return null;

    }
}

#region CustomInspector
#if UNITY_EDITOR
[CustomEditor(typeof(DataHandler))]
public class DataHandler_Editor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        var script = target as DataHandler;

        EditorGUILayout.HelpBox("The order and the amount of entry IDs must be the same as on your Google Form", MessageType.Info);
        EditorGUILayout.HelpBox("This means the data sent to this data handler script must be in the same order to send the data to the correct entry IDs on Google Forms.", MessageType.None);

        DrawDefaultInspector();

        if (!script.OnlyOneLogPerDevice)
            if (PlayerPrefs.GetInt("dataSubmitted") == 1)
                PlayerPrefs.SetInt("dataSubmitted", 0);
    }
}
#endif
#endregion