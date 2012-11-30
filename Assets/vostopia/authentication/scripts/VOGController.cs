using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VOGController : MonoBehaviour
{
    public enum TransitionDirection
    {
        Forward,
        Backward,
        Over
    }

    public class Transition
    {
        public TransitionDirection Direction;
        public float Progress;
        public VOGStateBase FromState;
        public VOGStateBase ToState;
    }

    public class VisibleState
    {
        public VOGStateBase State;
        public Matrix4x4 Position;
        public float TransitionProgress;
    }

    [System.Serializable]
    public class ScreenSize
    {
        public int MinimumResolution = 320;
        public float ScreenMultiplier = 1; 
        public Texture[] Textures = new Texture[] {};
        public GUISkin Skin;
    }

    public float TransitionDuration = 0.2f;
    public Texture2D[] SpinnerTextures;
    public int SpinnerFrameRate = 10;
    public AudioClip TransitionAudioClip;
    public float TransitionAudioVolume = 1;

    public ScreenSize[] ScreenSizes;
    private int CurrentScreenWidth = 0;
    [HideInInspector] public ScreenSize CurrentScreenSize = null;


    [HideInInspector] public List<VisibleState> VisibleStates = new List<VisibleState>();
    [HideInInspector] public Stack<VOGStateBase> StateStack = new Stack<VOGStateBase>();

    private VOGStateMessageDialog MessageDialogState = null;

    #region Enable/Disable Input

    private int _InputEnabled;
    public bool InputEnabled(VOGStateBase state)
    {
        return (_InputEnabled >= 0) && (state == StateStack.Peek());
    }

    public void EnableInput()
    {
        _InputEnabled++;
    }

    public void DisableInput()
    {
        _InputEnabled--;
    }

    #endregion

    #region Transitions

    public void StartTransition(VOGStateBase to, object data = null)
    {
        VOGStateBase from = null;
        from = StateStack.Count > 0 ? StateStack.Peek() : null;
        StateStack.Push(to);
        StartCoroutine(RunTransition(from, to, TransitionDirection.Forward, data));
    }

    public void StartBackTransition()
    {
        VOGStateBase from = StateStack.Pop();
        VOGStateBase to = StateStack.Count > 0 ? StateStack.Peek() : null;

        //if to-state is already visible, don't transition it
        if (FindVisibleState(to) != null)
        {
            to = null;
        }

        StartCoroutine(RunTransition(from, to, TransitionDirection.Backward, null));
    }

    public void StartOverTransition(VOGStateBase to, object data = null)
    {
        StateStack.Push(to);
        StartCoroutine(RunTransition(null, to, TransitionDirection.Over, data));
    }

    private bool _IsTransition = false;
    public bool IsTransition()
    {
        return _IsTransition;
    }

    private VisibleState FindVisibleState(VOGStateBase state)
    {
        return VisibleStates.Find(s => s.State == state);
    }

    private VisibleState CreateVisibleState(VOGStateBase state)
    {
        VisibleState visibleState = VisibleStates.Find(s => s.State == state);
        if (visibleState == null)
        {
            visibleState = new VisibleState()
            {
                State = state,
            };
            if (state != null)
            {
                VisibleStates.Add(visibleState);
            }
        }
        return visibleState;
    }

    AudioListener Listener = null;
    
    private IEnumerator RunTransition(VOGStateBase from, VOGStateBase to, TransitionDirection direction, object data)
    {
        if (Listener == null)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                cam = (Camera)GameObject.FindObjectOfType(typeof(Camera));
            }
            Listener = cam.GetComponent<AudioListener>();
            if (Listener == null)
            {
                Listener = cam.gameObject.AddComponent<AudioListener>();
            }
        }
        if (Listener != null)
        {
            var source = Listener.audio;
            if (source == null)
            {
                source = Listener.gameObject.AddComponent<AudioSource>();
            }
            source.PlayOneShot(TransitionAudioClip, TransitionAudioVolume);
        }

        //Debug.Log("From " + from + " to " + to);
        VisibleState visibleFrom = CreateVisibleState(from);
        VisibleState visibleTo = CreateVisibleState(to);

        //enable to-state
        if (to != null)
        {
            try
            {
                if (data != null)
                {
                    to.OnDataChanged(data);
                }
                to.OnStateEnable(this);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error while disabling state, " + ex.ToString());
            }
        }

        System.Action<float> updatePosition = (prog) =>
        {
            visibleFrom.TransitionProgress = visibleTo.TransitionProgress = prog;

            if (direction == TransitionDirection.Forward)
            {
                visibleFrom.Position = Matrix4x4.TRS(new Vector3(-Screen.width * prog, 0), Quaternion.identity, Vector3.one);
                visibleTo.Position = Matrix4x4.TRS(new Vector3(Screen.width * (1 - prog), 0), Quaternion.identity, Vector3.one);
            }
            else if (direction == TransitionDirection.Backward)
            {
                visibleFrom.Position = Matrix4x4.TRS(new Vector3(Screen.width * prog, 0), Quaternion.identity, Vector3.one);
                visibleTo.Position = Matrix4x4.TRS(new Vector3(-Screen.width * (1 - prog), 0), Quaternion.identity, Vector3.one);
            }
            else if (direction == TransitionDirection.Over)
            {
                visibleFrom.Position = Matrix4x4.TRS(new Vector3(0, Screen.width * prog, 0), Quaternion.identity, Vector3.one);
                visibleTo.Position = Matrix4x4.TRS(new Vector3(0, -Screen.width * (1 - prog), 0), Quaternion.identity, Vector3.one);
            }
        };

        //Run Transition
        _IsTransition = true;
        DisableInput();

        float startTime = Time.realtimeSinceStartup;
        float progress = 0;
        while (Time.realtimeSinceStartup - startTime < TransitionDuration)
        {
            progress = (Time.realtimeSinceStartup - startTime) / TransitionDuration;
            updatePosition(progress);

            yield return null;
        }

        //Force last frame with progress = 1
        updatePosition(1);
        yield return null;

        EnableInput();
        _IsTransition = false;

        //Disable from state
        if (from != null)
        {
            try
            {
                from.OnStateDisable(this);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error while disabling state, " + ex.ToString());
            }
            VisibleStates.Remove(visibleFrom);
        }
    }

    #endregion

    #region Unity Events

    public virtual void Start()
    {
        UpdateScreenSize();
        CurrentScreenWidth = Screen.width;
        if (CurrentScreenSize == null)
        {
            CurrentScreenSize = ScreenSizes[0];
        }
    }

    public virtual void Update()
    {
        if (CurrentScreenWidth != Screen.width)
        {
            UpdateScreenSize();
            CurrentScreenWidth = Screen.width;
        }
    }

    public virtual void OnGUI()
    {
        GUISkin oldSkin = GUI.skin;
        GUI.skin = CurrentScreenSize.Skin;

        Matrix4x4 oldMat = GUI.matrix;
        foreach (VisibleState state in VisibleStates.ToArray())
        {
            GUI.matrix = state.Position;
            state.State.OnBeforeDrawGui(this);
            state.State.OnDrawGui(this);
        }
        GUI.matrix = oldMat;

        VOGStateBase currentState = StateStack.Count > 0 ? StateStack.Peek() : null;
        if (_InputEnabled < 0 && !IsTransition() && currentState != null)
        {
            Color c = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
            var style = GUI.skin.FindStyle("background-color");
            GUI.Label(new Rect(-Screen.width, -Screen.height, 3 * Screen.width, 3 * Screen.height), "", style);
            GUI.backgroundColor = c;

            if (SpinnerTextures != null && SpinnerTextures.Length > 0)
            {
                style = GUI.skin.FindStyle("image");
                Texture2D tex = SpinnerTextures[(int)(Time.realtimeSinceStartup * SpinnerFrameRate) % SpinnerTextures.Length];
                GUI.DrawTexture(new Rect((Screen.width - tex.width) / 2, (Screen.height - tex.height) / 2, tex.width, tex.height), tex);
            }
        }

        GUI.skin = oldSkin;
    }
    #endregion

    #region Message Dialog

    public void ShowMessageDialog(string header, string body, System.Action callback)
    {
        if (MessageDialogState == null)
        {
            GameObject go = new GameObject();
            go.transform.parent = gameObject.transform;
            MessageDialogState = go.AddComponent<VOGStateMessageDialog>();
        }
        var data = new VOGStateMessageDialog.DialogData()
        {
            Header = header,
            Body = body,
            Callback = callback,
        };
        StartOverTransition(MessageDialogState, data);
    }

    #endregion

    #region Screen Size

    private void UpdateScreenSize()
    {
        ScreenSize newScreenSize = null;
        int screenSize = Mathf.Min(Screen.height, Screen.width);
        foreach (ScreenSize scrn in ScreenSizes.Reverse())
        {
            if (screenSize >= scrn.MinimumResolution)
            {
                newScreenSize = scrn;
                break;
            }
        }

        if (newScreenSize != null)
        {
            CurrentScreenSize = newScreenSize;
        }
    }

    public Texture GetTexture(string textureName)
    {
        return CurrentScreenSize.Textures.First(tex => tex.name == textureName);
    }



    #endregion
}


