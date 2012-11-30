using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VOGStateMessageDialog : VOGStateBase
{
    public class DialogData
    {
        public string Header;
        public string Body;
        public System.Action Callback;
    }

    DialogData Data;

    public override void OnDataChanged(object data)
    {
        base.OnDataChanged(data);
        Data = data as DialogData;
    }

    public override void OnStateEnable(VOGController ctrl)
    {
        base.OnStateEnable(ctrl);
        if (Data == null)
        {
            Debug.LogError("VOGMessageDialog without DialogData");
        }
    }

    public override void OnStateDisable(VOGController ctrl)
    {
        base.OnStateDisable(ctrl);
        if (Data != null && Data.Callback != null)
        {
            Data.Callback();
        }
        Data = null;
    }
    
    public override void OnDrawGui(VOGController ctrl)
    {
        if (Data == null)
        {
            Data = new DialogData();
        }

        ShadeScreen();

        BeginWindow(280, 150);

        HeadingCentered(Data.Header ?? "");
        BodyCentered(Data.Body ?? "");

        GUILayout.FlexibleSpace();
        if (Button("Ok", ctrl.InputEnabled(this)))
        {
            ctrl.StartBackTransition();
        }

        EndWindow();
    }

}
