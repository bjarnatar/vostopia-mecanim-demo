using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VOGStateAuthDidYouKnow : VOGStateBase
{
    public string VostopiaLogo;

    float LogoVerticalSpace = 10;

    public override void OnDrawGui(VOGController ctrl)
    {
        BeginWindow();

        //Header
        Heading("Did you know?");
        Body("The avatar you've just created can be used in multiple games.  Look out for the Vostopia logo in other games!");

        GUILayout.Space(LogoVerticalSpace);
        GUILayout.Label(ctrl.GetTexture(VostopiaLogo), GUI.skin.FindStyle("image"), GUILayout.ExpandWidth(true));
        GUILayout.Space(LogoVerticalSpace);

        SetDefaultControl();
        SetDefaultButton();
        if (BackButton("Awesome, let's go!", ctrl.InputEnabled(this)))
        {
            (ctrl as VOGControllerAuth).AuthenticationCompleted();
        }

        EndWindow();
    }
}
