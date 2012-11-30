using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * This example shows how to integrate an external user database with 
 * vostopia users. The external user database is here represented by a list of 
 * user objects. In a real implementation, the user database would be hosted on a server
 * somewhere and the registration and authentication process would be more complicated than shown
 * here.
 * 
 * The purpose of this example is to show how you can store the AuthenticationToken and use it 
 * to automatically log in a user.
 */ 
public class LinkedAccountsExample : MonoBehaviour
{
    public class User
    {
        public string Username;
        public string Password;
        public string VostopiaUserId;
        public string AuthToken;
    }

    public VostopiaApiController ApiController;
    public bool IsAuthenticating;

    [HideInInspector]
    public List<User> Users = new List<User>();

    public User CurrentUser;

    public void Start()
    {
        if (ApiController == null)
        {
            ApiController = GameObject.Find("VostopiaApiController").GetComponent<VostopiaApiController>();
        }

        ApiController.AuthenticationSettings.OnAuthenticationCanceled += OnAuthCanceled;
        ApiController.AuthenticationSettings.OnAuthenticationCompleted += OnAuthCompleted;
    }

    public IEnumerator SignOut()
    {
        var call = VostopiaClient.Authentication.BeginSignOut();
        var e = call.Wait();
        while (e.MoveNext()) { yield return e.Current; }
        VostopiaClient.Authentication.EndSignOut(call);
    }

    private string guiUsername = "";
    private string guiPassword = "";

    public void OnGUI()
    {
        if (!IsAuthenticating)
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, Screen.height - 20));

			GUILayout.Label("This example shows how to link the vostopia authentication system with an external authentication mechanism. See the docs for more information");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Username (non-vostopia):");
            guiUsername = GUILayout.TextField(guiUsername);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Password (non-vostopia):");
            guiPassword = GUILayout.TextField(guiPassword);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create User"))
            {
                var existingUser = Users.Find((user) => user.Username == guiUsername);
                if (existingUser != null)
                {
                    Debug.LogWarning("User alreday exists in database");
                }
                else
                {
                    var u = new User();
                    u.Username = guiUsername;
                    u.Password = guiPassword;
                    CurrentUser = u;
                    Users.Add(u);

                    //Connect 
                    ApiController.AuthenticationSettings.AuthenticationToken = u.AuthToken;
                    ApiController.Connect();
                    IsAuthenticating = true;
                }
            }
            if (GUILayout.Button("Log In"))
            {
                var existingUser = Users.Find((user) => user.Username == guiUsername && user.Password == guiPassword);
                if (existingUser != null)
                {
                    //Connect 
                    CurrentUser = existingUser;

                    ApiController.AuthenticationSettings.AuthenticationToken = existingUser.AuthToken;
                    ApiController.Connect();
                    IsAuthenticating = true;
                }
                else
                {
                    Debug.LogWarning("User Not Found");
                }
            }
            GUILayout.EndHorizontal();

            if (VostopiaClient.IsAuthenticated)
            {
                var vuser = VostopiaClient.Authentication.ActiveUser;
                GUILayout.Label("Vostopia User Logged In As " + vuser.DisplayName + "(" + vuser.Id + ")");
                if (GUILayout.Button("Log Out"))
                {
                    StartCoroutine(SignOut());
                }
            }
            else
            {
                GUILayout.Label("Not logged in to vostopia");
            }

            GUILayout.Label("Linked Users");
            foreach (var u in Users)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(u.Username + ", vostopiaUserId: " + u.VostopiaUserId);
                if (GUILayout.Button("Expire Authentication Token"))
                {
                    //simulate that the authentication token has expires for some reason (most likely user has changed vostopia password)
                    var auth = Newtonsoft.Json.Linq.JObject.Parse(u.AuthToken);
                    auth["AuthKey"] = "...";
                    u.AuthToken = auth.ToString();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();

        }
    }

    public void OnAuthCompleted(VostopiaAuthenticationSettings.AuthenticationCompletedArgs e)
    {
        IsAuthenticating = false;

        CurrentUser.AuthToken = e.AuthenticationToken;
        CurrentUser.VostopiaUserId = e.User.Id;
    }

    public void OnAuthCanceled(VostopiaAuthenticationSettings.AuthenticationCanceledArgs e)
    {
        IsAuthenticating = false;
    }


}
