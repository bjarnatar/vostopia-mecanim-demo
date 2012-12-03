using UnityEngine;
using System.Collections;

public class BootLoader : MonoBehaviour {

    public VostopiaApiController VostopiaClientInitializer;
    public string MenuSceneName;

	// Use this for initialization
	void Start () 
    {
        if (VostopiaClientInitializer != null)
        {
            VostopiaClientInitializer.Connect();
        }
	}
	
	// Update is called once per frame
	void Update () {
		if (VostopiaClient.IsAuthenticated)
		{
            if (MenuSceneName != null && MenuSceneName != "")
            {
                Application.LoadLevel(MenuSceneName);
            }
		}
	}
}
