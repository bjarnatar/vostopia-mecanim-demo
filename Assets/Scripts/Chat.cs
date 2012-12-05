using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This simple chat example showcases the use of RPC targets and targetting certain players via RPCs.
/// </summary>
public class Chat : Photon.MonoBehaviour
{

    public List<string> messages = new List<string>();

    private int chatHeight = 140;
    private Vector2 scrollPos = Vector2.zero;
    private string chatInput = "";

    private bool chatActive = false;
    private bool displayChatHint = true;

    private bool skipNextKeyEvent = false;
    private bool setFocus = false;

    public GUISkin GUISkin=null;

    void Awake()
    {
    }

    void OnGUI()
    {
        bool keyIntercepted = false;
        if (Event.current.type == EventType.KeyDown)
        {

            if (skipNextKeyEvent)
            {
                Event.current.Use();
                skipNextKeyEvent = false;
                keyIntercepted = true;
            }
            else
            {
                if (chatActive)
                {
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        SendChat(PhotonTargets.All);
                        Event.current.Use();
                        keyIntercepted = true;
                        skipNextKeyEvent = true;
                    }
                    else if (Event.current.keyCode == KeyCode.Escape)
                    {
                        chatActive = false;
                        Event.current.Use();
                        keyIntercepted = true;
                    }
                }
                else
                {
                    if (Event.current.keyCode == KeyCode.T)
                    {
                        chatActive = true;
                        displayChatHint = false;
                        Event.current.Use();
                        keyIntercepted = true;
                        skipNextKeyEvent = true;
                        setFocus = true;
                    }
                }
            }


        }

        if(! keyIntercepted )
        {
            GUI.skin = GUISkin;
            GUILayout.BeginArea(new Rect(0, Screen.height - chatHeight, Screen.width, chatHeight));
            {
                GUILayout.BeginVertical();
                {
                    //Show scroll list of chat messages
                    scrollPos = GUILayout.BeginScrollView(scrollPos);
                    {
                        GUI.color = Color.black;
                        for (int i = messages.Count - 1; i >= 0; i--)
                        {
                            GUILayout.Label(messages[i]);
                        }
                    }
                    GUILayout.EndScrollView();
                    GUI.color = Color.white;

                    //Chat input
                    if (chatActive)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUI.SetNextControlName("chatText");
                            chatInput = GUILayout.TextArea(chatInput,GUILayout.ExpandWidth(true));


                            if (GUILayout.Button("Send", GUILayout.Width(60)))
                            {
                                SendChat(PhotonTargets.All);
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (setFocus)
                        {
                            GUI.FocusControl("chatText");
                            setFocus = false;
                        }

                    }
                    else
                    {
                        if (displayChatHint)
                        {
                            GUILayout.Label("Press 'T' to Chat");
                        }
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.EndArea();
        }
    }

    private void AddMessage(string text)
    {
        messages.Add(text);
        if (messages.Count > 15)
            messages.RemoveAt(0);
    }


    [RPC]
    void SendChatMessage(string text, PhotonMessageInfo info)
    {
        AddMessage("[" + info.sender + "] " + text);
    }

    void SendChat(PhotonTargets target)
    {
        photonView.RPC("SendChatMessage", target, chatInput);
        chatInput = "";
    }

    void SendChat(PhotonPlayer target)
    {
        chatInput = "[PM] " + chatInput;
        photonView.RPC("SendChatMessage", target, chatInput);
        chatInput = "";
    }
}
