using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VOGStateAuthEmail : VOGStateBase
{
    public VOGStateAuthPassword PasswordState;
    public VOGStateBase NewUserState;
    public string VostopiaLogo;

    string Email = "";

    public override void OnDrawGui(VOGController ctrl)
    {
        BeginWindow();

        //Header
        Heading("Please enter your email address:");
        Body("(or your pre-existing Vostopia username)");

        SetDefaultControl();
        Email = TextField(Email, ctrl.InputEnabled(this));

        GUILayout.BeginHorizontal();

        SetBackButton();
        if (BackButton("Back", ctrl.InputEnabled(this), GUILayout.Width(SplitButtonWidth)))
        {
            ctrl.StartBackTransition();
        }

        SetDefaultButton();
        if (Button("Continue", ctrl.InputEnabled(this), GUILayout.Width(SplitButtonWidth)))
        {
            StartCoroutine(Continue(ctrl));
        }

        GUILayout.EndHorizontal();

        Body("By continuing you agree to the Vostopia");
        if (LinkButton("terms and conditions", ctrl.InputEnabled(this)))
        {
            Application.OpenURL("http://vostopia.com/en/vossa/legal/tos/");
        }

        //Footer logo
        GUILayout.FlexibleSpace();
        GUILayout.Label(ctrl.GetTexture(VostopiaLogo), GUI.skin.FindStyle("image"), GUILayout.ExpandWidth(true));
        EndWindow();
    }

    private IEnumerator Continue(VOGController ctrl)
    {
        try
        {
            ctrl.DisableInput();

            //wait for connection
            while (!VostopiaClient.HasSessionKey)
            {
                yield return null;
            }

            //See if user exists
            ApiCall call = VostopiaClient.Authentication.BeginFindUser(Email);
            IEnumerator e = call.Wait();
            while (e.MoveNext()) { yield return e.Current; }
            VostopiaUser user = VostopiaClient.Authentication.EndFindUser(call);

            if (user != null)
            {
                var passwordData = new VOGStateAuthPassword.PasswordData();
                passwordData.CancelOnBack = false;
                passwordData.User = user;
                ctrl.StartTransition(PasswordState, passwordData);
            }
            else
            {
                if (!ValidateEmail(Email))
                {
                    ctrl.ShowMessageDialog("Oops!", "Please enter a valid email address.", () => { });
                    yield break;
                }
                else
                {
                    ctrl.StartTransition(NewUserState, Email);
                }
            }

        }
        finally
        {
            ctrl.EnableInput();
        }
    }

    private bool ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

        if (!email.Contains("@"))
        {
            return false;
        }

        return true;
    }


}
