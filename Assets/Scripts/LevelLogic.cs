using UnityEngine;
using System.Collections;

public class LevelLogic : Photon.MonoBehaviour 
{
    public Transform PlayerPrefab;
    public string MenuSceneName;

    public GUISkin GUISkin=null;

    private GameObject mPlayer;

    private const int messageWidth = 500;
    private const int messageHeight = 60;

	// Use this for initialization
	void Awake () 
    {
        Debug.Log(string.Format("LevelInitialisation.Awake, PhotonNetwork.isMessageQueueRunning={0}", PhotonNetwork.isMessageQueueRunning));
        PhotonNetwork.isMessageQueueRunning = true;

	}

    void Start()
    {
        if (PlayerPrefab != null)
        {
            mPlayer = (GameObject)PhotonNetwork.Instantiate(PlayerPrefab.name, this.transform.position, this.transform.rotation, 0);
            if (mPlayer != null)
            {
                if (Camera.mainCamera != null)
                {
                    FollowCam camera = Camera.mainCamera.GetComponent<FollowCam>();
                    if (camera != null)
                    {
                        camera.Target = mPlayer;
                    }
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update () 
    {
	
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //Debug.Log(string.Format("OnPhotonSerializeView {0}", name));

//		if (stream.isWriting)
//		{ 
//		}

	}

    void OnGUI()
    {
        GUI.skin = GUISkin;
        GUILayout.BeginArea(new Rect(10, 10, 100, 100));
        if (GUILayout.Button("Quit", GUILayout.Width(50)))
        {
            if (MenuSceneName != null && MenuSceneName != "")
            {
                PhotonNetwork.LeaveRoom();
                Application.LoadLevel(MenuSceneName);
            }

        }
        GUILayout.EndArea();

        if (mPlayer != null)
        {
            if (mPlayer.GetComponent<AvatarLoadingController>().IsLoading)
            {
                GUILayout.BeginArea(new Rect((Screen.width - messageWidth) / 2, (Screen.height - messageHeight) / 2, messageWidth, messageHeight), GUI.skin.box);
                {
                    GUILayout.Label(string.Format("Loading Avatar"), GUI.skin.FindStyle("header"), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                }
                GUILayout.EndArea();
            }
        }

            
    }




    void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerConnected: " + player);
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.LogWarning("OnPlayerDisconneced: " + player);
    }

    void OnLeftRoom()
    {
        Debug.LogWarning("OnLeftRoom (local)");
    }
    void OnReceivedRoomList()
    {
        Debug.LogWarning("OnReceivedRoomList");
    }
    void OnReceivedRoomListUpdate()
    {
        Debug.LogWarning("OnReceivedRoomListUpdate");
    }
    void OnMasterClientSwitched(PhotonPlayer player)
    {
        Debug.LogWarning("OnMasterClientSwitched: " + player);
        if (PhotonNetwork.connected)
        {
            photonView.RPC("SendChatMessage", PhotonNetwork.masterClient, "Hi master! From:" + PhotonNetwork.player);
            photonView.RPC("SendChatMessage", PhotonTargets.All, "WE GOT A NEW MASTER: " + player + "==" + PhotonNetwork.masterClient + " From:" + PhotonNetwork.player);
        }
    }

    void OnConnectedToPhoton()
    {
        Debug.LogWarning("OnConnectedToPhoton");
    }
    void OnDisconnectedFromPhoton()
    {
        Debug.LogWarning("OnDisconnectedFromPhoton");
        //Back to main menu        
        Application.LoadLevel(Application.loadedLevelName);
    }
    void OnFailedToConnectToPhoton()
    {
        Debug.LogWarning("OnFailedToConnectToPhoton");
    }
    void OnPhotonInstantiate()
    {
        Debug.LogWarning("OnPhotonInstantiate");

    }

}
