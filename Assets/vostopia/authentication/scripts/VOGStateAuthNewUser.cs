using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VOGStateAuthNewUser : VOGStateBase
{
    public VOGStateBase DidYouKnowState;

    string Email;
    bool KeepInTouch = true;

    public override void OnDataChanged(object data)
    {
        base.OnDataChanged(data);

        Email = data as string;
        if (Email == null)
        {
            Debug.LogError("Missing email to password prompt");
        }
    }

    public override void OnDrawGui(VOGController ctrl)
    {
        BeginWindow();

        //Header
        Heading("Welcome!");
        Body("Please choose what gender you'd like your avatar to be:");

        SetDefaultControl();
        GUILayout.BeginHorizontal();

        SetDefaultButton();
        if (Button("Male", ctrl.InputEnabled(this), GUILayout.Width(SplitButtonWidth)))
        {
            StartCoroutine(OnNewUser(ctrl as VOGControllerAuth, AvatarGender.Male));
        }

        if (Button("Female", ctrl.InputEnabled(this), GUILayout.Width(SplitButtonWidth)))
        {
            StartCoroutine(OnNewUser(ctrl as VOGControllerAuth, AvatarGender.Male));
        }

        GUILayout.EndHorizontal();

        Body("Don't worry, you can change this later if you wish.");

        KeepInTouch = Toggle(KeepInTouch, "Keep in touch about avatar games", ctrl.InputEnabled(this));

        GUILayout.FlexibleSpace();

        SetBackButton();
        if (BackButton("Back", ctrl.InputEnabled(this)))
        {
            ctrl.StartBackTransition();
        }
        EndWindow();
    }

    public IEnumerator OnNewUser(VOGControllerAuth ctrl, AvatarGender gender)
    {
        try
        {
            ctrl.DisableInput();

            //Create user
            ApiCall call = VostopiaClient.Authentication.BeginRegister(Email, KeepInTouch, gender);
            IEnumerator e = call.Wait();
            while (e.MoveNext()) { yield return e.Current; }
            var registerResult = VostopiaClient.Authentication.EndRegister(call);
            if (registerResult != VostopiaAuthenticationClient.RegisterResult.SUCCESS)
            {
                Debug.LogWarning("Error creating new user, " + registerResult);
                string msg = "";
                if (registerResult == VostopiaAuthenticationClient.RegisterResult.USER_ALREADY_EXISTS)
                {
                    msg = "That email address is already in use!  Please try another.";
                }
                else if (registerResult == VostopiaAuthenticationClient.RegisterResult.INVALID_EMAIL)
                {
                    msg = "That email address doesn't make sense to us =(  Please try again.";
                }
                else
                {
                    msg = "Something went wrong while creating your account =(  Please try again.";
                }

                //Show message box and return to previous screen
                ctrl.ShowMessageDialog("Uh-oh", msg, () =>
                {
                    ctrl.StartBackTransition();
                });

                yield break;
            }

            //Store authentication key
            if (call.ResponseData["authKey"] != null)
            {
                string authKey = (string)call.ResponseData["authKey"];
                ctrl.StoreGuestAuthenticationKey(authKey);
            }

            //Continue to auth completed screen
            ctrl.AuthenticationCompleted();
        }
        finally
        {
            ctrl.EnableInput();
        }
    }

}
