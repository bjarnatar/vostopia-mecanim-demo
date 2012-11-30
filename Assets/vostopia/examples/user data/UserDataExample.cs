using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

public class UserDataExample : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitForUserData());
    }

    bool hasUserData;

    public IEnumerator WaitForUserData()
    {
        while (!VostopiaClient.IsAuthenticated)
        {
            yield return null;
        }

        while (!VostopiaClient.User.UserData.IsLoaded)
        {
            yield return null;
        }
        hasUserData = true;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 500, 400));

		GUILayout.Label("This example shows how to store and retrieve custom user data. Use UserData to store level progression, preferences or other persistant data for the logged on user.");

        if (hasUserData)
        {
            VostopiaUserData userData = VostopiaClient.User.UserData;
            
			GUILayout.Label("Number of Clicks: " + userData.GetInt("clicks"));
            if (GUILayout.Button("click me"))
            {
                userData.SetInt("clicks", userData.GetInt("clicks") + 1);
            }

            GUILayout.Label("Number of Clicks2: " + userData.GetInt("clicks2"));
            if (GUILayout.Button("click me"))
            {
                userData.SetInt("clicks2", userData.GetInt("clicks2") + 1);
            }
        }
        else
        {
            GUILayout.Label("loading...");
        }

        GUILayout.EndArea();
    }

}
