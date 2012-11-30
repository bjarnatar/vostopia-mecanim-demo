using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VOGStateAuthPassword : VOGStateBase
{
    public class PasswordData
    {
        public VostopiaUser User;
        public bool CancelOnBack = false;
    }

    public string VostopiaLogo;

    PasswordData CurrentPasswordData;
    string Password = "";

    public override void OnDataChanged(object data)
    {
        base.OnDataChanged(data);

        CurrentPasswordData = data as PasswordData;
        if (CurrentPasswordData == null)
        {
            Debug.LogError("Missing password data to password dialog");
        }
    }

    public override void OnDrawGui(VOGController ctrl)
    {
        BeginWindow();

        //Header
        Heading("Welcome back!");
        Body("Please enter your vostopia password and we'll fetch your avatar for you:");
        SetDefaultControl();
        Password = PasswordField(Password, ctrl.InputEnabled(this));

        GUILayout.BeginHorizontal();

        SetBackButton();
        if (BackButton("Back", ctrl.InputEnabled(this), GUILayout.Width(SplitButtonWidth)))
        {
            ctrl.StartBackTransition();
        }

        SetDefaultButton();
        if (Button("Continue", ctrl.InputEnabled(this), GUILayout.Width(SplitButtonWidth)))
        {
            StartCoroutine(OnAuthenticate(ctrl as VOGControllerAuth));
        }

        GUILayout.EndHorizontal();

        Heading("Oops!");
        Body("Forgotten or never received your password? No problem.");
        if (Button("Send me a new password", ctrl.InputEnabled(this)))
        {
            StartCoroutine(OnSendNewPassword(ctrl as VOGControllerAuth));
        }

        EndWindow();
    }

    public IEnumerator OnAuthenticate(VOGControllerAuth ctrl)
    {
        try
        {
            ctrl.DisableInput();


            if (CurrentPasswordData == null || CurrentPasswordData.User == null)
            {
                ctrl.ShowMessageDialog("Doh!", "An error occurred during authentication.  Please try again", () => { });
                yield break;
            }
            VostopiaUser user = CurrentPasswordData.User;


            //Try to authenticate user
            ApiCall call = VostopiaClient.Authentication.BeginSignIn(user.Username, Password, true);
            IEnumerator e = call.Wait();
            while (e.MoveNext()) { yield return e.Current; }
            if (!VostopiaClient.Authentication.EndSignIn(call))
            {
                ctrl.ShowMessageDialog("Uh-oh", "The password you entered doesn't match our records.  Please try again or use the 'Send me a new password' option below.", () => { });
                yield break;
            }

            //Store authentication key
            if (call.ResponseData["authKey"] != null)
            {
                ctrl.StoreUserAuthenticationKey(user, (string)call.ResponseData["authKey"]);
            }

            ctrl.AuthenticationCompleted();
        }
        finally
        {
            ctrl.EnableInput();
        }
    }

    public IEnumerator OnSendNewPassword(VOGController ctrl)
    {
        try
        {
            ctrl.DisableInput();

            if (CurrentPasswordData == null || CurrentPasswordData.User == null)
            {
                ctrl.ShowMessageDialog("Doh!", "An error occurred during authentication. Please try again", () => { });
                yield break;
            }

            ApiCall call = VostopiaClient.Authentication.BeginRecoverPassword(CurrentPasswordData.User.Username);
            IEnumerator e = call.Wait();
            while (e.MoveNext()) { yield return e.Current; }
            if (VostopiaClient.Authentication.EndRecoverPassword(call))
            {
                ctrl.ShowMessageDialog("Email Sent", "Alright! We've sent out your new password. Please check your email in a few minutes and return here to enter your password.", () => { });
            }
            else
            {
                ctrl.ShowMessageDialog("Doh!", "An error occurred during sending recovery email. Please try again", () => { });
            }
        }
        finally
        {
            ctrl.EnableInput();
        }
    }
}
