using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AnimationExample : MonoBehaviour
{

    void OnGUI()
    {
		if (!VostopiaClient.IsAuthenticated)
		{
			return;
		}

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));

		GUILayout.Label("This example shows gives a basic example of how to play animations on vostopia avatars.");

        GUILayout.EndArea();
    }

}
