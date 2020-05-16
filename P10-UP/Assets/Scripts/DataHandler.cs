// Updated script - Google Forms data handler - Based on YT tutorial: https://www.youtube.com/watch?v=z9b5aRfrz7M

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;
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
    private PersistentDataContainer persistentData;
    [HideInInspector] public int indexToModify;
    [HideInInspector] public bool toggleState;
    private string spec = "G";
    private CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");

    private void Start()
    {
        persistentData = FindObjectOfType<PersistentDataContainer>();
        if (persistentData.container.s.Length != entryIds.Length)
        {
            persistentData.container.s = new string[entryIds.Length];
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
        persistentData.container.s[indexToModify] = data;
    }

    public void AssignSliderData(float data)
    {
        persistentData.container.s[indexToModify] = data.ToString(spec, ci);
    }

    public void AssignMultipleChoiceData(string data)
    {
        if (toggleState && (string.IsNullOrEmpty(persistentData.container.s[indexToModify]) ||
                            !persistentData.container.s[indexToModify].Contains(data)))
        {
            persistentData.container.s[indexToModify] += data + ", ";
        }
        else if (!toggleState && persistentData.container.s[indexToModify].Contains(data))
        {
            persistentData.container.s[indexToModify] = persistentData.container.s[indexToModify]
                .Remove(persistentData.container.s[indexToModify].IndexOf(data), data.Length + 2); // + 2 to include comma and space
        }
    }

    public void SendInternalData()
    {
        // Hard-coding some questions that depend on each other
        if (!string.IsNullOrEmpty(persistentData.container.s[0]) && persistentData.container.s[0] == "None") // Steering general discomfort
        {
            for (int j = 1; j < 8; j++)
            {
                persistentData.container.s[j] = "None";
            }
        }
        else if (!string.IsNullOrEmpty(persistentData.container.s[4]) && persistentData.container.s[4] == "None") // Steering Dizzyness 
        {
            persistentData.container.s[5] = "None"; // Setting steering Vertigo to "None"
        }
        if (!string.IsNullOrEmpty(persistentData.container.s[8]) && persistentData.container.s[8] == "No") // Steering problems with vision
        {
            for (int j = 9; j < 12; j++)
            {
                persistentData.container.s[j] = "None";
            }
        }

        if (!string.IsNullOrEmpty(persistentData.container.s[16]) && persistentData.container.s[16] == "None") // Walking general discomfort
        {
            for (int j = 17; j < 24; j++)
            {
                persistentData.container.s[j] = "None";
            }
        }
        else if (!string.IsNullOrEmpty(persistentData.container.s[20]) && persistentData.container.s[20] == "None") // Walking Dizzyness 
        {
            persistentData.container.s[21] = "None"; // Setting walking Vertigo to "None"
        }
        if (!string.IsNullOrEmpty(persistentData.container.s[24]) && persistentData.container.s[24] == "No") // Walking problems with vision
        {
            for (int j = 25; j < 28; j++)
            {
                persistentData.container.s[j] = "None";
            }
        }

        for (int i = 0; i < persistentData.container.s.Length; i++)
        {
            if (string.IsNullOrEmpty(persistentData.container.s[i]))
            {
                if (i < persistentData.container.s.Length - 16)
                {
                    persistentData.container.s[i] = "None";
                }
                else if (sliderIndeces.Contains(i))
                {
                    persistentData.container.s[i] = "3"; // When participants do not change the slider value, assign default value
                }
                else if (i < persistentData.container.s.Length - 1) // Multiple choice data
                {
                    persistentData.container.s[i] = "No selection";
                }
                else // Last question (Movement type from player prefs)
                {
                    if (PlayerPrefs.GetInt("MovementType") == 1)
                    {
                        persistentData.container.s[i] = "Natural_walking";
                    }
                    else
                    {
                        persistentData.container.s[i] = "Steering";
                    }
                }
            }
            else
            {
                // Remove commas at the end of multiple choice
                if (persistentData.container.s[i].EndsWith(","))
                {
                    persistentData.container.s[i] = persistentData.container.s[i].Remove(persistentData.container.s[i].Length - 1);
                }
            }
        }
        StartCoroutine(Post(persistentData.container.s.ToList()));
    }

    public void
        SendData(List<float> data) // Call if sending float data only. Otherwise sending a string list is preferred.
    {
        List<string> tempConvertedData = new List<string>();

        // Culture specification to get . instead of , when converting to strings:

        foreach (float floatData in data)
        {
            tempConvertedData.Add(floatData.ToString(spec, ci));
        }

        StartCoroutine(Post(tempConvertedData));
    }

    public void
        SendData(List<string> data) // Preferred to use this function, and do the float conversion as seen above elsewhere if needed.
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
            Debug.LogError(
                "Result POST error: data list received is not the same length as entry ID array. Make sure they have the same length.");
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

        EditorGUILayout.HelpBox("The order and the amount of entry IDs must be the same as on your Google Form",
            MessageType.Info);
        EditorGUILayout.HelpBox(
            "This means the data sent to this data handler script must be in the same order to send the data to the correct entry IDs on Google Forms.",
            MessageType.None);

        DrawDefaultInspector();

        if (!script.OnlyOneLogPerDevice)
            if (PlayerPrefs.GetInt("dataSubmitted") == 1)
                PlayerPrefs.SetInt("dataSubmitted", 0);
    }
}
#endif

#endregion