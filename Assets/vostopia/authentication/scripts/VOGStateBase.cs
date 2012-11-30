using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class VOGStateBase : MonoBehaviour
{
    public virtual void OnDataChanged(object data) { }
    public virtual void OnStateEnable(VOGController ctrl) 
    {
        GUI.FocusControl("disabled");
    }
   
    public virtual void OnStateDisable(VOGController ctrl) 
    {
        GUI.FocusControl("disabled");
    }

    public abstract void OnDrawGui(VOGController ctrl);

    protected float ScreenSizeMultiplier = 1;
    public Color WindowBackground = new Color(.47f, .78f, .35f, 1f);
    public Color ScreenShade = new Color(0, 0, 0, .5f);
    public Color ShadowColor = new Color(0, 0, 0, 0.35f);
    private int _SplitButtonWidth = 145;
    protected int SplitButtonWidth {
        get
        {
            return Mathf.RoundToInt(_SplitButtonWidth * ScreenSizeMultiplier);
        }
    }

    protected GUIStyle StyleHeadingCentered;
    protected GUIStyle StyleBodyCentered;

    protected GUIStyle StyleButtonActive;
    protected GUIStyle StyleButtonBackActive;

#if UNITY_IPHONE || UNITY_ANDROID
    private TouchScreenKeyboard keyboard;
#endif

    #region Controls

    protected void ShadeScreen()
    {
        Color c = GUI.backgroundColor;
        GUI.backgroundColor = ScreenShade;
        var style = GUI.skin.FindStyle("background-color");
        GUI.Label(new Rect(-Screen.width, -Screen.height, 3 * Screen.width, 3 * Screen.height), "", style);
        GUI.backgroundColor = c;
    }

    protected void BeginWindow(float width = 320, float height = 320)
    {
        int offsetX = Mathf.RoundToInt(2 * ScreenSizeMultiplier);
        width *= ScreenSizeMultiplier;
        height *= ScreenSizeMultiplier;

        var style = GUI.skin.FindStyle("window");
        Color oldCol = GUI.backgroundColor;
        GUI.backgroundColor = WindowBackground;
        GUILayout.BeginArea(new Rect((Screen.width - width) / 2 + offsetX, (Screen.height - height) / 2, width, height), style);
        GUI.backgroundColor = oldCol;
    }

    protected void EndWindow()
    {
        GUILayout.EndArea();
    }

    protected void Heading(string text)
    {
        GUIStyle style = GUI.skin.FindStyle("heading");
        GUILayout.Label(text, style);
    }

    protected void HeadingCentered(string text)
    {
        if (StyleHeadingCentered == null)
        {
            StyleHeadingCentered = new GUIStyle(GUI.skin.FindStyle("heading"));
            StyleHeadingCentered.alignment = TextAnchor.MiddleCenter;
        }
        GUILayout.Label(text, StyleHeadingCentered);
    }

    protected void Body(string text)
    {
        GUIStyle style = GUI.skin.FindStyle("label");
        GUILayout.Label(text, style);
    }

    protected void BodyCentered(string text)
    {
        if (StyleBodyCentered == null)
        {
            StyleBodyCentered = new GUIStyle(GUI.skin.FindStyle("label"));
            StyleBodyCentered.alignment = TextAnchor.MiddleCenter;
        }
        GUILayout.Label(text, StyleBodyCentered);
    }

    protected bool Toggle(bool value, string text, bool enabled)
    {
        bool ret = value;
        GUIStyle style = GUI.skin.FindStyle("toggle");
        if (enabled)
        {
            GUI.SetNextControlName(CurrentControlName);
            if (IsActiveControl())
            {
                disableDefaultButton = true;
            }
            
            ret = GUILayout.Toggle(value, text, style);
            NextControl();
        }
        else
        {
            GUILayout.Label(text, style);
        }
        return enabled ? ret : value;
    }

    protected string TextField(string value, bool enabled)
    {
        GUIStyle style = GUI.skin.FindStyle("textfield");
        string ret = value;
        if (enabled)
        {
            GUI.SetNextControlName(CurrentControlName);
            if (IsActiveControl())
            {
                disableDefaultButton = false;
            }

            ret = GUILayout.TextField(value, style);
            NextControl();
        }
        else
        {
            GUILayout.Label(value, style);
        }

        return ret;
    }

    protected string PasswordField(string value, bool enabled)
    {
        GUIStyle style = GUI.skin.FindStyle("textfield");
        string ret = value;
        if (enabled)
        {
            GUI.SetNextControlName(CurrentControlName);
            if (IsActiveControl())
            {
                disableDefaultButton = false;
            }

#if UNITY_IPHONE || UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (!TouchScreenKeyboard.visible)
                {
                    string mask = new string('*', value.Length);
                    if (GUILayout.Button(mask, style))
                    {
                        if (keyboard != null)
                        {
                            keyboard.active = false;
                        }
                        keyboard = TouchScreenKeyboard.Open(value, TouchScreenKeyboardType.Default, false, false, true, false, "");
                    }
                }
                else
                {
                    if (keyboard != null)
                    {
                        ret = keyboard.text;
                    }
                    string mask = new string('*', value.Length);
                    GUILayout.Label(mask, style);
                }
            }
            else
#endif
            {
                ret = GUILayout.PasswordField(value, '*', style);
            }

            NextControl();
        }
        else
        {
            string mask = new string('*', value.Length);
            GUILayout.Label(mask, style);
        }
        return ret;
    }

    protected bool Button(string text, bool enabled, params GUILayoutOption[] options)
    {
        GUIStyle style = GUI.skin.FindStyle("button");
        Color cc = GUI.contentColor;

        //Draw background without foreground
        GUI.contentColor = new Color(0, 0, 0, 0);
        bool ret = false;
        if (enabled)
        {
            GUI.SetNextControlName(CurrentControlName);
            if (IsActiveControl())
            {
                disableDefaultButton = true;
            }

            if (IsActiveControl() || IsDefaultButton())
            {
                if (StyleButtonActive == null)
                {
                    StyleButtonActive = new GUIStyle(style);
                    StyleButtonActive.normal = StyleButtonActive.hover;
                }
                style = StyleButtonActive;
                if (IsKeyboardForward())
                {
                    ret = true;
                }
            }
            if (IsKeyboardBack() && IsBackButton())
            {
                ret = true;
            }
            ret = GUILayout.Button(text, style, options) || ret;

            NextControl();
        }
        else
        {
            GUILayout.Label(text, style, options);
        }
        Rect rect = GUILayoutUtility.GetLastRect();
        GUI.contentColor = cc;

        //Draw outlined foreground
        DrawOutlinedText(rect, text, style);

        return enabled ? ret : false;
    }

    protected bool BackButton(string text, bool enabled, params GUILayoutOption[] options)
    {
        GUIStyle style = GUI.skin.FindStyle("button-back");
        Color cc = GUI.contentColor;

        //Draw background without foreground
        GUI.contentColor = new Color(0, 0, 0, 0);
        bool ret = false;
        if (enabled)
        {
            //Focus
            GUI.SetNextControlName(CurrentControlName);
            if (IsActiveControl())
            {
                disableDefaultButton = true;
            }

            if (IsActiveControl() || IsDefaultButton())
            {
                if (StyleButtonBackActive == null)
                {
                    StyleButtonBackActive = new GUIStyle(style);
                    StyleButtonBackActive.normal = StyleButtonBackActive.hover;
                }
                style = StyleButtonBackActive;
                if (IsKeyboardForward())
                {
                    ret = true;
                }
            }
            if (IsKeyboardBack() && IsBackButton())
            {
                ret = true;
            }
            ret = GUILayout.Button(text, style, options) || ret;
            NextControl();
        }
        else
        {
            GUILayout.Label(text, style, options);
        }
        Rect rect = GUILayoutUtility.GetLastRect();
        GUI.contentColor = cc;

        //Draw outlined foreground
        DrawOutlinedText(rect, text, style);

        return enabled ? ret : false;
    }

    protected bool LinkButton(string text, bool enabled, params GUILayoutOption[] options)
    {
        GUIStyle style = GUI.skin.FindStyle("label-link");

        bool ret = false;
        if (enabled)
        {
            //Focus
            GUI.SetNextControlName(CurrentControlName);
            if (IsActiveControl())
            {
                disableDefaultButton = true;
            }

            if (IsActiveControl() || IsDefaultButton())
            {
                if (IsKeyboardForward())
                {
                    ret = true;
                }
            }
            if (IsKeyboardBack() && IsBackButton())
            {
                ret = true;
            }
            ret = GUILayout.Button(text, style, options) || ret;
            NextControl();
        }
        else
        {
            GUILayout.Label(text, style, options);
        }

        return enabled ? ret : false;
    }


    #endregion

    #region Active Control

    bool disableDefaultButton = false;
    int defaultButton = 0;
    int defaultControl = 0;
    int backButton = -1;

    int activeControl = -1;
    int currentControl = 0;
    int lastControl = int.MaxValue;

    bool keyboardForward = false;
    bool keyboardBack = false;

    public virtual void OnBeforeDrawGui(VOGController ctrl)
    {
        ScreenSizeMultiplier = ctrl.CurrentScreenSize.ScreenMultiplier;
        //GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(0.5f, 0.5f, 1)) * GUI.matrix;

        lastControl = currentControl;
        currentControl = 0;

        bool changed = false;
        if (Event.current.Equals(Event.KeyboardEvent("up")) || Event.current.Equals(Event.KeyboardEvent("#tab")))
        {
            Event.current.Use();
            changed = true;
            activeControl--;
        }
        else if (Event.current.Equals(Event.KeyboardEvent("down")) || Event.current.Equals(Event.KeyboardEvent("tab")))
        {
            Event.current.Use();
            changed = true;
            activeControl++;
        }

        keyboardBack = false;
        if (Event.current.Equals(Event.KeyboardEvent("escape")))
        {
            keyboardBack = true;
        }

        keyboardForward = false;
        if (Event.current.Equals(Event.KeyboardEvent("return")))
        {
            keyboardForward = true;
        }

        if (activeControl < 0)
        {
            activeControl = 0;
        }
        if (activeControl >= lastControl)
        {
            activeControl = lastControl - 1;
        }
        if (changed || !GUI.GetNameOfFocusedControl().StartsWith("Ctrl_" + name))
        {
            GUI.FocusControl(ControlName(activeControl));
        }

        //GUI.Label(new Rect(0, 0, 100, 20), "Active: " + GUI.GetNameOfFocusedControl());
    }

    void NextControl()
    {
        currentControl++;
    }

    protected void SetDefaultControl()
    {
        defaultControl = currentControl;
        if (activeControl == -1)
        {
            activeControl = defaultControl;
            GUI.FocusControl(ControlName(activeControl));
        }
    }

    protected void SetDefaultButton()
    {
        defaultButton = currentControl;
    }

    protected void SetBackButton()
    {
        backButton = currentControl;
    }

    bool IsActiveControl()
    {
        return currentControl == activeControl;
    }

    bool IsDefaultButton()
    {
        return defaultButton == currentControl && !disableDefaultButton;
    }

    bool IsBackButton()
    {
        return currentControl == backButton;
    }

    bool IsKeyboardForward()
    {
        return keyboardForward;
    }

    bool IsKeyboardBack()
    {
        return keyboardBack;
    }

    protected string CurrentControlName
    {
        get
        {
            return ControlName(currentControl);
        }
    }

    protected string ControlName(int controlIdx)
    {
        return "Ctrl_" + name + "_" + controlIdx;
    }


    #endregion

    #region Text Outline

    void DrawShadowText(Rect rect, string text, GUIStyle style)
    {
        Color cc = GUI.contentColor;
        Color bc = GUI.backgroundColor;

        //Draw text shadow
        GUI.contentColor = ShadowColor;
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        var shadowRect = new Rect(rect);
        float xOffset = 1;
        float yOffset = 1;
        shadowRect.center = new Vector2(rect.center.x + xOffset, rect.center.y + yOffset);
        GUI.Label(shadowRect, text, style);

        //Draw foreground without background
        GUI.contentColor = cc;
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        GUI.Label(rect, text, style);

        //Reset colors
        GUI.backgroundColor = bc;
        GUI.contentColor = cc;
    }

    void DrawOutlinedText(Rect rect, string text, GUIStyle style)
    {
        Color cc = GUI.contentColor;
        Color bc = GUI.backgroundColor;

        //Draw text shadow
        GUI.contentColor = ShadowColor;
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        float offsetScale = 1f;
        for (int i = 0; i < 4; i++)
        {
            var shadowRect = new Rect(rect);
            float xOffset = (i % 2) == 0 ? -offsetScale : offsetScale;
            float yOffset = (i < 2) ? -offsetScale : offsetScale;
            shadowRect.center = new Vector2(rect.center.x + xOffset, rect.center.y + yOffset);
            GUI.Label(shadowRect, text, style);
        }

        for (int i = 0; i < 4; i++)
        {
            var shadowRect = new Rect(rect);
            float xOffset = (i < 2) ? (i % 2) * 2 * offsetScale - offsetScale : 0;
            float yOffset = (i >= 2) ? (i % 2) * 2 * offsetScale - offsetScale : 0;
            shadowRect.center = new Vector2(rect.center.x + xOffset, rect.center.y + yOffset);
            GUI.Label(shadowRect, text, style);
        }

        //Draw foreground without background
        GUI.contentColor = cc;
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        GUI.Label(rect, text, style);

        //Reset colors
        GUI.backgroundColor = bc;
        GUI.contentColor = cc;
    }

    #endregion

}
