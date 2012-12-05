using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AvatarLoadingController))]
public class AvatarLoaderNetwork : Photon.MonoBehaviour
{
	private AvatarLoaderUserId mLoaderUserId;
	private bool haveAskedToLoadAvatar = false;
	private string avatarUserId = "";

    void Awake()
    {
		mLoaderUserId = GetComponent<AvatarLoaderUserId>();
    }

    // Use this for initialization
    void Start()
    {
        if(photonView.isMine)
        {
			avatarUserId = VostopiaClient.Authentication.ActiveUser.Id;
        }
    }

	public void Update()
	{
		if (!haveAskedToLoadAvatar && !avatarUserId.Equals(""))
		{
			Debug.Log("Loading Avatar: " + avatarUserId);
			mLoaderUserId.UserId = avatarUserId;
			mLoaderUserId.LoadAvatar();
			haveAskedToLoadAvatar = true;
		}
	}

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            string userId = VostopiaClient.Authentication.ActiveUser.Id;

			//We own this player: send the others our data
            stream.SendNext(userId);
        }
        else
        {
			avatarUserId = (string)stream.ReceiveNext();
        }
    }
}
