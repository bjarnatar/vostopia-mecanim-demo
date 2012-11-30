using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum AvatarEyeExpression
{
    Default = 0,
    RaisedEyebrow = 1,
    Angry = 2,
    Sad = 3,

    LookLeft = 4,
    LookRight = 5,
    LookLeftSneaky = 6,
    LookRightSneaky = 7,

    LookLeftUp = 8,
    LookRightUp = 9,
    LookDown = 10,
    Tired = 11,

    Shocked = 12,
    Crying = 13,
    Closed = 14,
    Glee = 15,

    Dizzy = 16,
    OMG = 17,
    Squint = 18,
    KnockedOut = 19
}

public enum AvatarMouthExpression
{
    Default = 0,
    SmallGrin = 1,
    BigGrin = 2,
    CheesyGrin = 3,

    Smile = 4,
    OpenSmile = 5,
    OpenWideSmile = 6,
    CheekyLaugh = 7,

    Grimace = 8,
    OpenGrimace = 9,
    OpenWideGrimace = 10,
    PhonemeO = 11,

    Sneer = 12,
    SmallClenchedTeeth = 13,
    BigClenchedTeeth = 14,
    Wibble = 15,

    Mischievous = 16,
    TongueOut = 17,
    Pout = 18,
    Drool = 19,

    PhonemeFV = 20,
    Sour = 21,
    PhonemeU = 22,
    PuffedUp = 23,
}

[ExecuteInEditMode]
[RequireComponent(typeof(AvatarFaceController))]
public class AvatarFaceAnimationController : MonoBehaviour
{
    internal static Dictionary<AvatarEyeExpression, AvatarEyeExpression> MirrorExpression = new Dictionary<AvatarEyeExpression, AvatarEyeExpression>()
    {
        {AvatarEyeExpression.LookLeft, AvatarEyeExpression.LookRight},
        {AvatarEyeExpression.LookRight, AvatarEyeExpression.LookLeft},
        {AvatarEyeExpression.LookLeftSneaky, AvatarEyeExpression.LookRightSneaky},
        {AvatarEyeExpression.LookRightSneaky, AvatarEyeExpression.LookLeftSneaky},
        {AvatarEyeExpression.LookLeftUp, AvatarEyeExpression.LookRightUp},
        {AvatarEyeExpression.LookRightUp, AvatarEyeExpression.LookLeftUp},
    };

    //blinking
    public bool AutomaticBlinking = true;
    public bool BlinkInExpressions = false;
    public AvatarEyeExpression EyeBlinkFrame = AvatarEyeExpression.Closed;
    public float BlinkDelay = 3;
    public float BlinkDelaySpread = 0.3f;
    public float BlinkDuration = 0.1f;
    private bool IsBlinking = false;

    //current frame (without blinking)
    public AvatarEyeExpression LeftEyeFrame = AvatarEyeExpression.Default;
    public AvatarEyeExpression RightEyeFrame = AvatarEyeExpression.Default;
    public AvatarMouthExpression MouthFrame = AvatarMouthExpression.Default;

    private AvatarFaceController _FaceCtrl;
    public AvatarFaceController FaceCtrl
    {
        get
        {
            if (_FaceCtrl == null)
            {
                _FaceCtrl = GetComponent<AvatarFaceController>();
            }
            return _FaceCtrl;
        }
    }

    public void Start()
    {
        LeftEyeFrame = AvatarEyeExpression.Default;
        RightEyeFrame = AvatarEyeExpression.Default;
        MouthFrame = AvatarMouthExpression.Default;
        StartCoroutine(BlinkRunner());
    }

    public void OnLevelWasLoaded()
    {
        MouthFrame = AvatarMouthExpression.Default;
        LeftEyeFrame = AvatarEyeExpression.Default;
        RightEyeFrame = AvatarEyeExpression.Default;
    }

    public void Update()
    {
        //Left Eye
        var leftExpr = LeftEyeFrame;
        if (IsBlinking && (leftExpr == AvatarEyeExpression.Default || BlinkInExpressions))
        {
            leftExpr = EyeBlinkFrame;
        }
        FaceCtrl.LeftEyeFrame = (int)leftExpr;

        //Right Eye
        var rightExpr = RightEyeFrame;
        if (IsBlinking && (rightExpr == AvatarEyeExpression.Default || BlinkInExpressions))
        {
            rightExpr = EyeBlinkFrame;
        }
        if (MirrorExpression.ContainsKey(rightExpr))
        {
            rightExpr = MirrorExpression[rightExpr];
        }
        FaceCtrl.RightEyeFrame = (int)rightExpr;

        //Mouth
        FaceCtrl.MouthFrame = (int)MouthFrame;
    }

    #region Animation Interface

    public void SetEyeExpression(AvatarEyeExpression expr)
    {
        LeftEyeFrame = expr;
        RightEyeFrame = expr;
    }

    public void SetRightEyeExpression(AvatarEyeExpression expr)
    {
        RightEyeFrame = expr;
    }

    public void SetLeftEyeExpression(AvatarEyeExpression expr)
    {
        LeftEyeFrame = expr;
    }

    public void SetMouthExpression(AvatarMouthExpression expr)
    {
        MouthFrame = expr;
    }

    public void DisableBlinking()
    {
        AutomaticBlinking = false;
    }

    public void EnableBlinking()
    {
        AutomaticBlinking = true;
    }

    #endregion


    private IEnumerator BlinkRunner()
    {
        while (true)
        {
            //wait for next blink
            float nextBlinkWait = BlinkDelay * (1 + Random.Range(-BlinkDelaySpread, BlinkDelaySpread));
            yield return new WaitForSeconds(nextBlinkWait);

            if (AutomaticBlinking)
            {
                //blink
                IsBlinking = true;
                yield return new WaitForSeconds(BlinkDuration);
                IsBlinking = false;
            }
        }
    }

}
