using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VOGStateAuthNewGuest : VOGStateBase
{
    public override void OnDrawGui(VOGController ctrl)
    {
        BeginWindow();

        //Header
        Heading("Really?");
        Body("It's a lot more fun to sign up - that way you'll get a customisable avatar you can use across multiple games.");

        SetBackButton();
        if (BackButton("Oh, go on then!", ctrl.InputEnabled(this)))
        {
            ctrl.StartBackTransition(); ;
        }

        Body("If you really prefer not to, we won't hold it against you ;-)\nYou can continue as a guest by choosing a gender for your avatar:");

        GUILayout.BeginHorizontal();

        SetDefaultControl();
        SetDefaultButton();
        if (BackButton("Male", ctrl.InputEnabled(this), GUILayout.Width(SplitButtonWidth)))
        {
            StartCoroutine(OnNewGuest(ctrl as VOGControllerAuth, AvatarGender.Male));
        }

        if (Button("Female", ctrl.InputEnabled(this), GUILayout.Width(SplitButtonWidth)))
        {
            StartCoroutine(OnNewGuest(ctrl as VOGControllerAuth, AvatarGender.Female));
        }

        GUILayout.EndHorizontal();

        EndWindow();
    }

    public IEnumerator OnNewGuest(VOGControllerAuth ctrl, AvatarGender gender)
    {
        try
        {
            ctrl.DisableInput();

            //Create guest
            ApiCall call = VostopiaClient.Authentication.BeginSignInGuest(gender);
            IEnumerator e = call.Wait();
            while (e.MoveNext()) { yield return e.Current; }
            if (!VostopiaClient.Authentication.EndSignIn(call))
            {
                ctrl.ShowMessageDialog("Uh-oh", "There was an error creating a guest account for you. Please try again.", () => { });
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
