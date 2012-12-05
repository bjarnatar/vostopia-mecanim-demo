using UnityEngine;
using System.Collections;

public class Menu : Photon.MonoBehaviour {

    public string[] levelNames;
    public GUISkin GUISkin;

    private Vector2 mRoomListScrollPosition;
    private int mSelectedRoomIndex = 0;
    private RoomInfo mSelectedRoom = null;


    private Vector2 mLevelListScrollPosition;
    private int mSelectedLevelIndex=0;
    private string mSelectedLevelName=null;

    private string mError;

    private const int menuWidth = 400;
    private const int menuHeight = 400;

    private const int messageWidth = 500;
    private const int messageHeight = 60;

    private const int buttonWidth = 150;

    private enum MenuState
    {
        ConnectingToVostopia,
        ConnectingToPhoton,
        DisplayRooms,
        CreateNewRoom,
        ConnectingToRoom,
        Loading,
        Error,

    }

    private MenuState mMenuState;



	// Use this for initialization
	void Start () {

        mMenuState = MenuState.ConnectingToVostopia;

        //PhotonNetwork.ConnectUsingSettings();
	}
	
	// Update is called once per frame
	void Update () 
    {
        switch (mMenuState)
        {
            case MenuState.ConnectingToVostopia:
                UpdateConnectingToVostopia();
                break;
            case MenuState.ConnectingToPhoton:
                UpdateConnectingToPhoton();
                break;
        }
	
	}

    private void OnGUI()
    {

        GUI.skin = GUISkin;
        GUI.depth = 10;
        {

            switch (mMenuState)
            {
                case MenuState.ConnectingToVostopia:
                    OnGUIConnectingToVostopia();
                    break;
                case MenuState.ConnectingToPhoton:
                    OnGUIConnectingToPhoton();
                    break;
                case MenuState.DisplayRooms:
                    OnGUIDisplayRooms();
                    break;
                case MenuState.CreateNewRoom:
                    OnGUICreateNewRoom();
                    break;
                case MenuState.ConnectingToRoom:
                    OnGUIConnectingToRoom();
                    break;
                case MenuState.Loading:
                    OnGUILoading();
                    break;
                case MenuState.Error:
                    OnGUIError();
                    break;
            }
        }
    }

    private void OnGUIConnectingToVostopia()
    {
    }
    private void UpdateConnectingToVostopia()
    {
        if (VostopiaClient.IsAuthenticated)
        {
            mMenuState = MenuState.ConnectingToPhoton;
            if (!PhotonNetwork.connected)
            {
                //only try and connect if we are not already connected,  this will be skipped if we are returning to the menu from a game room.
                PhotonNetwork.ConnectUsingSettings("1.0");
                PhotonNetwork.playerName = VostopiaClient.Authentication.ActiveUser.DisplayName;
            }

        }

    }

    private void OnGUIConnectingToPhoton()
    {
        GUILayout.BeginArea(new Rect((Screen.width - messageWidth) / 2, (Screen.height - messageHeight) / 2, messageWidth, messageHeight), GUI.skin.box);
        {
            GUILayout.Label("Connecting To Photon...", GUI.skin.FindStyle("header"), GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));
        }
        GUILayout.EndArea();
    }

    private void UpdateConnectingToPhoton()
    {
        if (PhotonNetwork.connected)
        {
            mMenuState = MenuState.DisplayRooms;
        }

    }





    private void OnGUIDisplayRooms()
    {
        GUILayout.BeginArea(new Rect((Screen.width - menuWidth) / 2, (Screen.height - menuHeight) / 2, menuWidth, menuHeight), GUI.skin.box);
        {

            GUILayout.Label(string.Format("Select a level to Join"), GUI.skin.FindStyle("header"));

            mSelectedRoom = null;

            mRoomListScrollPosition = GUILayout.BeginScrollView(mRoomListScrollPosition, GUILayout.Height(menuHeight-100));
            {
                RoomInfo[] rooms = PhotonNetwork.GetRoomList();
                if (rooms == null || rooms.Length == 0)
                {
                    GUILayout.Label("No Rooms available");
                }
                else
                {
                    for (int i = 0; i < rooms.Length; i++)
                    {
                        RoomInfo room = rooms[i];
                        bool selected = mSelectedRoomIndex == i;
                        selected = GUILayout.Toggle(selected, string.Format("{0} {1}/{2}", room.name, room.playerCount, room.maxPlayers));
                        if (selected)
                        {
							mSelectedRoom = room;
                            mSelectedRoomIndex = i;
							Debug.Log("Selected room name: " + mSelectedRoom.name + ", index: " + mSelectedRoomIndex);
                        }
                    }
                }
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create New Room", GUILayout.Width(buttonWidth)))
                {
                    mMenuState = MenuState.CreateNewRoom;
                }
                GUI.enabled = mSelectedRoom != null;
                if (GUILayout.Button("Join Room", GUILayout.Width(buttonWidth)))
                {
                    DoJoinRoom(mSelectedRoom);
                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }
    private void OnGUICreateNewRoom()
    {
        GUILayout.BeginArea(new Rect((Screen.width - menuWidth) / 2, (Screen.height - menuHeight) / 2, menuWidth, menuHeight), GUI.skin.box);
        {
            mSelectedLevelName = null;

            GUILayout.Label("Select Level", GUI.skin.FindStyle("header"));
            mLevelListScrollPosition = GUILayout.BeginScrollView(mLevelListScrollPosition, GUILayout.Height(menuHeight-100));
            {
                for (int i = 0; i < levelNames.Length; i++)
                {
                    string levelname = levelNames[i];
                    if (levelname != null && levelname != "")
                    {
                        bool selected = mSelectedLevelIndex == i;
                        bool clicked = GUILayout.Toggle(selected, levelname);
                        if (clicked)
                        {
                            mSelectedLevelIndex = i;
                            mSelectedLevelName = levelname;
                        }

                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (mSelectedLevelName == null)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Create Room", GUILayout.Width(buttonWidth)))
                {
                    DoCreateNewRoom(mSelectedLevelName);
                }
                GUI.enabled = true;

                if (GUILayout.Button("Cancel", GUISkin.FindStyle("btn_red"), GUILayout.Width(buttonWidth)))
                {
                    mMenuState = MenuState.DisplayRooms;
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }
    private void OnGUIConnectingToRoom()
    {
        GUILayout.BeginArea(new Rect((Screen.width - messageWidth) / 2, (Screen.height - messageHeight) / 2, messageWidth, messageHeight), GUI.skin.box);
        {
            GUILayout.Label(string.Format("ConnectingToRoom... {0}", PhotonNetwork.connectionStateDetailed.ToString()), GUI.skin.FindStyle("header"), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
        GUILayout.EndArea();
    }


    private void OnGUILoading()
    {
        GUILayout.BeginArea(new Rect((Screen.width - messageWidth) / 2, (Screen.height - messageHeight) / 2, messageWidth, messageHeight), GUI.skin.box);
        {
            GUILayout.Label("Loading...", GUI.skin.FindStyle("header"), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
        GUILayout.EndArea();
    }


    private void OnGUIError()
    {
        GUILayout.BeginArea(new Rect((Screen.width - messageWidth) / 2, (Screen.height - messageHeight) / 2, messageWidth, messageHeight), GUI.skin.box);
        {
            GUILayout.Label(string.Format("Error: {0}", mError), GUI.skin.FindStyle("header"), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
        GUILayout.EndArea();
    }

    private void DoCreateNewRoom(string levelName)
    {
        Debug.Log("calling CreateRoom");
        PhotonNetwork.CreateRoom(levelName);
        Debug.Log("called CreateRoom");
        mMenuState = MenuState.ConnectingToRoom;
    }

    private void DoJoinRoom(RoomInfo room)
    {
        mSelectedLevelName = room.name;
        Debug.Log("calling CreateRoom");
        PhotonNetwork.JoinRoom(room.name);
        Debug.Log("called CreateRoom");

        mMenuState = MenuState.ConnectingToRoom;
    }

    private void DoError(string error)
    {
        mError = error;
        mMenuState = MenuState.Error;
    }

    // We have two options here: we either joined(by title, list or random) or created a room.
    private void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        if (PhotonNetwork.room != null)
        {
            mMenuState = MenuState.Loading;
            Debug.Log(string.Format("Loading level {0} PhotonNetwork.isMessageQueueRunning = {1}", mSelectedLevelName, PhotonNetwork.isMessageQueueRunning));
            PhotonNetwork.isMessageQueueRunning = false;
            Application.LoadLevel(mSelectedLevelName);
        }
    }

    private void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }



}
