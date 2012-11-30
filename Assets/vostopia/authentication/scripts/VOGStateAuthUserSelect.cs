using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VOGStateAuthUserSelect : VOGStateBase
{
    public string VostopiaLogo;
    public string ProfilePictureFrame;
    public string ProfilePictureBackground;
    public string ProfilePictureMissing;

    public VOGStateBase AuthSelectorScreen;

    [HideInInspector]
    public VOGControllerAuth.StoredAuthenticationKey UserAuth;

    private WWW profilePictureDownload;
    private Texture2D profilePicture;

    public override void OnStateDisable(VOGController ctrl)
    {
        base.OnStateDisable(ctrl);

        if (profilePicture != null)
        {
            Object.Destroy(profilePicture);
            profilePicture = null;
        }
        if (profilePictureDownload != null)
        {
            profilePictureDownload.Dispose();
            profilePictureDownload = null;
        }
    }

    public override void OnStateEnable(VOGController ctrl)
    {
        base.OnStateEnable(ctrl);

        StartCoroutine(LoadProfilePicture());
    }

    public override void OnDataChanged(object data)
    {
        base.OnDataChanged(data);

        UserAuth = data as VOGControllerAuth.StoredAuthenticationKey;
        if (UserAuth == null)
        {
            Debug.LogWarning("Updating VOGStateAuthUserSelect with data '" + data + "' != StoredAuthenticationKey");
            return;
        }

        //System.UriBuilder uriBuilder = new System.UriBuilder(VostopiaClient.ApiUrl);
        //uriBuilder.Path = string.Format("/profilepicture/icon/{0}", UserAuth.UserId);
        //ProfileIcon.TextureUrl = uriBuilder.ToString();
    }

    public override void OnDrawGui(VOGController ctrl)
    {
        BeginWindow();

        //Header
        Heading("Hi and welcome back!");
        Body("You've been here before haven't you?\nJust click the button below to sign in again.");

        //Profile Picture
        GUILayout.BeginHorizontal();

        //Image
        var imageStyle = GUI.skin.FindStyle("image");
        GUILayout.Label(ctrl.GetTexture(ProfilePictureBackground), imageStyle);
        var imageRect = GUILayoutUtility.GetLastRect();
        if (profilePicture != null)
        {
            float sizeOffset = 4;
            GUI.Label(new Rect(imageRect.xMin + sizeOffset, imageRect.yMin + sizeOffset, imageRect.width - 2 * sizeOffset, imageRect.height - 2 * + sizeOffset), profilePicture, imageStyle);
        }
        else
        {
            GUI.Label(imageRect, ctrl.GetTexture(ProfilePictureMissing), imageStyle);
        }
        GUI.Label(imageRect, ctrl.GetTexture(ProfilePictureFrame), imageStyle);

        //Username
        GUILayout.Label(UserAuth.DisplayName, GUI.skin.FindStyle("label-profilepicture"));

        GUILayout.EndHorizontal();

        SetDefaultControl();
        SetDefaultButton();
        if (Button("Sweet, sign me in", ctrl.InputEnabled(this)))
        {
            StartCoroutine(OnSignIn(ctrl as VOGControllerAuth));
        }

        SetBackButton();
        if (BackButton("I'm someone else!", ctrl.InputEnabled(this)))
        {
            OnAnotherUser(ctrl);
        }

        //Footer logo
        GUILayout.FlexibleSpace();
        GUILayout.Label(ctrl.GetTexture(VostopiaLogo), GUI.skin.FindStyle("image"), GUILayout.ExpandWidth(true));
        EndWindow();
    }

    public IEnumerator OnSignIn(VOGControllerAuth ctrl)
    {
        try
        {
            ctrl.DisableInput();

            var authSelectData = new VOGStateAuthSelect.AuthSelectData();
            authSelectData.EnableCancel = false;

            //Make sure we have UserAuth object
            if (UserAuth == null || string.IsNullOrEmpty(UserAuth.AuthKey))
            {
                ctrl.ShowMessageDialog("Doh!", "An error occurred during authentication. Please try again", () =>
                {
                    ctrl.StartTransition(AuthSelectorScreen, authSelectData);
                });
                yield break;
            }

            //Try to authenticate user
            ApiCall call = VostopiaClient.Authentication.BeginSignInAuthKey(UserAuth.AuthKey);
            IEnumerator e = call.Wait();
            while (e.MoveNext()) { yield return e.Current; }
            if (!VostopiaClient.Authentication.EndSignIn(call))
            {
                ctrl.ShowMessageDialog("Session Expired", "Your authentication session has expired. Please sign in again.", () =>
                {
                    ctrl.StartTransition(AuthSelectorScreen, authSelectData);
                });
                yield break;
            }

            //Continue to auth completed screen
            ctrl.AuthenticationCompleted();
        }
        finally
        {
            ctrl.EnableInput();
        }
    }

    public void OnAnotherUser(VOGController ctrl)
    {
        var authSelectData = new VOGStateAuthSelect.AuthSelectData();
        authSelectData.EnableCancel = false;
        ctrl.StartTransition(AuthSelectorScreen, authSelectData);
    }

    public IEnumerator LoadProfilePicture()
    {
        while (!VostopiaClient.HasSessionKey)
        {
            yield return null;
        }

        string url = null;
        if (UserAuth != null)
        {
            url = AssetDownloadManager.AssetBaseUrlDownload(string.Format("/profilepicture/icon/{0}/", UserAuth.UserId));
            profilePictureDownload = new WWW(url);
            yield return profilePictureDownload;
        }

        if (profilePictureDownload != null && string.IsNullOrEmpty(profilePictureDownload.error))
        {
            profilePicture = profilePictureDownload.texture;
        }
    }

}
