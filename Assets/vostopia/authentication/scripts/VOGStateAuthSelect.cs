using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VOGStateAuthSelect : VOGStateBase
{
    public class AuthSelectData
    {
        public bool EnableCancel = false;
    }

    public VOGStateBase EmailState;
    public VOGStateBase GuestState;

    public string VostopiaLogo;

    [HideInInspector]
    AuthSelectData CurrentAuthSelectData;

    public override void OnDataChanged(object data)
    {
        base.OnDataChanged(data);

        CurrentAuthSelectData = data as AuthSelectData;
        if (CurrentAuthSelectData == null)
        {
            Debug.LogWarning("Updating VOGAuthSelect with data '" + data + "' != AuthSelectData");
            return;
        }
    }

    public override void OnDrawGui(VOGController ctrl)
    {
        BeginWindow();

        //Header
        Heading("Hi and welcome!");
        Body("This game uses Vostopia avatars!\nThey exist across multiple games and apps, and you can use the same avatar everywhere that uses the Vostopia system.\n\nIn order to find your avatar, we need a way to remember you by:");

        //Ok button
        SetDefaultControl();
        SetDefaultButton();
        if (Button("Cool, let's do this", ctrl.InputEnabled(this)))
        {
            ctrl.StartTransition(EmailState);
        }
        
        //Cancel button
        SetBackButton();
        if (CurrentAuthSelectData.EnableCancel)
        {
            if (BackButton("Cancel", ctrl.InputEnabled(this)))
            {
                OnCancel(ctrl as VOGControllerAuth);
            }
        }
        else
        {
            if (BackButton("I don't wish to be remembered!", ctrl.InputEnabled(this)))
            {
                StartCoroutine(OnSignInGuest(ctrl as VOGControllerAuth));
            }
        }

        //Footer logo
        GUILayout.FlexibleSpace();
        GUILayout.Label(ctrl.GetTexture(VostopiaLogo), GUI.skin.FindStyle("image"), GUILayout.ExpandWidth(true));
        EndWindow();
    }


    /**
     * Sign in as guest. If a guest authkey is found in playerprefs, use that to log in as the same guest. 
     * If no guest authkey is found, show the sign in as guest screen, where the user gets to select a gender.
     */
    public IEnumerator OnSignInGuest(VOGControllerAuth ctrl)
    {
        try
        {
            ctrl.DisableInput();

            //Try to authenticate as the same guest as last time (if there's a last time)
            string authKey = ctrl.GetStoredGuestAuthenticationKey();

            //Don't use saved guest account if ctrl is pressed while clicking on the button
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                authKey = null;
            }

            bool completed = false;
            if (!string.IsNullOrEmpty(authKey))
            {
                ApiCall call = VostopiaClient.Authentication.BeginSignInAuthKey(authKey);
                IEnumerator e = call.Wait();
                while (e.MoveNext()) { yield return e.Current; }
                if (VostopiaClient.Authentication.EndSignIn(call))
                {
                    //Complete auth
                    ctrl.AuthenticationCompleted();
                    completed = true;
                }
            }

            //otherwise, send on to gender select screen
            if (!completed)
            {
                ctrl.StartTransition(GuestState);
            }
        }
        finally
        {
            ctrl.EnableInput();
        }
    }

    /** 
     * Cancel authentication process
     */
    public void OnCancel(VOGControllerAuth ctrl)
    {
        ctrl.AuthenticationCanceled();
    }

}
